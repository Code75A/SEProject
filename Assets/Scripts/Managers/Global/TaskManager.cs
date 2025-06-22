using System.Collections.Generic;
using UnityEngine;
public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }
    public PawnManager pawnManager; // 引用唯一的 PawnManager 对象

    //private int dev_current_id = 0; // 当前任务id-临时


    public enum TaskTypes
    {
        Move,
        Build,
        PlantALL,//调用种植任务时使用
        PlantOnly,//种植任务自己使用
        Harvest,
        Transport,//单纯运输任务，不涉及蓝图交互、箱子交互等内容
        BuildingTransport, // 建造运输任务,需要与蓝图进行交互
        GetTool, // 获取工具任务
        Total // 用于获取任务种类总数
    }

    //任务类
    //对于建造类型任务，根据材料、数量及位置能确定好任务的信息以完整指挥pawn运动

    //对于种植类型任务，materialid中存储种子id，materialamount存储种子数量，其中materialamount默认为一个
    //既单地块任务接口。
    //实际过程中考虑合并多个任务，todo: 任务合并（如指定一片区域进行种植任务，统一拿种子但分别播种）

    //对于收割类型任务，materialid 用来表示待收割物品类型，materialamount暂定没有作用，通过待收割物品类型与掉落物的映射确定掉落物
    //注意收割物品和掉落物映射的维护
    public class Task
    {
        public Vector3Int target_position; // 完成此任务的地点

        public TaskTypes type; // 任务类型

        public int task_id;//任务id，每一个任务的唯一标识

        //不只有材料，可能改名 --cjh
        public int id;//需要材料的id，建造任务中建筑的id

        public int amount; //需要材料的数量,在运输任务中指代需要运输的物品数量
        //该数量可能并非实际运输的量，当需求量大于单地块上物品数量时，可能需要多次运输


        public int materialType;

        public int tasklevel = 0; //任务等级，表示任务的难度和复杂程度

        public Task(Vector3Int position, TaskTypes type, int task_id, int id = 0, int amount = 0, int materialType = -1)
        {
            this.target_position = position;
            this.type = type;
            this.task_id = task_id;
            this.id = id;
            this.amount = amount;
            //this.materialType = materialType;
        }

    }

    public class PlantALLTask : Task
    {
        public List<Vector3Int> Plant_positions; // 完成此任务的地点列表，可能是多个地块
        public PlantALLTask(Vector3Int position, TaskTypes type, int task_id, int id, List<Vector3Int> plant_positions)
        : base(position, type, task_id, id) // 调用基类构造函数，传递 id
        {
            this.Plant_positions = plant_positions;
        }
    }

    // public class BuildingTask : Task{
    //     public List<int> materialIds;    // 材料 ID 列表
    //     public List<int> materialAmounts; // 对应的每种材料数量
    //     public List<int> materialTypes;   // 材料类型（如区分泥土、木材等）

    //     public BuildingTask(Vector3Int position, TaskTypes type, int task_id)
    //         : base(position, type, task_id)
    //     {
    //         materialIds = new List<int>();
    //         materialAmounts = new List<int>();
    //         materialTypes = new List<int>();
    //     }

    //     public BuildingTask(Vector3Int position, TaskTypes type, int task_id,
    //                         List<int> materialIds, List<int> materialAmounts, List<int> materialTypes)
    //         : base(position, type, task_id)
    //     {
    //         this.materialIds = materialIds;
    //         this.materialAmounts = materialAmounts;
    //         this.materialTypes = materialTypes;
    //     }
    // }
    //检查第index位置上的材料，如果为需求数量为0则删除
    // public bool DeleteMaterial(BuildingTask task, int index = 0)
    // {
    //     if (task.materialAmounts[index] == 0)
    //     {
    //         task.materialIds.RemoveAt(index);
    //         task.materialAmounts.RemoveAt(index);
    //         task.materialTypes.RemoveAt(index);
    //         return true;
    //     }
    //     return false;
    // }
    //运输任务类，继承自任务类
    public class TransportTask : Task
    {
        public Vector3Int beginPosition; // 运输任务的起始位置

        public TransportTask(Vector3Int position, TaskTypes type, int task_id, int id, int amount, Vector3Int beginPosition) : base(position, type, task_id, id, amount, -1)
        {
            this.beginPosition = beginPosition;
        }
    }

    // 创建一个简易的 TransportTask
    public TransportTask CreateTransportTask(Vector3Int beginPos, Vector3Int endPos, int itemId, int amount = 1)
    {
        int newTaskId = TaskIdUpdate();
        TransportTask transportTask = new TransportTask(
            position: endPos,
            type: TaskTypes.BuildingTransport,
            task_id: newTaskId,
            id: itemId,
            amount: amount,
            beginPosition: beginPos
        );
        Debug.Log($"创建运输任务：{transportTask.GetType()}");
        return transportTask;
    }


    public class MoveTask : Task
    {
        //public Vector3Int beginPosition; // 目标位置
        public MoveTask(Vector3Int position, TaskTypes type, int task_id, int id, int amount) : base(position, type, task_id, id, amount, -1)
        {

        }
    }


    //todo:添加任务时自动为任务分配id的taskid生成器
    public int TaskIdUpdate()
    {
        // 创建一个 HashSet 来存储所有已使用的任务 ID
        HashSet<int> usedIds = new HashSet<int>();

        // 遍历 TaskManager 中的任务列表，收集所有任务的 ID
        foreach (var task in availableTaskList)
        {
            usedIds.Add(task.id);
        }
        foreach (var task in inavailableTaskList)
        {
            usedIds.Add(task.id);
        }

        // 遍历 PawnManager 中的任务列表，收集所有任务的 ID
        foreach (var pawn in PawnManager.Instance.pawns)
        {
            if (pawn.handlingTask != null)
            {
                usedIds.Add(pawn.handlingTask.id);
            }
            foreach (var task in pawn.PawntaskList)
            {
                usedIds.Add(task.id);
            }
        }

        // 找到未被使用的最小正整数 ID
        int newId = 1;
        while (usedIds.Contains(newId))
        {
            newId++;
        }

        return newId;
    }
    public List<Task> availableTaskList = new List<Task>(); // 满足条件的任务列表
    public List<Task> inavailableTaskList = new List<Task>(); // 不满足条件的任务列表

    private const int MAX_TASKS_PER_FRAME = 5; // 每帧检测任务上限

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        pawnManager = FindObjectOfType<PawnManager>(); // 获取 PawnManager 实例
    }

    // Update is called once per frame
    void Update()
    {
        // 检测并分配 availableTaskList 中的任务

        for (int i = 0; i < MAX_TASKS_PER_FRAME; i++)
        {
            if (availableTaskList.Count == 0) break; // 防止 availableTaskList 为空时的异常
            Task task = availableTaskList[0];
            PawnManager.Pawn availablePawn = pawnManager.GetAvailablePawn();
            if (availablePawn != null)
            {
                availablePawn.isOnTask = true;
                availablePawn.handlingTask = task;
                Debug.Log($"task type : {task.type}");
                availableTaskList.RemoveAt(0);

                PawnManager.Instance.HandleTask(availablePawn);
                //PawnManager.Instance.StartCoroutine(PawnManager.Instance.HandleTask(availablePawn));
            }
            else
            {
                //inavailableTaskList.Add(task);
                //availableTaskList.RemoveAt(0);
            }
        }

        // 检测并处理 inavailableTaskList 中的任务
        for (int i = 0; i < MAX_TASKS_PER_FRAME; i++)
        {
            if (inavailableTaskList.Count == 0) break; // 防止 inavailableTaskList 为空时的异常
            Task task = inavailableTaskList[0];
            PawnManager.Pawn availablePawn = pawnManager.GetAvailablePawn();
            if (availablePawn != null)
            {
                // availablePawn.isOnTask = true;
                // availablePawn.handlingTask = task;
                // inavailableTaskList.RemoveAt(0);

                // PawnManager.Instance.HandleTask(availablePawn);
                //PawnManager.Instance.StartCoroutine(PawnManager.Instance.HandleTask(availablePawn));
            }
            else
            {
                // inavailableTaskList.Add(task); // 将任务移到列表尾部
                // inavailableTaskList.RemoveAt(0); // 移除列表首部的任务                
            }
        }
    }

    // 添加任务到任务列表中
    // 将materialId改为id，materialAmount改为amount，方便后续扩展 --cjh
    public void AddTask(Vector3Int pos, TaskTypes type, int id = 0, int amount = 0)
    {
        int newTaskId = TaskIdUpdate(); // 需要实现ID生成器

        Task newTask = new Task(
            position: pos,
            type: type,
            task_id: newTaskId,
            id: id,
            amount: amount,
            materialType: -1 // 默认为-1，表示不需要材料或材料类型未知
        );
        Debug.Log($"添加任务: {newTask.type}，ID: {newTask.task_id}，位置: {newTask.target_position}，材料ID: {newTask.id}，数量: {newTask.amount}");

        availableTaskList.Add(newTask);
    }
    
    //添加失败的任务
    public void AddInavailableTask(Task task)
    {
        inavailableTaskList.Add(task);
    }
}
