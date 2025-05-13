
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;


public class MapManager : MonoBehaviour
{
    #region 成员变量
    public static MapManager Instance { get; private set; }

        #region 常数 与 类型枚举
    public const int MAP_SIZE = 64;
    public Vector3Int[] DIRECTIONS = {new Vector3Int(0, 1, 0), new Vector3Int(1, 0, 0), new Vector3Int(0, -1, 0), new Vector3Int(-1, 0, 0)};
    public Vector3Int[] DIAGONAL_DIRECTIONS = {new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0), new Vector3Int(-1, 1, 0), new Vector3Int(-1, -1, 0)};
    public const int DEBUG_MAX_PILE_NUM = 100;//TODO: 区分不同物品堆叠数量

    public enum tileTypes{
        grass,path,water,tree,farm,total
    }
        #endregion

    public class MapData{
        public Vector3Int position;
        public TileBase texture;
        public tileTypes type;

        public bool has_print = false;
        public bool has_building = false;
        public bool has_item = false; public ItemInstanceManager.ItemInstance item = null;
        public bool has_pawn = false; //是否有pawn,用于pawn移动判定
        //assert: has_print/has_building -> has_item
        //assert: has_item -> item != null

        //用于维护寻路点阵图
        public bool can_walk = true;
        public bool can_build = true;
        public bool can_plant = true;
        //TODO: 拓展为List<bool> cans + enum canTypes{walk,build,plant}

        public float fertility = 1.0f;
        public float humidity = 0.0f;
        public float light = 1.0f;

        public float walk_speed = 1.0f;
    }
    
    public Tilemap landTilemap;

    public GameObject content;

        #region 数据矩阵
    public bool[,] walkVectors = new bool[MAP_SIZE,MAP_SIZE];

    //TODO: 可维护一个bool二位数组，用于优化物品放置时的检测
    //public bool[,] set_material_state = new bool[MAP_SIZE, MAP_SIZE];

    public TileBase[] tiles = new TileBase[(int)tileTypes.total];
    public MapData[,] mapDatas = new MapData[MAP_SIZE, MAP_SIZE];
        #endregion

    #endregion

    void SetWalkableState(Vector3Int pos, bool can_walk){
        if(!IsInBoard(pos)) return;

        walkVectors[pos.x, pos.y] = can_walk;
        mapDatas[pos.x, pos.y].can_walk = can_walk;
    }
    void SetWalkableState(MapData mapData, bool can_walk){
        int x = mapData.position.x;int y = mapData.position.y;

        walkVectors[x, y] = can_walk;
        mapDatas[x, y].can_walk = can_walk;
    }
    public void SetPawnState(Vector3Int pos, bool hasPawn){
        if(!IsInBoard(pos)) return;

        mapDatas[pos.x, pos.y].has_pawn = hasPawn;
    }

    Dictionary<BuildManager.BuildingType, Action<MapData, BuildManager.Building>> buildSetActions = new Dictionary<BuildManager.BuildingType, Action<MapData, BuildManager.Building>>{
        {BuildManager.BuildingType.Dev, (data, building) => Instance.SetTileDev(data, building)},
        {BuildManager.BuildingType.Wall, (data, building) => Instance.SetTilePrint(data, building)},
        {BuildManager.BuildingType.Farm, (data, building) => Instance.SetTileFarm(data, building)}
    };

    Dictionary<BuildManager.BuildingType, Action<MapData, BuildManager.Building>> buildTaskActions = new Dictionary<BuildManager.BuildingType, Action<MapData, BuildManager.Building>>{
        {BuildManager.BuildingType.Dev, (data, building) => Instance.SetTileDev(data, building)},
        {BuildManager.BuildingType.Wall, (data, building) => Instance.AddBuildTask(data, building)},
        {BuildManager.BuildingType.Farm, (data, building) => Instance.AddBuildTask(data, building)},
    };

    //或许有必要分开？
    // void AddWallBuildTask(MapData data, BuildManager.Building building){
    //     TaskManager.Instance.AddTask(data.position, TaskManager.TaskTypes.Build, building.id, 1);
    // }

    void AddBuildTask(MapData data, BuildManager.Building building){
        data.has_print = true;
        SetTilePrint(data, building);

        TaskManager.Instance.AddTask(data.position, TaskManager.TaskTypes.Build, building.id, 1);
    }

    public void SetTileDev(MapData data,BuildManager.Building building){
        int id=building.id;

        data.type = (tileTypes)id;
        data.texture = tiles[id];

        data.can_build=building.can_build;

        SetWalkableState(data, building.can_walk);
        
        data.can_plant=building.can_plant;

        data.has_print = false;
        data.has_building = false;
        data.has_item = false;

        if(data.item != null){
            ItemInstanceManager.ItemInstance to_destroy = data.item;
            data.item = null;
            ItemInstanceManager.Instance.DestroyItem(to_destroy, ItemInstanceManager.DestroyMode.RemainAll);
        }

        landTilemap.SetTile(data.position, data.texture);
    }
    public void SetTilePrint(MapData data, BuildManager.Building building){

        data.has_print = true;
        data.has_item = true;

        SetWalkableState(data, true);//临时

        data.can_build = false;
        data.can_plant = false;

        data.item = ItemInstanceManager.Instance.SpawnItem(data.position, building.id, ItemInstanceManager.ItemInstanceType.PrintInstance);
    }
    public void SetTileFarm(MapData data, BuildManager.Building building){
    
        if(building.type != BuildManager.BuildingType.Farm){
            Debug.Log("Error: SetTileFarm传入的建筑类型错误！");
            return;
        }
        
        // if(!data.can_plant){
        //     Debug.Log("此处无法种植!");
        //     return;
        // }

        data.type = tileTypes.farm;
        data.texture = tiles[(int)tileTypes.farm];

        data.has_print = false;
        data.has_building = true;
        data.has_item = true;

        SetWalkableState(data, building.can_walk);
        data.can_build = building.can_build;
        data.can_plant = building.can_plant;

        //临时：在这里就删掉
        Destroy(data.item.instance);
        data.item = ItemInstanceManager.Instance.SpawnItem(data.position, building.id, ItemInstanceManager.ItemInstanceType.BuildingInstance);
    }

    #region 初始化
    public void GenerateMapData(){
        for (int x = 0; x < MAP_SIZE; x++){
            for (int y = 0; y < MAP_SIZE; y++){
                mapDatas[x, y] = new MapData();
                mapDatas[x, y].type = tileTypes.grass;
                mapDatas[x, y].texture = tiles[(int)tileTypes.grass];
                mapDatas[x, y].position = new Vector3Int(x, y, 0);

                mapDatas[x, y].has_pawn = false;

                SetTileDev(mapDatas[x, y], BuildManager.Instance.GetBuilding(0));//草地
            }
        }
    }
    public void GenerateMapTiles(){
        // 生成地图瓦片
        for (int x = 0; x < MAP_SIZE; x++){
            for (int y = 0; y < MAP_SIZE; y++){
                landTilemap.SetTile(new Vector3Int(x, y, 0), mapDatas[x, y].texture);
            }
        }
    }
    public void GenerateBoolVectors(){
        for (int x = 0; x < MAP_SIZE; x++){
            for (int y = 0; y < MAP_SIZE; y++){
                walkVectors[x, y] = mapDatas[x, y].can_walk;
            }
        }
    }
    void Awake(){
        if(Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    void Start(){
        GenerateMapData();
        GenerateMapTiles();
        GenerateBoolVectors();
    }

    #endregion


    #region 对外方法

    public void BuildByPlayer(Vector3Int cellPos, BuildManager.Building building){
        TileBase clickedTile = landTilemap.GetTile(cellPos);
        MapData clickedData = mapDatas[cellPos.x, cellPos.y];

        if (clickedTile != null){

            //UIManager.Instance.DebugTextAdd("点击到了 Tile: " + cellPos);
            if(building != null){
                UIManager.Instance.DebugTextAdd("放置建筑: " + building.name);

                //非Dev建筑占地特判
                if(building.type != BuildManager.BuildingType.Dev && ( !clickedData.can_build )){
                    Debug.Log("此处已有建筑/蓝图，无法放置");
                    return;
                }

                //根据buildingType更新数据
                if(buildTaskActions.TryGetValue(building.type, out Action<MapData, BuildManager.Building> action))
                    action(clickedData, building);
                else
                    Debug.Log("未定义的建筑类型: " + building.type);

                //Dev：landtilemap更新贴图
                if(building.type == BuildManager.BuildingType.Dev || building.type == BuildManager.BuildingType.Farm){
                    landTilemap.SetTile(cellPos, clickedData.texture);
                }
            }
        }
    }

    /// <summary>
    /// 将material_list中的物品以pos为中心放置到地图上。直到地图上无空格或material_list中的物品放置完毕。
    /// </summary>
    /// <param name="pos">要放置的中心位置（包含越界检测）</param>
    /// <param name="material_list">需要放置的(id,num)列表</param>
    /// <returns></returns>
    public int SetMaterial(Vector3Int pos, List<KeyValuePair<int, int>> material_list){
        if(!IsInBoard(pos)){
            Debug.Log("Error: SetMaterial越界");
            return -1;
        }

        bool[,] visited = new bool[MAP_SIZE, MAP_SIZE];
        for(int i = 0; i < MAP_SIZE; i++){
            for(int j = 0; j < MAP_SIZE; j++){
                visited[i, j] = false;
            }
        }

        Queue<Vector3Int> checklist = new Queue<Vector3Int>();
        checklist.Enqueue(pos);

        while(checklist.Count > 0 && material_list.Count > 0){
            Vector3Int cur_check_pos = checklist.Dequeue();
            visited[cur_check_pos.x, cur_check_pos.y] = true;

            foreach(Vector3Int dir in DIRECTIONS){
                Vector3Int next_pos = cur_check_pos + dir;
                if(IsInBoard(next_pos) && !visited[next_pos.x, next_pos.y]){
                    checklist.Enqueue(next_pos);
                }
            }

            if(mapDatas[cur_check_pos.x, cur_check_pos.y].has_item){
                ItemInstanceManager.ItemInstance the_instance = mapDatas[cur_check_pos.x, cur_check_pos.y].item;
                if(the_instance is not ItemInstanceManager.MaterialInstance)
                    continue;
                else
                {   
                    ItemInstanceManager.MaterialInstance the_material_instance = the_instance as ItemInstanceManager.MaterialInstance;

                    if(material_list.Any(kpv => kpv.Key == the_material_instance.item_id)){
                        
                        
                        int index = material_list.FindIndex(kvp => kvp.Key == the_material_instance.item_id);
                        KeyValuePair<int, int> result = material_list[index];

                        int unload_material_num = Math.Min(result.Value, DEBUG_MAX_PILE_NUM - the_material_instance.GetAmount());
                        
                        int reformed_amount = the_material_instance.GetAmount() + unload_material_num;

                        the_material_instance.SetAmount(reformed_amount);
                        //the_material_instance.amount += unload_material_num;

                        int material_num = result.Value - unload_material_num;
                        if (material_num == 0)
                            material_list.RemoveAt(index);
                        else
                            material_list[index] = new KeyValuePair<int, int>(result.Key, material_num);
                    }
                    else continue;
                }
            }
            else{
                int unload_material_num = Math.Min(material_list[0].Value, DEBUG_MAX_PILE_NUM);
                mapDatas[cur_check_pos.x, cur_check_pos.y].has_item = true;
                mapDatas[cur_check_pos.x, cur_check_pos.y].item = ItemInstanceManager.Instance.SpawnItem(
                    cur_check_pos, material_list[0].Key, ItemInstanceManager.ItemInstanceType.MaterialInstance, unload_material_num);
                
                int material_num = material_list[0].Value - unload_material_num;
                if (material_num == 0)
                    material_list.RemoveAt(0);
                else
                    material_list[0] = new KeyValuePair<int, int>(material_list[0].Key, material_num);
            }
        }
        
        return 1;
    }
    
    /// <summary>
    /// 寻路算法，使用A*算法实现。返回路径点列表。支持八向寻路。
    /// </summary>
    /// <returns>基于start由近到远的可直向同行的路径点集。若为空说明不可通行或起点与重点重合。</returns>
    public List<Vector3Int> FindPath(Vector3Int start, Vector3Int end){
        List<Vector3Int> path = new List<Vector3Int>();

        if(start == end) return path;

        // #if NET6_0_OR_GREATER
        //     PriorityQueue<Vector3Int, int> check_q = new PriorityQueue<Vector3Int, int>();
        // #else
            MyPriorityQueue<Vector3Int, int> check_q = new MyPriorityQueue<Vector3Int, int>();
        //#endif

        check_q.Enqueue(start, Math.Abs(start.x - end.x) + Math.Abs(start.y - end.y));

        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        Dictionary<Vector3Int, float> costTowards = new Dictionary<Vector3Int, float>();

        costTowards[start] = 0.0f;

        while(!check_q.Empty()){
            Vector3Int current = check_q.Dequeue();

            if(current == end){

                Vector3Int cur_stand;
                Vector3Int last_stand = end;
                Vector3Int dir=Vector3Int.zero;

                while(cameFrom.ContainsKey(last_stand)){

                    cur_stand = cameFrom[last_stand];
                    Vector3Int tmp_dir = cur_stand - last_stand;

                    if(tmp_dir != dir){
                        UIManager.Instance.DebugTextAdd("路径点: " + last_stand);
                        path.Add(last_stand);
                        dir = tmp_dir;
                    }
                    else{
                        UIManager.Instance.DebugTextAdd("路径点: " + last_stand + "(被压缩)");
                    }

                    if(cur_stand == start) {
                        current = cur_stand;
                        break;
                    }

                    last_stand = cur_stand;
                }
                
                path.Reverse();

                if(current != start){
                    Debug.Log("Error: 生成路径失败，寻路算法道路不连通。");
                    return path;
                }

                return path;
            }

            foreach(Vector3Int dir in DIRECTIONS){
                Vector3Int next = current + dir;
                if(!IsInBoard(next) || !IsWalkable(next)) continue;

                float newCost = costTowards[current] + 1.0f;

                if(!costTowards.ContainsKey(next) || newCost < costTowards[next]){
                    costTowards[next] = newCost;
                    int priority = Math.Abs(next.x - end.x) + Math.Abs(next.y - end.y);
                    check_q.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }

            foreach(Vector3Int dir in DIAGONAL_DIRECTIONS){
                Vector3Int next = current + dir;
                if(!IsInBoard(next) || !IsWalkable(next)) continue;
                if(!DiagonalCrossable(current,dir)) continue;

                float newCost = costTowards[current] + 1.4f; //对角线代价
                if(!costTowards.ContainsKey(next) || newCost < costTowards[next]){
                    costTowards[next] = newCost;
                    int priority = Math.Abs(next.x - end.x) + Math.Abs(next.y - end.y);
                    check_q.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        return path;
    }

    #endregion

    #region 判定接口

    public bool IsInBoard(Vector3Int pos){
        if(pos.x >= 0 && pos.x < MAP_SIZE && pos.y >= 0 && pos.y < MAP_SIZE)
            return pos.x >= 0 && pos.x < MAP_SIZE && pos.y >= 0 && pos.y < MAP_SIZE;
        else{ 
            Debug.Log("Error: IsInBoard越界, 别再用Vector3了！");
            return false;
        }
    }

    //判定对角线是否可通行
    private bool DiagonalCrossable(Vector3Int st, Vector3Int dir){
        Vector3Int one = st + new Vector3Int(dir.x, 0, 0);
        Vector3Int the_other = st + new Vector3Int(0, dir.y, 0);
        if(!IsInBoard(one) || !IsInBoard(the_other)) return false;
        if(!IsWalkable(one) && !IsWalkable(the_other)) return false;

        return true;
    }

    //当玩家指定移动至某地块时，获取该地块的可通行情况
    public bool IsWalkable(Vector3Int pos){
        if(!IsInBoard(pos)) return false;
        return mapDatas[pos.x, pos.y].can_walk;
    }
    
    //检测某个格子上是否有pawn
    public bool HasPawnAt(Vector3Int pos){
        if(!IsInBoard(pos)) return false;
        return mapDatas[pos.x, pos.y].has_pawn;
    }
    
    //检测某个格子上是否有item
    public bool HasItemAt(Vector3Int pos){
        if(!IsInBoard(pos)) return false;
        return mapDatas[pos.x, pos.y].has_item;
    }
    
    //获取某个地格的所有MapData信息
    public MapData GetMapData(Vector3Int pos){
        if(!IsInBoard(pos)) {
            Debug.Log("Error: GetMapData越界");
            return null;
        }
        return mapDatas[pos.x, pos.y];
    }

    //获取世界坐标对应的格子坐标
    public Vector3Int GetCellPosFromWorld(Vector3 worldPos){
        return landTilemap.WorldToCell(worldPos);
    }
    
    //获取某个地块的移速
    public float GetWalkSpeed(Vector3Int pos){
        if(!IsInBoard(pos)) return 1.0f;
        return mapDatas[pos.x, pos.y].walk_speed;
    }

    public GameObject GetItem(Vector3Int pos){
        if(!IsInBoard(pos)) return null;
        if(!mapDatas[pos.x, pos.y].has_item) return null;
        return mapDatas[pos.x, pos.y].item.instance;
    }
    #endregion
}
