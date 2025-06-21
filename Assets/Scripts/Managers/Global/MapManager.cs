
using UnityEngine;
using UnityEngine.Tilemaps;

using System.Collections.Generic;
using System;

using System.Linq;

public class MapManager : MonoBehaviour
{
    #region 成员变量
    public static MapManager Instance { get; private set; }

        #region 常数 与 类型枚举
    public const int MAP_SIZE = 64;
    public Vector3Int[] DIRECTIONS = {new Vector3Int(0, 1, 0), new Vector3Int(1, 0, 0), new Vector3Int(0, -1, 0), new Vector3Int(-1, 0, 0)};
    public Vector3Int[] DIAGONAL_DIRECTIONS = {new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0), new Vector3Int(-1, 1, 0), new Vector3Int(-1, -1, 0)};
    public const int DEBUG_MAX_PILE_NUM = 100;//TODO: 区分不同物品堆叠数量

    const int OFFSET_MIN = -1000000;
    const int OFFSET_MAX = 1000000;

    public enum tileTypes
    {
        grass, path, water, tree, rock, farm, total
    }
    public enum landformTypes{
        waterland, grassland, treegrassland, rockland, total
    }
    #endregion

    public class MapData
    {
        public Vector3Int position;
        public TileBase texture;
        public tileTypes type;

        #region 信号位
        //是否被instance占用
        public bool has_print = false;
        public bool has_building = false;
        public bool has_item = false; public ItemInstanceManager.ItemInstance item = null;
        //是否有pawn,用于pawn移动判定
        public bool has_pawn = false;
        public bool will_has_pawn = false;

        //assert: has_print/has_building -> has_item
        //assert: has_item -> item != null

        //用于维护寻路点阵图
        public bool can_walk = true;
        public bool can_build = true;
        public bool can_plant = true;
        //TODO: 拓展为List<bool> cans + enum canTypes{walk,build,plant}
        #endregion

        #region 交互参数
        public float fertility = 1.0f;
        public float humidity = 0.0f;
        public float light = 1.0f;

        public float walk_speed = 1.0f;
        #endregion
    }

    public void SetMapDataItem(ItemInstanceManager.ItemInstance item, Vector3Int pos){
        int x = pos.x; int y = pos.y;
        if (item.type != ItemInstanceManager.ItemInstanceType.BuildingInstance)
        {
            //Debug.Log("Error: SetMapDataItem传入的物品类型错误！");
            mapDatas[x, y].item = item;
            mapDatas[x, y].has_item = true;
            // Debug.Log("SetMapDataItem: " + item.id + " at " + pos);
            // Debug.Log(item.id);
            return;
        }
        else
        {
            //todo: 处理建筑物的item
            return;
        }
    }
    //暂时在卸载函数时调用
    public void DeleteMapDataItem(Vector3Int pos)
    {
        int x = pos.x; int y = pos.y;
        mapDatas[x, y].has_item = false;
        mapDatas[x, y].item = null;
    }

    public void Setmaterialamount(Vector3Int pos, int amount){
        int x = pos.x; int y = pos.y;
        if (mapDatas[x, y].item != null)
        {
            if (mapDatas[x, y].item is ItemInstanceManager.MaterialInstance material)
            {
                material.SetAmount(amount);
            }
        }
    }

    public Tilemap landTilemap;
    public GameObject content;

        #region 柏林噪声地形生成相关参数
    public bool random_seed = true;
    public int seed;

    public float lac = 0.075f; //个人测试表现良好的lac值

    public float min_value = 21474836;
    public float max_value = -21474836;

    [System.Serializable]
    class LandformRange
    {
        public LandformRange(float min, float max, landformTypes type)
        {
            min_value = min;
            max_value = max;
            landform_type = type;
        }
        public float min_value;
        public float max_value;
        public landformTypes landform_type;

        public bool InRange(float index) { return index < max_value && index >= min_value; }
    }
    [SerializeField]
    LandformRange[] Landforms = new LandformRange[(int)landformTypes.total];

        #endregion

    #region 数据矩阵
    public bool[,] walkVectors = new bool[MAP_SIZE, MAP_SIZE];

    //TODO: 可维护一个bool二位数组，用于优化物品放置时的检测
    //public bool[,] set_material_state = new bool[MAP_SIZE, MAP_SIZE];

    public TileBase[] tiles = new TileBase[(int)tileTypes.total];
    public float[,] landformDatas = new float[MAP_SIZE, MAP_SIZE];
    public MapData[,] mapDatas = new MapData[MAP_SIZE, MAP_SIZE];
    #endregion

    #endregion

    void SetWalkableState(Vector3Int pos, bool can_walk)
    {
        if (!IsInBoard(pos)) return;

        walkVectors[pos.x, pos.y] = can_walk;
        mapDatas[pos.x, pos.y].can_walk = can_walk;
    }
    void SetWalkableState(MapData mapData, bool can_walk)
    {
        int x = mapData.position.x; int y = mapData.position.y;

        walkVectors[x, y] = can_walk;
        mapDatas[x, y].can_walk = can_walk;
    }
    public void SetPawnState(Vector3Int pos, bool hasPawn){
        if(!IsInBoard(pos)) return;
        mapDatas[pos.x, pos.y].has_pawn = hasPawn;
    }
    public void SetWillPawnState(Vector3Int pos, bool willHasPawn){
        if(!IsInBoard(pos)) return;
        mapDatas[pos.x, pos.y].will_has_pawn = willHasPawn;
    }

    //建造任务完成->更新地图数据
    Dictionary<BuildingType, Action<MapData, Building>> buildSetActions = new Dictionary<BuildingType, Action<MapData, Building>>{
        {BuildingType.Dev, (data, building) => Instance.SetTileDev(data, building)},
        {BuildingType.Wall, (data, building) => Instance.SetTilePrint(data, building)},
        {BuildingType.Farm, (data, building) => Instance.SetTileFarm(data, building)}
    };
    //玩家指令->添加建造任务
    Dictionary<BuildingType, Action<MapData, Building>> buildTaskActions = new Dictionary<BuildingType, Action<MapData, Building>>{
        {BuildingType.Dev, (data, building) => Instance.SetTileDev(data, building)},
        {BuildingType.Wall, (data, building) => Instance.AddBuildTask(data, building)},
        {BuildingType.Farm, (data, building) => Instance.AddBuildTask(data, building)},
        {BuildingType.Storage, (data, building) => Instance.AddBuildTask(data, building)},
    };

    public void SethasitemState(MapData mapData, bool hasItem){
        int x = mapData.position.x;int y = mapData.position.y;

        //walkVectors[x, y] = hasItem;
        mapDatas[x, y].has_item = hasItem;
    }

    void AddBuildTask(MapData data, Building building)
    {
        data.has_print = true;
        SetTilePrint(data, building);

        TaskManager.Instance.AddTask(data.position, TaskManager.TaskTypes.Build, building.id, 1);
    }

    public void SetTileDev(MapData data,Building building){
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
            ItemInstanceManager.Instance.DestroyItem(to_destroy, ItemInstanceManager.DestroyMode.RemainNone);
        }

        landTilemap.SetTile(data.position, data.texture);
    }
    public void SetTilePrint(MapData data, Building building){

        data.has_print = true;
        data.has_item = true;

        SetWalkableState(data, true);//临时

        data.can_build = false;
        data.can_plant = false;

        data.item = ItemInstanceManager.Instance.SpawnItem(data.position, building.id, ItemInstanceManager.ItemInstanceType.PrintInstance);
    }

    public void SetTileBuild(MapData data, Building building)
    {

        data.has_print = true;
        data.has_item = true;

        SetWalkableState(data, building.can_walk);
        data.can_build = building.can_build;
        data.can_plant = building.can_plant;

        data.item = ItemInstanceManager.Instance.SpawnItem(data.position, building.id, ItemInstanceManager.ItemInstanceType.BuildingInstance);
    }
    public void SetTileFarm(MapData data, Building building)
    {

        if (building.type != BuildingType.Farm)
        {
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
    public void LandformDataNormalize(int x, int y){
        landformDatas[x, y] = Mathf.InverseLerp(min_value, max_value, landformDatas[x, y]);
        return;
    }
    public void GenerateMapData()
    {
        if (random_seed)
            seed = System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000;
        else seed = 0;
        UnityEngine.Random.InitState(seed);

        float random_offset = UnityEngine.Random.Range(OFFSET_MIN, OFFSET_MAX);

        for (int x = 0; x < MAP_SIZE; x++)
        {
            for (int y = 0; y < MAP_SIZE; y++)
            {
                landformDatas[x, y] = Mathf.PerlinNoise(x * lac + random_offset, y * lac + random_offset);
                mapDatas[x, y] = new MapData();

                if (landformDatas[x, y] > max_value) max_value = landformDatas[x, y];
                if (landformDatas[x, y] < min_value) min_value = landformDatas[x, y];
            }
        }
    }
    public void GenerateMapTiles()
    {
        // 生成地图瓦片
        for (int x = 0; x < MAP_SIZE; x++)
        {
            for (int y = 0; y < MAP_SIZE; y++)
            {
                LandformDataNormalize(x, y);

                foreach (LandformRange landform in Landforms)
                {
                    if (landform.InRange(landformDatas[x, y]))
                    {
                        //TODO: 减少复用
                        switch (landform.landform_type)
                        {
                            case landformTypes.waterland:
                                mapDatas[x, y].type = tileTypes.water;
                                mapDatas[x, y].texture = tiles[(int)tileTypes.water];
                                mapDatas[x, y].position = new Vector3Int(x, y, 0);

                                mapDatas[x, y].has_pawn = false;

                                SetTileDev(mapDatas[x, y], BuildManager.Instance.GetBuilding(2));
                                landTilemap.SetTile(new Vector3Int(x, y, 0), mapDatas[x, y].texture);
                                break;
                            case landformTypes.grassland:
                                mapDatas[x, y].type = tileTypes.grass;
                                mapDatas[x, y].texture = tiles[(int)tileTypes.grass];
                                mapDatas[x, y].position = new Vector3Int(x, y, 0);

                                mapDatas[x, y].has_pawn = false;

                                SetTileDev(mapDatas[x, y], BuildManager.Instance.GetBuilding(0));
                                landTilemap.SetTile(new Vector3Int(x, y, 0), mapDatas[x, y].texture);
                                break;
                            case landformTypes.treegrassland:
                                mapDatas[x, y].type = tileTypes.grass;
                                mapDatas[x, y].texture = tiles[(int)tileTypes.grass];
                                mapDatas[x, y].position = new Vector3Int(x, y, 0);

                                mapDatas[x, y].has_pawn = false;

                                SetTileDev(mapDatas[x, y], BuildManager.Instance.GetBuilding(0));
                                landTilemap.SetTile(new Vector3Int(x, y, 0), mapDatas[x, y].texture);

                                mapDatas[x, y].item = ItemInstanceManager.Instance.SpawnItem(mapDatas[x, y].position, 5, ItemInstanceManager.ItemInstanceType.CropInstance);
                                mapDatas[x, y].has_item = true;
                                break;
                            case landformTypes.rockland:
                                mapDatas[x, y].type = tileTypes.rock;
                                mapDatas[x, y].texture = tiles[(int)tileTypes.rock];
                                mapDatas[x, y].position = new Vector3Int(x, y, 0);

                                mapDatas[x, y].has_pawn = false;

                                SetTileDev(mapDatas[x, y], BuildManager.Instance.GetBuilding(4));
                                landTilemap.SetTile(new Vector3Int(x, y, 0), mapDatas[x, y].texture);
                                break;
                            default:
                                Debug.LogError("FatalError: Unknown landformType in(" + x + "," + y + ")");
                                break;
                        }

                        break;
                    }
                }


            }
        }
        //提供测试用的草地
        for (int x = 30; x < 35; x++)
        {
            for (int y = 30; y < 35; y++)
            {
                mapDatas[x, y].type = tileTypes.grass;
                mapDatas[x, y].texture = tiles[(int)tileTypes.grass];
                mapDatas[x, y].position = new Vector3Int(x, y, 0);

                mapDatas[x, y].has_pawn = false;

                SetTileDev(mapDatas[x, y], BuildManager.Instance.GetBuilding(0));
                landTilemap.SetTile(new Vector3Int(x, y, 0), mapDatas[x, y].texture);
                if (mapDatas[x, y].item != null)
                {
                    ItemInstanceManager.Instance.DestroyItem(mapDatas[x, y].item, ItemInstanceManager.DestroyMode.RemainNone);
                    mapDatas[x, y].has_item = false;
                    mapDatas[x, y].item = null;
                }
            }
        }

        {//生成商人测试
            MapData data = mapDatas[32, 34];
            TraderManager.TraderBuilding building = TraderManager.Instance.trader;
            data.has_print = false;
            data.has_building = true;
            data.has_item = true;

            SetWalkableState(data, building.can_walk);
            data.can_build = building.can_build;
            data.can_plant = building.can_plant;

            data.item = ItemInstanceManager.Instance.SpawnItem(data.position, building.id, ItemInstanceManager.ItemInstanceType.BuildingInstance);
            //Debug.Log(data.item.id + "商人生成");
        }
        
        const float ORE_SPAWN_LINE = 0.85f;
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                if (landformDatas[x, y] > ORE_SPAWN_LINE)
                {
                    ItemInstanceManager.Instance.SpawnItem(new Vector3Int(x, y, 0), 16, ItemInstanceManager.ItemInstanceType.ResourceInstance);
                    mapDatas[x, y].can_build = false;
                }
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

    void Start()
    {
        // Landforms[(int)landformTypes.waterland] = new LandformRange(0f, 0.1f, landformTypes.waterland);
        // Landforms[(int)landformTypes.grassland] = new LandformRange(0.1f, 0.7f, landformTypes.grassland);
        // Landforms[(int)landformTypes.rockland] = new LandformRange(0.7f, 1.1f, landformTypes.rockland);

        GenerateMapData();
        GenerateMapTiles();
        GenerateBoolVectors();

        #region (1)CropInstance收割和生长接口自测试 【由于会导致脚本调用顺序成环,转移到MapManager 原因：用到MapManager】
        Debug.Log("spawning some crop instance.");
        ItemInstanceManager.CropInstance tmp1 = (ItemInstanceManager.CropInstance)ItemInstanceManager.Instance.SpawnItem(new Vector3Int(30, 30, 0), 0, ItemInstanceManager.ItemInstanceType.CropInstance);
        ItemInstanceManager.CropInstance tmp2 = (ItemInstanceManager.CropInstance)ItemInstanceManager.Instance.SpawnItem(new Vector3Int(30, 31, 0), 1, ItemInstanceManager.ItemInstanceType.CropInstance);
        ItemInstanceManager.CropInstance tmp3 = (ItemInstanceManager.CropInstance)ItemInstanceManager.Instance.SpawnItem(new Vector3Int(30, 32, 0), 2, ItemInstanceManager.ItemInstanceType.CropInstance);
        // ItemInstanceManager.Instance.HarvestCrop(tmp1);
        // ItemInstanceManager.Instance.HarvestCrop(tmp2);
        // ItemInstanceManager.Instance.HarvestCrop(tmp3);
        #endregion
    }

    #endregion


    #region 对外方法

    public void BuildByPlayer(Vector3Int cellPos, Building building){
        TileBase clickedTile = landTilemap.GetTile(cellPos);
        MapData clickedData = mapDatas[cellPos.x, cellPos.y];

        if (clickedTile != null){

            //UIManager.Instance.DebugTextAdd("点击到了 Tile: " + cellPos);
            if(building != null){
                UIManager.Instance.DebugTextAdd("放置建筑: " + building.build_name);

                //非Dev建筑占地特判
                if(building.type != BuildingType.Dev && ( !clickedData.can_build )){
                    Debug.Log("此处已有建筑/蓝图，无法放置");
                    return;
                }

                //根据buildingType更新数据
                if(buildTaskActions.TryGetValue(building.type, out Action<MapData, Building> action))
                    action(clickedData, building);
                else
                    Debug.Log("未定义的建筑类型: " + building.type);

                //Dev：landtilemap更新贴图
                if(building.type == BuildingType.Dev || building.type == BuildingType.Farm){
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
    public bool PawnCanMoveTo(Vector3Int targetCellPos)
    {
        if(!IsWalkable(targetCellPos)) Debug.Log("Error: 因为not walkable");
        if(HasPawnAt(targetCellPos)) Debug.Log("Error: 因为has pawn");
        if(WillHasPawnAt(targetCellPos)) Debug.Log("Error: 因为will has pawn");
        
        return Instance.IsWalkable(targetCellPos) &&
        !Instance.HasPawnAt(targetCellPos) && !Instance.WillHasPawnAt(targetCellPos);
    }

    public bool IsWalkable(Vector3Int pos)
    {
        if (!IsInBoard(pos)) return false;
        return mapDatas[pos.x, pos.y].can_walk;
    }
    public bool IsPlantable(Vector3Int pos){
        if(!IsInBoard(pos)) return false;
        return mapDatas[pos.x, pos.y].can_plant;
    }
    public bool HasPawnAt(Vector3Int pos)
    {
        if (!IsInBoard(pos)) return false;
        return mapDatas[pos.x, pos.y].has_pawn;
    }
    public bool WillHasPawnAt(Vector3Int pos)
    {
        if (!IsInBoard(pos)) return false;
        return mapDatas[pos.x, pos.y].will_has_pawn;
    }
    public bool HasItemAt(Vector3Int pos)
    {
        if (!IsInBoard(pos)) return false;
        return mapDatas[pos.x, pos.y].has_item;
    }
    public bool HasCropAt(Vector3Int pos){
        if(!HasItemAt(pos)) return false;
        if(mapDatas[pos.x, pos.y].item is ItemInstanceManager.CropInstance) return true;
        return false;
    }
    //获取某个地格的所有MapData信息

    public MapData GetMapData(Vector3Int pos)
    {
        if (!IsInBoard(pos))
        {
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
