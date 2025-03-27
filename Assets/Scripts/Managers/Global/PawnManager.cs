
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PawnManager : MonoBehaviour{
    public static PawnManager Instance { get; private set; } // 单例模式，确保全局唯一
    public TaskManager TaskManager; // 引用唯一的 TaskManager 对象
    public ItemManager ItemManager; // 引用唯一的 ItemManager 对象
    public GameObject pawnPrefab; // Pawn 预设体，用于实例化 Pawn
    public Pawn SelectingPawn; // 当前被选中的 Pawn
    private List<Pawn> pawns = new List<Pawn>(); // 存储所有 Pawn 对象的列表


    private void Awake(){
        // 实现单例模式，确保 PawnManager 只有一个实例
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    // Pawn 类的定义
    public class Pawn{
        public int id; // 游戏内唯一标识符
        public bool isOnTask = false; // 记录 Pawn 是否正在执行任务

        public TaskManager.Task handlingTask = null; // 记录 Pawn 当前正在处理的任务  
        //public TaskManager.Task handlingTask = null; // 记录 Pawn 当前正在处理的任务  
        public float moveSpeed = 1.0f; // 移动速度（默认 100%）
        public float workSpeed = 1.0f; // 工作速度（默认 100%）
        public Vector2 position; // 当前位置
        public ItemManager.Tool handlingTool; // 当前所持工具 
        public List<TaskManager.Task> PawntaskList = new List<TaskManager.Task>(); // 存储单个小人的任务列表

        public GameObject Instance;  // 该 Pawn 的实例对象

        // 构造函数：在创建 Pawn 的时候自动实例化 GameObject
        public Pawn(int id, Vector2 startPos, GameObject pawnPrefab)
        {
            this.id = id;
            this.position = startPos;

            Instance = GameObject.Instantiate(pawnPrefab, startPos, Quaternion.identity);

            Instance.name = $"Pawn_{id}";

            PawnInteractController controller = Instance.AddComponent<PawnInteractController>();
            controller.pawn = this;
        }
    }
    // 创建一个 Pawn，并实例化 GameObject
    public void CreatePawn(Vector2 startPos)
    {
        int newId = pawns.Count + 1;

        // 创建 Pawn 对象并实例化 GameObject
        Pawn newPawn = new Pawn(newId, startPos, pawnPrefab);

        // 将 Pawn 添加到列表进行管理
        pawns.Add(newPawn);

        //Debug.Log($"Pawn_{newId} 已创建，位置：{startPos}");
    }

    // // 添加新的 Pawn，并将其实例化
    // public void AddPawn(Vector2 spawnPosition){
    //     GameObject newPawnObj = Instantiate(pawnPrefab, spawnPosition, Quaternion.identity); // 实例化 Pawn 预设体
    //     Pawn newPawn = new Pawn { id = pawns.Count, position = spawnPosition }; // 创建新 Pawn 并分配唯一 ID
    //     pawns.Add(newPawn); // 将 Pawn 添加到列表
        
    //     // 如果当前没有选择 Pawn，则默认选中第一个创建的 Pawn
    //     if (SelectingPawn == null){
    //         SelectingPawn = newPawn;
    //     }
    // }

    // 获取一个可用的 Pawn（即 isOnTask = false 的第一个对象）用于返回给TaskManager
    public Pawn GetAvailablePawn(){
        foreach (var pawn in pawns){
            if (!pawn.isOnTask){
                return pawn;
            }
        }
        return null; // 若无可用 Pawn，则返回 null
    }

    // 选择指定 ID 的 Pawn
    public void SelectPawn(int id){
        SelectingPawn = pawns.Find(pawn => pawn.id == id);
    }
    //按住右键清除当前选中pawn
    public void deleteselectPawn(){
        // 检测鼠标右键是否按住
        if (Input.GetMouseButton(1)){
            SelectingPawn = null; // 清除当前选中的 Pawn
        }
    }
    // 设置当前选中的 Pawn
    public void SetSelectingPawn(Pawn pawn){
        if(pawn != null){
            SelectingPawn = pawn;
        }
    }
    //设置selectingPawn 对应的任务对象 Task
    public void SetSelectingPawnTask(TaskManager.Task task){
        if(task != null){
            //PawnManager.Instance.SelectingTask = task;
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
    //当小人当前空闲并且任务列表中有任务的时候，获取下一个任务并分配给当前空闲的小人
    //应当在小人任务结束时调用该判定函数
    //todo:如果当前任务列表为空，可以对玩家进行提示小人空闲
    public void GetNextTask(Pawn pawn){
        if(pawn != null){
            if(pawn.isOnTask == false && pawn.PawntaskList.Count > 0){
                pawn.handlingTask = pawn.PawntaskList[0];//todo:这里默认获取任务列表中的第一个任务，后续可以考虑根据任务优先级优化
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
    //对pawntasklist的内部维护函数，不涉及task的加入
    //当tasklist的第一个task完成后调用，暂不考虑其他情况的调用
    private void PawnTaskListUpdate(Pawn pawn){
        if(pawn.handlingTask == null){
            Debug.LogWarning("没有正在处理的任务，无法更新任务列表！");
            return;
        }
        int removedCount = pawn.PawntaskList.RemoveAll(task => task.id == pawn.handlingTask.id);
        //目前暂定在task中加入id这一属性，便于对于任务的定位

        if(removedCount > 0){
            pawn.isOnTask = false;
            pawn.handlingTask = null;
            Debug.Log("任务已成功完成并从列表中移除！");
        }
        else{
            Debug.LogWarning("任务已不在列表中，可能已被其他逻辑移除！");
        }
    }

    //对pawntasklist的外部维护函数，负责task的加入
    public void AddPawnTask(int pawnID, TaskManager.Task task){
        if(task != null){
            //var pawn = pawns.Find(pawn => pawn.id == pawnID);
            SelectPawn(pawnID);//选择当前id的小人
            if(SelectingPawn != null){
                //Debug.Log($"✅ 添加 Pawn ID: {pawn.id} 的任务为 Task ID: {task.id}");  
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

}
