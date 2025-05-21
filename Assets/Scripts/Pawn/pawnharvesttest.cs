using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pawnharvesttest : MonoBehaviour
{
    // Start is called before the first frame update
    public void TestHarvestTaskByGrid()
    {
        // 1. 在 (0,0) 放置树木
        Vector3Int treePos = new Vector3Int(20, 20, 0);
        ItemInstanceManager.Instance.SpawnItem(treePos, 5, ItemInstanceManager.ItemInstanceType.CropInstance);


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
    }
}
