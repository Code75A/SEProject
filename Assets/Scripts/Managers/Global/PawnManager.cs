
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;


public class PawnManager : MonoBehaviour{
    #region 成员变量

        #region 常量
    public const float MOVESPEED = 4.0f; // 移动速度
    public const float WORKSPEED = 1.0f; // 工作速度 
    public const int CAPACITY = 50; // 运载容量
        #endregion
    
    public static PawnManager Instance { get; private set; } // 单例模式，确保全局唯一
    public TaskManager TaskManager => TaskManager.Instance; // 通过属性访问 TaskManager 实例
    public ItemManager ItemManager = ItemManager.Instance; // 引用唯一的 ItemManager 对象

    public GameObject pawnPrefab; // Pawn 预设体，用于实例化 Pawn

    //public Pawn SelectingPawn; // 当前被选中的 Pawn
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

    public struct attribute{
        int a;
        int b;
        int c;

        private attribute(int a1, int b2, int c3)
        {
            a = a1;
            b = b2;
            c = c3;
        }
        public void updata_attribute(int da, int db, int dc)
        {
            a = da;
            b = db;
            c = dc;
        }
    }

    public class Pawn
    {
        public int id;
        public bool isOnTask = false;
        public TaskManager.Task handlingTask = null;
        public float moveSpeed = MOVESPEED;
        public float workSpeed = WORKSPEED;

        //运输使用的属性
        public int capacity = CAPACITY;//运载容量

        public int instantCapacity = CAPACITY; //当前运载容量
        public int materialId = 0;
        public int materialAmount = 0; //物品数量
        public ItemInstanceManager.ItemInstanceType materialType; //物品类型

        //工具类型EnhanceType枚举，与小人属性挂钩，itemmanager调用
        public enum EnhanceType
        {
            Speed, Power, capacity, Total
        }

        //public Vector2 position; 
        //需要存储什么样的位置？和transform.position作何区别？是一个快捷访问，还是存储其地格坐标？如果是后者，应该改用Vector3Int --cjh
        public ItemManager.Tool handlingTool;
        public List<TaskManager.Task> PawntaskList = new List<TaskManager.Task>();
        public GameObject Instance;

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
            UnityEngine.Debug.LogWarning("Pawn 或其已经实例化，无法实例化！");
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
            controller.fromCellPos = startPos; // 设置起始位置
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

    // public void SelectPawn(int id){
    //     SelectingPawn = pawns.Find(pawn => pawn.id == id);
    // }
    // //直接改成重载就行 --cjh
    // public void SelectPawn(Pawn pawn){
    //     if(pawn != null){
    //         SelectingPawn = pawn;
    //     }
    // }

    #region 开发中任务接口
    // public void SetSelectingPawnTask(TaskManager.Task task){
    //     if(task != null){
    //         if(SelectingPawn != null)
    //         {
    //             SelectingPawn.handlingTask = task;
    //         }
    //         else{
    //             Debug.Log("未选中 Pawn selectingPawn is null");  
    //         }
    //     }
    //     else{
    //         Debug.Log("未选中任务 task is null");
    //     }
    // }

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
    // 添加多个任务到指定小人的任务列表中
    // public void AddPawnTasks(int pawnID, List<TaskManager.Task> tasks){
    //     SelectPawn(pawnID); // 确保 SelectingPawn 被设置

    //     if (SelectingPawn != null && tasks != null && tasks.Count > 0){
    //         SelectingPawn.PawntaskList.AddRange(tasks);
    //         Debug.Log($"批量添加了 {tasks.Count} 个任务给 Pawn ID: {SelectingPawn.id}");
    //     }
    //     else{
    //         Debug.Log("添加失败：未找到 Pawn 或任务列表为空");
    //     }
    // }
    // 清空指定小人的任务列表并取消当前处理的任务
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
    #endregion

