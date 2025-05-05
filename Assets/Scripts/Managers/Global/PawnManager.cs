
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public class PawnManager : MonoBehaviour{
    #region 成员变量

        #region 常量
    public const float MOVESPEED = 2.0f; // 移动速度
    public const float WORKSPEED = 1.0f; // 工作速度 
    public const int CAPACITY = 50; // 运载容量
        #endregion
    
    public static PawnManager Instance { get; private set; } // 单例模式，确保全局唯一
    public TaskManager TaskManager => TaskManager.Instance; // 通过属性访问 TaskManager 实例
    public ItemManager ItemManager = ItemManager.Instance; // 引用唯一的 ItemManager 对象

    public GameObject pawnPrefab; // Pawn 预设体，用于实例化 Pawn

    public Pawn SelectingPawn; // 当前被选中的 Pawn
    public List<Pawn> pawns = new List<Pawn>(); // 存储所有 Pawn 对象的列表

    public GameObject pawnSpawner; // 生成器对象，用于生成 Pawn

    #endregion

    //直接调用MapManager
    //public Tilemap landTilemap; // 地图的 Tilemap 对象，用于获取格子中心位置
    //public GameObject content; // 地图的内容对象

    #region 生命周期方法

    private void Start(){
        CreatePawn(new Vector3Int(32,32));
        CreatePawn(new Vector3Int(32,31));
        CreatePawn(new Vector3Int(32,33));
    }

    private void Awake(){
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }
    

    #endregion
    
    public class Pawn{
        public int id;
        public bool isOnTask = false;
        public TaskManager.Task handlingTask = null;
        public float moveSpeed = MOVESPEED;
        public float workSpeed = WORKSPEED;

        //运输使用的属性
        public int capacity = CAPACITY;//运载容量

        public int instantCapacity = CAPACITY; //当前运载容量
        public int materialId;
        public int materialAmount; //物品数量
        public ItemInstanceManager.ItemInstanceType materialType; //物品类型

        //工具类型EnhanceType枚举，与小人属性挂钩，itemmanager调用
        public enum EnhanceType{
            Speed, Power,capacity, Total
        }

        //public Vector2 position; 
        //需要存储什么样的位置？和transform.position作何区别？是一个快捷访问，还是存储其地格坐标？如果是后者，应该改用Vector3Int --cjh
        public ItemManager.Tool handlingTool;
        public List<TaskManager.Task> PawntaskList = new List<TaskManager.Task>();
        public GameObject Instance;

        public void HandleCurrentTask(){
            
        }
        public Pawn(int id)
        {
            this.id = id;
        }
    }

    // 根据工具增强属性修改移动速度和搬运容量
    // todo:增加放下工具的处理逻辑，重置基础属性
    //调用可能：UI组件直接调用？
    public void GetToolAttribute(Pawn pawn, ItemManager.Tool tool){
        //暂定比例增强，可后续改动算法
        float baseSpeed = pawn.moveSpeed; 
        float speedModifier = 1 + (tool.enhancements[PawnManager.Pawn.EnhanceType.Speed] / 100f);
        float actualSpeed = baseSpeed * speedModifier;
        pawn.moveSpeed = actualSpeed;
        //todo:搬运容量itemManager.tool.capacity尚未实现，暂时不修改搬运容量
        //todo:工作速度待考虑，可能不同任务的加成不同

    }
    //后续可能改用selectingPawn直接调用
    public void HasTool(Pawn pawn, ItemManager.Tool tool){
        pawn.handlingTool = tool;
        GetToolAttribute(pawn, tool);
    }
    
    //放下工具，重置属性
    public void DropTool(Pawn pawn){
        pawn.handlingTool = null;
        pawn.moveSpeed = MOVESPEED; // 重置为基础速度
        pawn.workSpeed = WORKSPEED; // 重置工作速度
        pawn.capacity = CAPACITY; // 重置容量
    }

    public void InstantiatePawn(Pawn pawn, Vector3Int startPos){

        if (pawn == null || pawn.Instance != null){
            Debug.LogWarning("Pawn 或其已经实例化，无法实例化！");
            return;
        }

        pawn.Instance = GameObject.Instantiate(pawnPrefab,PawnManager.Instance.pawnSpawner.transform);
        pawn.Instance.name = $"Pawn_{pawn.id}";
        
        // 设置位置
        Vector3 worldPosition = MapManager.Instance.landTilemap.GetCellCenterWorld(startPos);
        pawn.Instance.transform.position = worldPosition;
        // 消除缩放影响
        Vector3 contentLossyScale = MapManager.Instance.content.transform.lossyScale;
        Vector3 contentLocalScale = MapManager.Instance.content.transform.localScale;
        Vector3 totalScale = new Vector3(
            contentLocalScale.x / contentLossyScale.x,
            contentLocalScale.y / contentLossyScale.y,
            contentLocalScale.z / contentLossyScale.z
        );
        pawn.Instance.transform.localScale = totalScale ;
        //Instance.transform.localScale = new Vector3(0.5f, 0.5f, 1);

        // 获取已经挂在预制体上的 PawnInteractController 脚本
        PawnInteractController controller = pawn.Instance.GetComponent<PawnInteractController>();
        if (controller != null){
            controller.pawn =pawn; // 关键点
            controller.landTilemap = MapManager.Instance.landTilemap; 
            controller.content = MapManager.Instance.content; 
        }
        else{
            Debug.LogWarning("PawnInteractController 没有挂载在 Pawn 预制体上！");
        }
    }
    public void CreatePawn(Vector3Int startPos)
    {
        int newId = pawns.Count + 1;//todo:id处理函数
        Pawn newPawn = new Pawn(newId);
        InstantiatePawn(newPawn,startPos);

        MapManager.Instance.SetPawnState(startPos, true);

        pawns.Add(newPawn);
    }
    //根据位置创建小人，用于在部分任务中调用

    // 完全没必要存在 --cjh
    // public void CreatePawnAtPosition(Vector3Int position){CreatePawn(position);}

    public Pawn GetAvailablePawn(){
        foreach (var pawn in pawns){
            if (!pawn.isOnTask){
                return pawn;
            }
        }
        return null;
    }

    public void SelectPawn(int id){
        SelectingPawn = pawns.Find(pawn => pawn.id == id);
    }
    //直接改成重载就行 --cjh
    public void SelectPawn(Pawn pawn){
        if(pawn != null){
            SelectingPawn = pawn;
        }
    }

    public void SetSelectingPawnTask(TaskManager.Task task){
        if(task != null){
            if(SelectingPawn != null)
            {
                SelectingPawn.handlingTask = task;
            }
            else{
                Debug.Log("未选中 Pawn selectingPawn is null");  
            }
        }
        else{
            Debug.Log("未选中任务 task is null");
        }
    }

    public void GetNextTask(Pawn pawn){
        if(pawn != null){
            if(pawn.isOnTask == false && pawn.PawntaskList.Count > 0){
                pawn.handlingTask = pawn.PawntaskList[0];
                pawn.isOnTask = true;
            }
            else{
                Debug.Log("当前 Pawn 没有空闲或没有任务");  
            }
        }
        else{
            Debug.Log("未选中 Pawn pawn is null");
        }
    }

    public void PawnTaskListUpdate(Pawn pawn){
        if(pawn.handlingTask == null){
            Debug.LogWarning("没有正在处理的任务，无法更新任务列表！");
            return;
        }
        int removedCount = pawn.PawntaskList.RemoveAll(task => task.task_id == pawn.handlingTask.task_id);

        if(removedCount > 0){
            pawn.isOnTask = false;
            pawn.handlingTask = null;
            Debug.Log("任务已成功完成并从列表中移除！");
        }
        else{
            Debug.LogWarning("任务已不在列表中，可能已被其他逻辑移除！");
        }
    }

    public void AddPawnTask(int pawnID, TaskManager.Task task){
        if(task != null){
            SelectPawn(pawnID);
            if(SelectingPawn != null){
                SelectingPawn.PawntaskList.Add(task);
            }
            else{
                Debug.Log("未找到对应的小人");  
            }
        }
        else{
            Debug.Log("未选中 Pawn 或未选择任务");
        }
    }
    // 添加多个任务到指定小人的任务列表中
    public void AddPawnTasks(int pawnID, List<TaskManager.Task> tasks){
        SelectPawn(pawnID); // 确保 SelectingPawn 被设置

        if (SelectingPawn != null && tasks != null && tasks.Count > 0){
            SelectingPawn.PawntaskList.AddRange(tasks);
            Debug.Log($"批量添加了 {tasks.Count} 个任务给 Pawn ID: {SelectingPawn.id}");
        }
        else{
            Debug.Log("添加失败：未找到 Pawn 或任务列表为空");
        }
    }
    // 清空指定小人的任务列表并取消当前处理的任务
    public void ClearPawnTaskList(int pawnID){
        SelectPawn(pawnID); // 选择目标小人

        if (SelectingPawn != null){
            SelectingPawn.PawntaskList.Clear();
            SelectingPawn.handlingTask = null;
            Debug.Log($"清空了 Pawn ID: {SelectingPawn.id} 的任务列表");
        }
        else{
            Debug.Log("清空失败：未找到 Pawn");
        }
    }
    // 处理任务失败，移除当前处理的任务并尝试获取下一个任务
    public void HandleTaskFailure(Pawn pawn){
        if (pawn != null && pawn.handlingTask != null){
            Debug.Log($"任务失败，移除 Task ID: {pawn.handlingTask.task_id}，尝试执行下一个任务");

            // 移除失败的任务
            pawn.PawntaskList.Remove(pawn.handlingTask);
            pawn.handlingTask = null;
            pawn.isOnTask = false;

            // 获取下一个任务
            GetNextTask(pawn);
        }
        else{
            Debug.Log("任务失败处理失败：Pawn 或 handlingTask 为 null");
        }
    }
    //todo:task中三种任务的处理逻辑

    //todo:工作时间计算函数：与当前power，任务难度、进度有关

    //todo:装载、卸载函数：
    //1.添加作为pawn的附加属性，删除原instance，卸载时去除附属属性，在新位置添加物品实例

    //todo:运输任务实现：
    //1.移动到物品位置
    //2.装载物品
    //3.移动到目标位置
    //4.卸载物品

    //todo:建造任务实现：
    //1。根据task查找建筑材料，根据iteminstancemanager.FindNearestItemPosition查找材料对应位置
    //2. 移动pawn到对应位置
    //3. 和运输任务相同逻辑运输到工作地点
    //4。调用工作时间计算函数，计算工作时间，进入“建造动画”
    //5. 工作时间耗尽建造完成后，消耗掉材料，创建建筑，结束任务

    //todo:种植任务，实现：建造任务的一种

    //todo:收割任务实现：
    //1.移动到任务地点
    //2.调用工作时间计算函数，计算工作时间，进入“收割动画”
    //3.工作完成，创建物品，结束任务

    //工作时间计算
    private float GetWorkTime(Pawn pawn ,TaskManager.Task task){
        int TotalProcess = task.tasklevel * 100; 
        float time = TotalProcess / pawn.workSpeed;
        return time; //仅为简易实现，后续数值及算法有待调整
    }

    //装载、卸载函数
    public bool PawnLoad(Pawn pawn,ItemInstanceManager.MaterialInstance itemInstance,TaskManager.Task task){
        if(pawn.instantCapacity > itemInstance.amount){
            //销毁instance
            pawn.materialId = itemInstance.GetModelId();
            pawn.materialAmount = itemInstance.GetAmount();
            pawn.instantCapacity -= itemInstance.GetAmount(); //运载容量减少
            //pawn.materialType = itemInstance.type;(默认为material)
            ItemInstanceManager.Instance.DestroyItem(itemInstance);
        }
        else{
            //不必销毁instance，改动数值
            pawn.materialId = itemInstance.GetModelId();
            itemInstance.SetAmount(itemInstance.GetAmount() - pawn.instantCapacity);
            pawn.materialAmount = pawn.instantCapacity;
            pawn.instantCapacity = 0; //运载容量清空
        }
        return true;
    }
    public bool PawnUnload(Pawn pawn){
        // 创建 instance 以及对应的 count
        // 假定目的地卸载位置上无其他 instance
        // todo: 卸载地点有相同的物品时，数量叠加；卸载地点有不同的物品时，显示卸载失败或者放在旁边的空位上
        Vector3 pawnWorldPosition = pawn.Instance.transform.position;
        Vector3Int pawnCellPosition = Vector3Int.FloorToInt(pawnWorldPosition); // 转换为 Vector3Int

        // 创建物品列表
        List<KeyValuePair<int, int>> materialList = new List<KeyValuePair<int, int>>();
        materialList.Add(new KeyValuePair<int, int>(pawn.materialId, pawn.materialAmount));

        // 调用 SetMaterial 方法
        int result = MapManager.Instance.SetMaterial(pawnCellPosition, materialList);

        // 重置 Pawn 的物品信息
        pawn.materialId = 0;
        pawn.materialAmount = 0;
        pawn.instantCapacity = pawn.capacity; // 重置容量

        // 更改 task 中的物品需求数目
        // 这里假设 task 中有一个方法来更新物品需求
        // task.UpdateRequiredItems(materialList);

        return result > 0; // 根据 SetMaterial 的返回值判断是否成功
    }

    //运输任务实现：

    public void Pawntransport(Pawn pawn, TaskManager.TaskTransport task){
        if (pawn == null || pawn.Instance == null){
            Debug.LogWarning("Pawn 或其实例为空，无法执行运输任务！");
            return;
        }

        // 获取 PawnInteractController 实例
        PawnInteractController controller = pawn.Instance.GetComponent<PawnInteractController>();
        if (controller == null){
            Debug.LogWarning("PawnInteractController 未挂载在 Pawn 实例上！");
            return;
        }

        // 移动到物品位置
        controller.MovePawnToPosition(task.beginPosition, pawn);

        // 获取目标地块的格子坐标
        Vector3Int targetCellPos = task.beginPosition;

        // 检查目标地块是否有物品
        if (MapManager.Instance.HasPawnAt(targetCellPos)){
            Debug.LogWarning("目标地块上已有其他 Pawn，无法装载物品！");
            return;
        }

        if (MapManager.Instance.GetMapData(targetCellPos).has_item){
            // 获取地块上的物品实例
            var itemInstance = MapManager.Instance.GetMapData(targetCellPos).item as ItemInstanceManager.MaterialInstance;

            if (itemInstance != null){
                // 调用 PawnLoad 方法进行装载
                bool loadSuccess = PawnLoad(pawn, itemInstance, task);

                if (loadSuccess){
                    Debug.Log($"Pawn ID: {pawn.id} 成功装载物品 ID: {itemInstance.item_id} 数量: {itemInstance.amount}");
                }
                else{
                    Debug.LogWarning($"Pawn ID: {pawn.id} 装载物品失败！");
                }
            }
            else{
                Debug.LogWarning("目标地块上的物品不是 MaterialInstance,无法装载！");
            }
        }
        else{
            Debug.LogWarning("目标地块没有物品可供装载！");
        }
    }


    Dictionary<TaskManager.TaskTypes, Action<Pawn>> taskHandler = new Dictionary<TaskManager.TaskTypes, Action<Pawn>>{
        { TaskManager.TaskTypes.Harvest, (pawn) => Instance.HandleHarvestTask(pawn) },
        { TaskManager.TaskTypes.Plant, (pawn) => Instance.HandleBuildFarmTask(pawn) }
    };

    Dictionary<BuildManager.BuildingType, Action<Pawn>> buildTaskHandler = new Dictionary<BuildManager.BuildingType, Action<Pawn>>{
        { BuildManager.BuildingType.Farm, (pawn) => Instance.HandleBuildFarmTask(pawn) },
        { BuildManager.BuildingType.Wall, (pawn) => Instance.HandleBuildTask(pawn) }
    };

    public void HandleTask(Pawn pawn){
        if (pawn == null || pawn.handlingTask == null){
            Debug.LogWarning("Pawn 或其任务为空，无法执行任务！");
            return;
        }

        TaskManager.Task task = pawn.handlingTask;

        if(task.type == TaskManager.TaskTypes.Build){
            BuildManager.BuildingType buildingType = BuildManager.Instance.GetBuildingType(task.id);

            if (buildTaskHandler.ContainsKey(buildingType)){
                buildTaskHandler[buildingType].Invoke(pawn);
            }
            else{
                Debug.LogWarning($"不支持的buildTask类型: {buildingType}");
            }
        }
        else{
            if (taskHandler.ContainsKey(task.type)){
                taskHandler[task.type].Invoke(pawn);
            }
            else{
                Debug.LogWarning($"未处理的任务类型: {task.type}");
            }
        }
        
        
    }

    //建造任务
    public void HandleBuildTask(Pawn pawn){
        if (pawn == null || pawn.handlingTask == null){
            Debug.LogWarning("Pawn 或其任务为空，无法执行任务！");
            return;
        }
        TaskManager.Task task = pawn.handlingTask;

        // 1. 根据任务查找建筑材料
        Vector3Int? materialPosition = ItemInstanceManager.Instance.FindNearestItemPosition(
            task.id, 
            MapManager.Instance.GetCellPosFromWorld(pawn.Instance.transform.position)
        );

        if (materialPosition == null){
            Debug.LogWarning($"未找到任务所需的材料 ID: {task.id}");
            return;
        }

        // 2. 移动 Pawn 到材料位置
        PawnInteractController controller = pawn.Instance.GetComponent<PawnInteractController>();
        if (controller == null){
            Debug.LogWarning("PawnInteractController 未挂载在 Pawn 实例上！");
            return;
        }

        controller.MovePawnToPosition(materialPosition.Value, pawn);

        // 等待 Pawn 到达材料位置
        // if (Vector3.Distance(pawn.Instance.transform.position, MapManager.Instance.GetCellPosFromWorld(materialPosition.Value)) > 0.05f){
        //     Debug.Log("Pawn 正在移动到材料位置...");
        //     return;
        // }

        // 3. 装载材料
        MapManager.MapData materialData = MapManager.Instance.GetMapData(materialPosition.Value);
        if (materialData.has_item && materialData.item is ItemInstanceManager.MaterialInstance materialInstance){
            bool loadSuccess = PawnLoad(pawn, materialInstance, task);
            if (!loadSuccess){
                Debug.LogWarning("装载材料失败！");
                return;
            }
        }
        else{
            Debug.LogWarning("目标位置没有可用的材料！");
            return;
        }

        // // 4. 移动到建造地点
        // controller.MovePawnToPosition(task.target_position, pawn);

        // // 等待 Pawn 到达建造地点
        // if (Vector3.Distance(pawn.Instance.transform.position, MapManager.Instance.GetCellPosFromWorld(task.target_position)) > 0.05f){
        //     Debug.Log("Pawn 正在移动到建造地点...");
        //     return;
        // }

        // 5. 等待建造完成
        float workTime = GetWorkTime(pawn, task);
        Debug.Log($"开始建造任务，预计耗时: {workTime} 秒");
        System.Threading.Thread.Sleep((int)(workTime * 1000)); // 模拟等待建造完成

        // 6. 建造完成后，消耗材料，创建建筑
        // MapManager.MapData buildData = MapManager.Instance.GetMapData(task.target_position);
        // if (buildData != null){
        //     buildData.has_building = true;
        //     buildData.item = ItemInstanceManager.Instance.SpawnItem(
        //         task.target_position, 
        //         task.MaterialId, 
        //         ItemInstanceManager.ItemInstanceType.BuildingInstance
        //     );

        //     Debug.Log($"建造任务完成，建筑已创建在位置: {task.target_position}");
        // }

        // 清空 Pawn 的任务状态
        pawn.isOnTask = false;
        pawn.handlingTask = null;
    }
    
    //开垦土地任务，到达目标地点，经过固定时间后添加农田
    //注意判断土地是否可开垦
    public void HandleBuildFarmTask(Pawn pawn){
        if (pawn == null || pawn.handlingTask == null){
            Debug.LogWarning("Pawn 或其任务为空，无法执行任务！");
            return;
        }
        TaskManager.Task task = pawn.handlingTask;

        // 1. 根据任务查找农作物位置
        Vector3Int? FarmPosition = task.target_position;

        // 2. 移动 Pawn 到农作物位置
        PawnInteractController controller = pawn.Instance.GetComponent<PawnInteractController>();
        if (controller == null){
            Debug.LogWarning("PawnInteractController 未挂载在 Pawn 实例上！");
            return;
        }
        controller.MovePawnToPosition(FarmPosition.Value, pawn);


        // if (Vector3.Distance(pawn.Instance.transform.position, MapManager.Instance.GetCellPosFromWorld(FarmPosition.Value)) > 0.05f){
        //     Debug.Log("Pawn 正在移动到农作物位置...");
        //     return;
        // }
        // Debug.Log("Pawn 到达农作物位置,开始FarmTask...");


        float workTime = GetWorkTime(pawn, task);
        Debug.Log($"开始开垦任务，预计耗时: {workTime} 秒"); 
        System.Threading.Thread.Sleep((int)(workTime * 1000));
        Debug.Log("FarmTask 完成!");

        pawn.handlingTask = null;
        pawn.isOnTask=false;

        // 4. 调用 MapManager 的 SetTileFarm 方法创建农田
        MapManager.MapData mapData = MapManager.Instance.GetMapData(FarmPosition.Value);

        if (mapData == null){
            Debug.LogWarning("目标位置的 MapData 不存在，无法创建农田！");
            return ;
        }
        MapManager.Instance.SetTileFarm(mapData, BuildManager.Instance.GetBuilding(task.id)); // 假设农田的类型为 `farm`
    }
    
    //收割任务，注意调用此函数时需要确保作物在可收割状态
    public void HandleHarvestTask(Pawn pawn){
        if (pawn == null || pawn.handlingTask == null){
            Debug.LogWarning("Pawn 或其任务为空，无法执行任务！");
            return;
        }
        TaskManager.Task task = pawn.handlingTask;
        Vector3Int? cropPosition = task.target_position;
        

        // 2. 移动 Pawn 到农作物位置
        PawnInteractController controller = pawn.Instance.GetComponent<PawnInteractController>();
        if (controller == null){
            Debug.LogWarning("PawnInteractController 未挂载在 Pawn 实例上！");
            return;
        }

        controller.MovePawnToPosition(cropPosition.Value, pawn);

        // 等待 Pawn 到达农作物位置
        if (Vector3.Distance(pawn.Instance.transform.position, cropPosition.Value) > 0.05f){
            Debug.Log("Pawn 正在移动到农作物位置...");
            return;
        }
        Debug.Log("Pawn 到达农作物位置,开始HarvestTask...");

        // 3. 等待收割完成
        float workTime = GetWorkTime(pawn, task);
        Debug.Log($"开始收割任务，预计耗时: {workTime} 秒"); 
        System.Threading.Thread.Sleep((int)(workTime * 1000)); // 模拟等待收割完成
        Debug.Log("HarvestTask 完成!");

        // 4. 收割完成后，创建物品实例
        MapManager.MapData cropData = MapManager.Instance.GetMapData(cropPosition.Value);
        if (cropData != null && !cropData.has_item){
            cropData.has_item = true;
            //暂时由任务来指定生成出来的物品，用于测试，后续需要根据作物收获物表格获取物品
            ItemInstanceManager.MaterialInstance newCrop = ItemInstanceManager.Instance.SpawnItem(
                task.target_position, 
                task.id, 
                ItemInstanceManager.ItemInstanceType.MaterialInstance
            ) as ItemInstanceManager.MaterialInstance;
            Debug.Log($"收割任务完成，物品已创建在位置: {task.target_position}");
        }
    }
    
    
}
