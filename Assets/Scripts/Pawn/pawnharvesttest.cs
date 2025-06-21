using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pawnharvesttest : MonoBehaviour
{
    // Start is called before the first frame update
    public void TestHarvestTaskByGrid()
    {
        // // 1. 在 (0,0) 放置树木
        Vector3Int treePos = new Vector3Int(20, 20, 0);
        // ItemInstanceManager.Instance.SpawnItem(treePos, 5, ItemInstanceManager.ItemInstanceType.CropInstance);


        // 2. 在 (100,100) 创建 Pawn
        Vector3Int pawnPos = new Vector3Int(10, 10, 0);
        PawnManager.Instance.CreatePawn(pawnPos);
        PawnManager.Pawn testPawn = PawnManager.Instance.GetAvailablePawn();
        if (testPawn == null)
        {
            Debug.LogError("创建 Pawn 失败！");
            return;
        }

        // 3. 创建收割任务
        TaskManager.Task harvestTask = new TaskManager.Task(
            position: treePos,
            type: TaskManager.TaskTypes.Harvest,
            task_id: 1,
            id: 4
        );
        testPawn.handlingTask = harvestTask;

        // 4. 启动收割任务
        StartCoroutine(PawnManager.Instance.HandleHarvestTask(testPawn));

        
    }
    public void createtree()
    {
        Vector3Int treePos = new Vector3Int(20, 20, 0);
        ItemInstanceManager.Instance.SpawnItem(treePos, 5, ItemInstanceManager.ItemInstanceType.CropInstance);
    }

    public void TestPawnUnload()
    {
        // 1. 获取一个空闲 Pawn，如果没有就创建一个
        PawnManager.Pawn testPawn = PawnManager.Instance.GetAvailablePawn();

        // 2. 先给 Pawn 装载一些物品（这里随意模拟）
        testPawn.materialId = 3;
        testPawn.materialAmount = 5;
        testPawn.instantCapacity -= 5;

        // 3. 调用 PawnUnload
        int unloadSuccess = PawnManager.Instance.PawnUnload(testPawn);

        // // 4. 打印结果
        // if (unloadSuccess)
        // {
        //     Debug.Log("PawnUnload 测试成功，已在当前位置生成对应物品。");
        // }
        // else
        // {
        //     Debug.LogWarning("PawnUnload 测试失败，未成功生成物品。");
        // }
    }
    public void TestTransportTask()
    {
        // 1. 在 (15,15) 放置一个物品（例如 ID=3 的材料）
        Vector3Int itemPos = new Vector3Int(15, 15, 0);
        ItemInstanceManager.Instance.SpawnItem(itemPos, 3, ItemInstanceManager.ItemInstanceType.MaterialInstance);
        Debug.Log("已在 (15,15) 放置了物品 ID=3...");

        // 2. 新建或获取一个空闲 Pawn
        PawnManager.Pawn testPawn = PawnManager.Instance.GetAvailablePawn();
        if (testPawn == null)
        {
            PawnManager.Instance.CreatePawn(new Vector3Int(10, 10, 0));
            testPawn = PawnManager.Instance.GetAvailablePawn();
        }
        Debug.Log($"选中的测试 Pawn ID：{testPawn.id}");

        // 3. 创建运输任务 (从 itemPos 搬运到 (20,20))
        TaskManager.TransportTask tTask = TaskManager.Instance.CreateTransportTask(itemPos, new Vector3Int(20, 20, 0), 3,10);
        testPawn.handlingTask = tTask;

        // 4. 启动运输协程
        StartCoroutine(PawnManager.Instance.HandleTransportTask(testPawn, tTask));
        Debug.Log("启动运输任务测试，Pawn将移动到物品位置装载，再移动到(20,20)卸载。");
    }
    public IEnumerator TestHarvestTask()
    {
        // 若没有空闲 Pawn, 先创建一个
        PawnManager.Pawn pawn = PawnManager.Instance.GetAvailablePawn();
        if (pawn == null)
        {
            PawnManager.Instance.CreatePawn(new Vector3Int(10, 10, 0));
            pawn = PawnManager.Instance.GetAvailablePawn();
        }

        // 记录分配前的任务状态
        int preTaskCount = pawn.PawntaskList.Count;

        // 添加一个收割任务
        Vector3Int testPos = new Vector3Int(5, 5, 0);
        TaskManager.Instance.AddTask(testPos, TaskManager.TaskTypes.Harvest, 3, 1);

        // 等待一帧，让 TaskManager 在 Update 中分配任务
        yield return null;

        // 检查分配结果
        if (pawn.handlingTask != null && pawn.handlingTask.type == TaskManager.TaskTypes.Harvest)
            Debug.Log("收割任务成功分配给 Pawn");
        else
            Debug.LogWarning("收割任务分配失败");

        Debug.Log($"分配前任务数: {preTaskCount}，分配后任务数: {pawn.PawntaskList.Count}");
    }
    // public void createwood()
    // {
    //     Vector3Int woodPos = new Vector3Int(2, 2, 0);
    //     MapManager.MapData woodData = MapManager.Instance.GetMapData(woodPos);
    //     if (woodData != null)
    //     {
    //         // 创建一份木材 MaterialInstance，数量设为20
    //         var woodItem = new ItemInstanceManager.MaterialInstance(ItemManager.Instance.GetItem("木材").id, 20);
    //         woodData.SetItem(woodItem);
    //         woodData.has_item = true;
    //     }
    // }

    public System.Collections.IEnumerator test_pawnload()
    {
        // 1. 获取一个空闲 Pawn，如果没有就创建一个
        PawnManager.Pawn testPawn = PawnManager.Instance.GetAvailablePawn();
        if (testPawn == null)
        {
            PawnManager.Instance.CreatePawn(new Vector3Int(10, 10, 0));
            testPawn = PawnManager.Instance.GetAvailablePawn();
        }

        // 2. 确保 Pawn 没有携带任何物品
        testPawn.materialId = 0; // 
        testPawn.materialAmount = 0;
        testPawn.instantCapacity -= 0;

        //创建10单位的木材实例
        //在20，20位置上生成
        ItemInstanceManager.Instance.SpawnItem(new Vector3Int(20, 20, 0), 5, ItemInstanceManager.ItemInstanceType.MaterialInstance, 10);

        // 3. 调用 PawnUnload
        MapManager.MapData beginData = MapManager.Instance.GetMapData(new Vector3Int(20, 20, 0));
        if (beginData != null && beginData.has_item &&
            beginData.item is ItemInstanceManager.MaterialInstance loadItem)
        {
            bool loadSuccess = PawnManager.Instance.PawnLoad(testPawn, loadItem, 5, new Vector3Int(20, 20, 0));
            Debug.Log($"测试装载物品：{testPawn.materialId}，数量：{testPawn.materialAmount}");
            if (!loadSuccess)
            {
                Debug.LogWarning("测试装载物品失败！");
                yield break;
            }
            Debug.Log("测试装载物品成功！");
        }
        //int unloadSuccess = PawnManager.Instance.PawnUnload(testPawn, 5);
    }
    //测试函数，针对HandlePlantALLTask
    public void TestHandlePlantAllTask()
    {
        // 1. 在 (30, 30) 处生成5颗稻种 (id=7)，示例可自行根据需要更换种子ID
        Vector3Int seedPos = new Vector3Int(30, 30, 0);
        int seedId = 7; // 稻种的ID
        ItemInstanceManager.Instance.SpawnItem(seedPos, seedId, ItemInstanceManager.ItemInstanceType.MaterialInstance, 5);

        // 2. 构造四个需要种植的地址
        List<Vector3Int> plantPositions = new List<Vector3Int>
        {
            new Vector3Int(36, 36, 0),
            new Vector3Int(37, 36, 0),
            new Vector3Int(38, 38, 0),
            new Vector3Int(39, 40, 0),
        };

        // 3. 创建一个 PlantALLTask
        TaskManager.PlantALLTask plantAllTask = new TaskManager.PlantALLTask(
            position: plantPositions[0], // 任务本身的参考坐标，可写成任意
            type: TaskManager.TaskTypes.PlantALL,
            task_id: TaskManager.Instance.TaskIdUpdate(),
            id: seedId,
            plant_positions: plantPositions
        );
        //TaskManager.Instance.availableTaskList.Add(plantAllTask);
        PawnManager.Pawn availablePawn = PawnManager.Instance.GetAvailablePawn();
        availablePawn.handlingTask = plantAllTask;
        StartCoroutine(PawnManager.Instance.HandlePlantALLTask(availablePawn));

        // 6. 验证结果：可自行在此处加入断言或输出日志
        // ...
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TestHarvestTaskByGrid();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartCoroutine(TestHarvestTask());
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            createtree();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            //TestPawnUnload();
            //TestTransportTask();
            //StartCoroutine(test_pawnload());
            TestHandlePlantAllTask();
        }
        
    }
}