    public void AddPawnTask(Pawn pawn, TaskManager.Task task){
        if(task != null){
            if (pawn != null)
            {
                pawn.PawntaskList.Add(task);
                // 输出当前任务队列中的所有任务
                Debug.Log($"任务 {task.task_id} 已添加到 Pawn 的任务队列中。当前任务队列如下：");
                foreach (var t in pawn.PawntaskList)
                {
                    Debug.Log($"任务 ID: {t.task_id}, 类型: {t.type}, 目标位置: {t.target_position}");
                }
            }
            else
            {
                Debug.Log("Error: AddPawnTask时选中Pawn为空");
            }
        }
        else{
            Debug.Log("Error: AddPawnTask时任务为空");
        }
    }
    public void ClearPawnTaskList(Pawn pawn){
        if (pawn != null){
            pawn.PawntaskList.Clear();
            pawn.handlingTask = null;
            Debug.Log($"清空了 Pawn ID: {pawn.id} 的任务列表");
        }
        else{
            Debug.Log("Error：ClearPawnTaskList时 Pawn 为空");
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
    //注意这里装载时并不会更改amount，需要在卸载后更改amount并重新发布运输任务
    public bool PawnLoad(Pawn pawn, ItemInstanceManager.MaterialInstance itemInstance, int amount, Vector3Int pos)
    {
        int realtransportamount;
        if (amount < itemInstance.amount)
        {
            realtransportamount = amount;
        }
        else
        {
            realtransportamount = itemInstance.amount;
        }
        if (pawn.instantCapacity > realtransportamount)
        {
            //销毁instance
            pawn.materialId = itemInstance.GetModelId();
            pawn.materialAmount = itemInstance.GetAmount();
            pawn.instantCapacity -= itemInstance.GetAmount(); //运载容量减少
            //pawn.materialType = itemInstance.type;(默认为material)
            ItemInstanceManager.Instance.DestroyItem(itemInstance);
            Debug.Log($"Pawn ID: {pawn.id} 装载物品 ID: {itemInstance.item_id} 数量: {itemInstance.amount}");
            MapManager.Instance.DeleteMapDataItem(pos);//删除地图数据中的物品实例
        }

        else
        {
            //不必销毁instance，改动数值
            pawn.materialId = itemInstance.GetModelId();
            itemInstance.SetAmount(itemInstance.GetAmount() - pawn.instantCapacity);
            pawn.materialAmount = pawn.instantCapacity;
            pawn.instantCapacity = 0; //运载容量清空
        }
        return true;
    }
    public int PawnUnload(Pawn pawn){
        // 1. 使用 MapManager.Instance.GetCellPosFromWorld 获得该 Pawn 所在的地图格子坐标
        Vector3Int pawnCellPosition = MapManager.Instance.GetCellPosFromWorld(pawn.Instance.transform.position);

        // 2. 创建物品列表
        List<KeyValuePair<int, int>> materialList = new List<KeyValuePair<int, int>>()
        {
            new KeyValuePair<int, int>(pawn.materialId, pawn.materialAmount)
        };

        // 3. 调用 MapManager 的 SetMaterial，指定格子坐标
        int result = MapManager.Instance.SetMaterial(pawnCellPosition, materialList);

        // 4. 重置 Pawn 的物品信息
        int result1 = pawn.materialAmount; // 物品数量
        pawn.materialId = 0;
        pawn.materialAmount = 0;
        pawn.instantCapacity = pawn.capacity; // 恢复运载容量

        // 5. 根据返回值判断是否成功创建物品
        return result1;
    }

    //运输任务实现：
    //需要新增运输到蓝图的运输函数逻辑，直接改变对应progressinstance的进度而并非调用pawnunload创建新物品实例
    public System.Collections.IEnumerator HandleTransportTask(Pawn pawn, TaskManager.TransportTask task)
    {
        // 1. 检查合法性
        if (pawn == null || pawn.Instance == null)
        {
            Debug.LogWarning("Pawn 或其实例为空，无法执行运输任务！");
            yield break;
        }

        // 获取 PawnInteractController
        PawnInteractController controller = pawn.Instance.GetComponent<PawnInteractController>();
        if (controller == null)
        {
            Debug.LogWarning("PawnInteractController 未挂载在 Pawn 实例上！");
            yield break;
        }

        // 2. 移动到物品位置（beginPosition）
        // controller.MovePawnToPosition(task.beginPosition, pawn);
        // // 等待 Pawn 到达
        // yield return new WaitWhile(() => controller.isMoving);
        TaskManager.Task Transporttaskstart = pawn.handlingTask;
        pawn.handlingTask = new TaskManager.Task(
            position: task.beginPosition,
            type: TaskManager.TaskTypes.Move,
            task_id: -1
        );
        AddPawnTask(pawn, Transporttaskstart);
        HandleTask(pawn);
        //yield return StartCoroutine(HandleTask(pawn));
        Debug.Log("拆分移动任务完成start");

        // 3. 装载物品
        MapManager.MapData beginData = MapManager.Instance.GetMapData(task.beginPosition);
        if (beginData != null && beginData.has_item &&
            beginData.item is ItemInstanceManager.MaterialInstance loadItem)
        {
            bool loadSuccess = PawnLoad(pawn, loadItem,task.amount, task.beginPosition);
            if (!loadSuccess)
            {
                Debug.LogWarning("装载物品失败！");
                yield break;
            }
            Debug.Log("装载物品成功！");
        }
        else
        {
            Debug.LogWarning("物品位置不正确或物品类型不匹配，无法装载！");
            yield break;
        }

        // 4. 移动到目标位置（endPosition）
        // controller.MovePawnToPosition(task.target_position, pawn);
        // // 等待 Pawn 到达
        // yield return new WaitWhile(() => controller.isMoving);
        TaskManager.Task Transporttaskend = pawn.handlingTask;
        pawn.handlingTask = new TaskManager.Task(
            position: task.target_position,
            type: TaskManager.TaskTypes.Move,
            task_id: -1
        );
        AddPawnTask(pawn, Transporttaskend);
        HandleTask(pawn);
        //yield return StartCoroutine(HandleTask(pawn));
        Debug.Log("拆分移动任务完成end");

        // 5. 卸载物品
        int unloadSuccess_amount = PawnUnload(pawn);

    }

    public System.Collections.IEnumerator HandleToPrintInstanceTransportTask(Pawn pawn, TaskManager.TransportTask task)
    {
        // 1. 检查合法性
        if (pawn == null || pawn.Instance == null)
        {
            Debug.LogWarning("Pawn 或其实例为空，无法执行运输任务！");
            yield break;
        }

        // 获取 PawnInteractController
        PawnInteractController controller = pawn.Instance.GetComponent<PawnInteractController>();
        if (controller == null)
        {
            Debug.LogWarning("PawnInteractController 未挂载在 Pawn 实例上！");
            yield break;
        }

        //更改移动逻辑
        // 2. 移动到物品位置（beginPosition）
        // controller.MovePawnToPosition(task.beginPosition, pawn);
        // // 等待 Pawn 到达
        // yield return new WaitWhile(() => controller.isMoving);

        TaskManager.Task Transporttaskstart = pawn.handlingTask;
        pawn.handlingTask = new TaskManager.Task(
            position: task.beginPosition,
            type: TaskManager.TaskTypes.Move,
            task_id: -1
        );
        AddPawnTask(pawn, Transporttaskstart);
        HandleTask(pawn);
        //yield return StartCoroutine(HandleTask(pawn));
        Debug.Log("printinstance拆分移动任务完成start");

        // 3. 装载物品
        //有概率发生随机bug，导致无法进入装载物品分支，有待确定问题位置
        MapManager.MapData beginData = MapManager.Instance.GetMapData(task.beginPosition);
        Debug.LogWarning($"beginData: {beginData} beginData.has_item: {beginData.has_item} beginData.item: {beginData.item}");
        if (beginData != null && beginData.has_item &&
            beginData.item is ItemInstanceManager.MaterialInstance loadItem)
        {
            bool loadSuccess = PawnLoad(pawn, loadItem, task.amount, task.beginPosition);
            if (!loadSuccess)
            {
                Debug.LogWarning("装载物品失败！");
                yield break;
            }
            Debug.Log("装载物品成功！");
        }
        else
        {
            Debug.LogWarning("物品位置不正确或物品类型不匹配，无法装载！");
            yield break;
        }

        // 4. 移动到目标位置（endPosition）
        //由于蓝图的不可达性质，移动到蓝图位置下方一格（后续仍需改进查找临近格子）
        // controller.MovePawnToPosition(task.target_position + new Vector3Int(0, -1, 0), pawn);
        // // 等待 Pawn 到达
        // yield return new WaitWhile(() => controller.isMoving);

        TaskManager.Task Transporttaskend = pawn.handlingTask;
        pawn.handlingTask = new TaskManager.Task(
            position: task.target_position + new Vector3Int(0, -1, 0),
            type: TaskManager.TaskTypes.Move,
            task_id: -1
        );
        AddPawnTask(pawn, Transporttaskend);
        HandleTask(pawn);
        //yield return StartCoroutine(HandleTask(pawn));
        Debug.Log("printinstance拆分移动任务完成end");

        // 5. 卸载物品,更改蓝图进度
        //printinstance的装载进度接口，直接调用并清空pawn的装载数据即可
        MapManager.MapData targetData = MapManager.Instance.GetMapData(task.target_position);
        if (targetData != null && targetData.item is ItemInstanceManager.PrintInstance printInstance)
        {
            printInstance.PushProgress(pawn.materialId, pawn.materialAmount);
            pawn.materialId = 0;
            pawn.materialAmount = 0;
        }
        pawn.isOnTask = false;
        Debug.LogWarning($"卸载物品成功，已将物品 ID: {pawn.materialId} 数量: {pawn.materialAmount} 卸载到蓝图位置: {task.target_position}");
        //暂时没有对应结束需求
        //ResolveTask(pawn);
    }

    #region 任务处理函数
    //任务开始时进行的更新
    Dictionary<TaskManager.TaskTypes, Action<Pawn>> taskHandler = new Dictionary<TaskManager.TaskTypes, Action<Pawn>>{
        { TaskManager.TaskTypes.Move, (pawn) => Instance.HandleMoveTask(pawn)},
        { TaskManager.TaskTypes.Harvest, (pawn) => Instance.HandleHarvestTask(pawn) },
        //{  TaskManager.TaskTypes.Build, (pawn) => Instance.HandleBuildTask(pawn) },
        { TaskManager.TaskTypes.Transport, (pawn) => Instance.StartCoroutine(Instance.HandleTransportTask(pawn, pawn.handlingTask as TaskManager.TransportTask)) },
        { TaskManager.TaskTypes.BuildingTransport, (pawn) => Instance.StartCoroutine(Instance.HandleToPrintInstanceTransportTask(pawn, pawn.handlingTask as TaskManager.TransportTask)) },
        { TaskManager.TaskTypes.Plant, (pawn) => Instance.StartCoroutine(Instance.HandleBuildFarmTask(pawn)) }
    };

    Dictionary<BuildManager.BuildingType, Action<Pawn>> buildTaskHandler = new Dictionary<BuildManager.BuildingType, Action<Pawn>>{
        { BuildManager.BuildingType.Farm, (pawn) => Instance.StartCoroutine(Instance.HandleBuildFarmTask(pawn)) },
        { BuildManager.BuildingType.Wall, (pawn) => Instance.StartCoroutine(Instance.HandleBuildTask(pawn)) }
    };

    public void HandleTask(Pawn pawn){
        if (pawn == null || pawn.handlingTask == null){
            //TODO: 多任务队列  
            Debug.LogWarning("Pawn 或其任务为空，无法执行任务！");
            return;
        }
        TaskManager.Task task = pawn.handlingTask;
        Debug.Log($"开始处理任务: {task.type}，ID: {task.task_id}");

        pawn.isOnTask = true;
        if(task.type == TaskManager.TaskTypes.Build){
            BuildManager.BuildingType buildingType = BuildManager.Instance.GetBuildingType(task.id);
            Debug.Log($"建筑类型: {buildingType}");

            if (buildTaskHandler.ContainsKey(buildingType))
            {
                buildTaskHandler[buildingType].Invoke(pawn);
                Debug.Log($"开始处理建筑任务: {buildingType}");
                
            }
            else
            {
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

    public void HandleMoveTask(Pawn pawn){
        Debug.Log("HandleMoveTask开始");
        if (pawn == null || pawn.handlingTask == null){
            Debug.LogWarning("Pawn 或其任务为空，无法执行任务！");
            return;
        }
        TaskManager.Task task = pawn.handlingTask;

        Vector3Int currentCellPos = MapManager.Instance.GetCellPosFromWorld(pawn.Instance.transform.position);
        
        //TODO：
        //MapManager.Instance.SetPawnState(false);

        //提前预定了目标位置被占用但是在任务取消时在PawnInteractController注意需要取消占用（类似锁）
        PawnInteractController controller = pawn.Instance.GetComponent<PawnInteractController>();

        controller.MovePawnToPosition(task.target_position, pawn);

        Debug.Log(controller.fromCellPos);
        Debug.Log(currentCellPos);

        if(controller.fromCellPos == currentCellPos){
            MapManager.Instance.SetPawnState(controller.fromCellPos, false);
        }
        
        MapManager.Instance.SetPawnState(task.target_position, true);
    }
    public System.Collections.IEnumerator HandleBuildTask(Pawn pawn)
    {
        //todo:更改所有独立的移动逻辑改为调用移动任务的形式，尝试解决建筑任务中建筑提前完成建造的问题
        //需求调用的边界处理需要再做考虑
        Debug.Log("HandleBuildTask开始");
        Debug.Log($"taskid: {pawn.handlingTask.task_id}");
        if (pawn == null || pawn.handlingTask == null)
        {
            Debug.LogWarning("Pawn 或其任务为空，无法执行任务！");
            yield break;
        }

        TaskManager.Task task = pawn.handlingTask;

        Vector3Int buildPosition = task.target_position;
        PawnInteractController controller = pawn.Instance.GetComponent<PawnInteractController>();
        if (controller == null)
        {
            Debug.LogWarning("PawnInteractController 未挂载在 Pawn 实例上！");
            yield break;
        }

        MapManager.MapData targetData = MapManager.Instance.GetMapData(task.target_position);
        if (targetData != null && targetData.item is ItemInstanceManager.PrintInstance printInstance)
        {
            // 完成材料列表的运输任务,通过循环发布运输任务实现
            var requirement = printInstance.GetOneRequirement();
            Debug.Log($"获取到的第一个需求: {requirement}");
            while (!requirement.Equals(new KeyValuePair<int, int>(-1, -1)))
            {
                // 1. 获取最近的材料位置(相对于建造位置)

                int requireId = requirement.Key;
                int requireAmount = requirement.Value;
                Vector3Int pos = ItemInstanceManager.Instance.FindNearestItemPosition(requireId, buildPosition);
                if (pos == Vector3Int.zero)
                {
                    Debug.LogWarning("没有找到最近的材料位置，无法执行运输任务！");
                    yield break;
                }
                // 2. 创建并执行运输任务
                TaskManager.TransportTask transporttask = TaskManager.CreateTransportTask(pos, buildPosition, requireId, requireAmount);
                TaskManager.Task buildtask = pawn.handlingTask; // 保存当前建造任务
                pawn.handlingTask = transporttask; // 更新当前任务为运输任务
                AddPawnTask(pawn, buildtask);

                HandleTask(pawn);
                yield return new WaitUntil(() => !pawn.isOnTask);

                //yield return StartCoroutine(HandleTask(pawn));
                //yield return StartCoroutine(HandleToPrintInstanceTransportTask(pawn, transporttask));
                //等待一次运输任务执行完成后再执行下次运输任务
                //注意这里的任务执行并不会
                //yield return StartCoroutine(HandleTransportTask(pawn, transporttask));

                requirement = printInstance.GetOneRequirement();
                Debug.Log($"获取到的下一个需求: {requirement}");
            }
            Debug.Log("所有材料运输任务已完成，开始建造任务。");

            if (printInstance.IsFinished())
            {
                float workTime = GetWorkTime(pawn, task);
                Debug.Log($"开始建造任务，预计耗时: {workTime} 秒");
                yield return new WaitForSeconds(2);
                Debug.Log("buildingTask 完成!");

                ResolveTask(pawn);
                //yield return StartCoroutine(ResolveTask(pawn));
                yield break;

            }
            else
            {
                Debug.Log("somethings wrong");
            }
        }

        Debug.Log("建造任务完成！");
    }
    public System.Collections.IEnumerator HandleBuildFarmTask(Pawn pawn){
        Debug.Log("HandleBuildFarmTask开始");
        if (pawn == null || pawn.handlingTask == null){
            Debug.LogWarning("Pawn 或其任务为空，无法执行任务！");
            yield break;
        }
        TaskManager.Task task = pawn.handlingTask;

        Vector3Int? FarmPosition = task.target_position;
        PawnInteractController controller = pawn.Instance.GetComponent<PawnInteractController>();
        if (controller == null){
            Debug.LogWarning("PawnInteractController 未挂载在 Pawn 实例上！");
            yield break;
        }

        // 判断是否位于蓝图周围的可站位置
        if (Vector3Int.Distance(
                MapManager.Instance.GetCellPosFromWorld(pawn.Instance.transform.position),
                FarmPosition.Value) != 1)
        {
            TaskManager.Task build_farm_task = pawn.handlingTask;
            ClearPawnTaskList(pawn);

            // 假设位于下侧，可根据需要调整
            pawn.handlingTask = new TaskManager.Task(
                position: FarmPosition.Value + new Vector3Int(0, -1, 0),
                type: TaskManager.TaskTypes.Move,
                task_id: -1
            );
            AddPawnTask(pawn, build_farm_task);

            HandleTask(pawn);
            //yield return StartCoroutine(HandleTask(pawn));
            Debug.Log("Pawn拆分BuildFarm任务完成");
        }
        else
        {
            float workTime = GetWorkTime(pawn, task);
            Debug.Log($"开始开垦任务，预计耗时: {workTime} 秒");
            yield return new WaitForSeconds(workTime);
            Debug.Log("FarmTask 完成!");

            MapManager.MapData mapData = MapManager.Instance.GetMapData(FarmPosition.Value);
            if (mapData == null)
            {
                Debug.LogWarning("目标位置的 MapData 不存在，无法创建农田！");
                yield break;
            }
            MapManager.Instance.SetTileFarm(mapData, BuildManager.Instance.GetBuilding(task.id));
            ResolveTask(pawn);
            //yield return StartCoroutine(ResolveTask(pawn));
        }
    }
    //收割任务实现，暂且测试树木产生木材
    //采用协程形式处理等待问题
    public System.Collections.IEnumerator HandleHarvestTask(Pawn pawn){
        if (pawn == null || pawn.handlingTask == null){
            Debug.LogWarning("Pawn 或其任务为空，无法执行任务！");
            yield break;
        }
        TaskManager.Task task = pawn.handlingTask;
        Vector3Int? cropPosition = task.target_position;
        

        // 2. 移动 Pawn 到农作物位置
        PawnInteractController controller = pawn.Instance.GetComponent<PawnInteractController>();
        if (controller == null){
            Debug.LogWarning("PawnInteractController 未挂载在 Pawn 实例上！");
            yield break;
        }

        controller.MovePawnToPosition(cropPosition.Value, pawn);


        yield return new WaitWhile(() => controller.isMoving);

        Debug.Log("Pawn 到达农作物位置,开始HarvestTask...");

        // 3. 等待收割完成
        float workTime = GetWorkTime(pawn, task);
        yield return new WaitForSeconds(0);

        // 4. 收割完成后，创建物品实例
        // 这里假设收割的物品是木材，实际情况可能需要根据任务类型来判断
        MapManager.MapData mapData = MapManager.Instance.GetMapData(cropPosition.Value);
        Debug.Log($"目标位置的 MapData: {mapData.item}");
        if (mapData.item != null)
        {
            //判断是否为树木
            Debug.LogWarning("1");
            // if (mapData.item.id == 5)
            // {
                //mapmanager中的item操纵接口以更改地块上的has_item,重复了
                //MapManager.Instance.SethasitemState(mapData, true);
                if (mapData.item is ItemInstanceManager.CropInstance cropInstance)
                {
                    Debug.LogWarning("目标物品是 CropInstance调用 HarvestCrop");
                    ItemInstanceManager.Instance.HarvestCrop(cropInstance);
                }
                else
                {
                    Debug.LogWarning("目标物品不是 CropInstance，无法调用 HarvestCrop！");
                }

                // ItemInstanceManager.MaterialInstance newCrop = ItemInstanceManager.Instance.SpawnItem(
                //     task.target_position,
                //     task.id,
                //     ItemInstanceManager.ItemInstanceType.MaterialInstance
                // ) as ItemInstanceManager.MaterialInstance;
                Debug.Log($"收割任务完成，物品已创建在位置: {task.target_position}");
                //下述代码用于测试地图数据更新情况
                MapManager.MapData NEWMAPDATA = MapManager.Instance.GetMapData(cropPosition.Value);
                if (NEWMAPDATA != null)
                {
                    Debug.Log($"目标位置的 MapData 更新成功，has_item: {NEWMAPDATA.has_item}");
                    if (NEWMAPDATA.item != null)
                    {
                        Debug.Log($"目标位置：{cropPosition.Value}  目标位置的物品实例 ID: {NEWMAPDATA.item.id}");
                    }
                    else
                    {
                        Debug.Log("目标位置的物品实例为空！");
                    }
                }
                else
                {
                    Debug.LogWarning("目标位置的 MapData 不存在，无法创建物品！");
                }

            // }
            // else
            // {
            //     Debug.LogWarning("目标位置的物品不是树木，无法收割！");

            //     yield break;
            // }
        }


        // MapManager.MapData cropData = MapManager.Instance.GetMapData(cropPosition.Value);
        // if (cropData != null && !cropData.has_item){
        //     cropData.has_item = true;
        //     //暂时由任务来指定生成出来的物品，用于测试，后续需要根据作物收获物表格获取物品
        //     ItemInstanceManager.MaterialInstance newCrop = ItemInstanceManager.Instance.SpawnItem(
        //         task.target_position, 
        //         task.id, 
        //         ItemInstanceManager.ItemInstanceType.MaterialInstance
        //     ) as ItemInstanceManager.MaterialInstance;
        //     Debug.Log($"收割任务完成，物品已创建在位置: {task.target_position}");
        // }
    }
    
    //任务结束时进行的更新
    Dictionary<TaskManager.TaskTypes, Action<Pawn>> taskResolver = new Dictionary<TaskManager.TaskTypes, Action<Pawn>>{
        { TaskManager.TaskTypes.Move, (pawn) => Instance.ResolveMoveTask(pawn)},
        { TaskManager.TaskTypes.Build, (pawn) => Instance.StartCoroutine(Instance.ResolveBuildTask(pawn)) },
        { TaskManager.TaskTypes.Harvest, (pawn) => Instance.HandleHarvestTask(pawn)}
    };

    public void ResolveTask(Pawn pawn){
        if (pawn == null || pawn.handlingTask == null){
            Debug.LogWarning("Pawn 或其任务为空，无法执行任务！");
            return;
        }
        TaskManager.Task task = pawn.handlingTask;

        if(taskResolver.ContainsKey(task.type)){
            taskResolver[task.type].Invoke(pawn);
        }
        else{
            Debug.LogWarning($"未处理的任务类型: {task.type}");
        }

        if (pawn.PawntaskList.Count > 0)
        {
            pawn.handlingTask = pawn.PawntaskList[0];
            pawn.PawntaskList.RemoveAt(0);

            //谨防循环调度爆栈
            HandleTask(pawn); 
            //yield return StartCoroutine(HandleTask(pawn));
            
        }
        else
        {
            pawn.isOnTask = false;
        }
    }

    public void ResolveMoveTask(Pawn pawn){
        Debug.Log("ResolveMoveTask开始");
        if (pawn == null || pawn.handlingTask == null){
            Debug.LogWarning("Pawn 或其任务为空，无法执行任务！");
            return;
        }
        TaskManager.Task task = pawn.handlingTask;

        // 1. 获取目标地块的格子坐标
        Vector3Int targetCellPos = task.target_position;
        Debug.Log("Position:"+task.target_position);

        if (MapManager.Instance.IsWalkable(targetCellPos)){
            PawnInteractController controller = pawn.Instance.GetComponent<PawnInteractController>();
            if (controller != null){
                Debug.Log($"Pawn ID: {pawn.id} 到达目标位置: {targetCellPos}");
                MapManager.Instance.SetPawnState(targetCellPos, true);

                controller.fromCellPos = targetCellPos;
            }
            else{
                Debug.LogWarning("PawnInteractController 未挂载在 Pawn 实例上！");
            }
        }
        else{
            Debug.Log("这个触发了？");
            //TODO: 在即将抵达的时候目标地格变得不可移动，需要重新就近移动
        }
    }

    public System.Collections.IEnumerator ResolveBuildTask(Pawn pawn)
    {
        Debug.Log("ResolveBuildTask开始");

        if (pawn == null || pawn.handlingTask == null)
        {
            Debug.LogWarning("Pawn 或其任务为空，无法执行任务！");
            yield break;
        }
        TaskManager.Task task = pawn.handlingTask;

        Vector3Int targetCellPos = task.target_position;

        //TODO: 把该地块上的蓝图类型实例转化为
        MapManager.MapData mapData = MapManager.Instance.GetMapData(targetCellPos);
        if (mapData == null)
        {
            Debug.LogWarning("目标位置的 MapData 不存在，无法创建建筑！");
            yield break;
        }
        int buildingId = task.id;
        BuildManager.Building building = BuildManager.Instance.GetBuilding(buildingId);
        Debug.Log($"获取到的建筑: {building}");
        ItemInstanceManager.Instance.DestroyItem(mapData.item, ItemInstanceManager.DestroyMode.RemainNone);
        MapManager.Instance.SetTileBuild(mapData, building);
        // 如需等待动画或其他效果，可在此处 yield return 等待
        // yield return new WaitForSeconds(1.0f);

        Debug.Log("ResolveBuildTask完成");
    }

    #endregion
}
