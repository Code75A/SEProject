
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PawnManager : MonoBehaviour{
    public static PawnManager Instance { get; private set; } // 单例模式，确保全局唯一
    public TaskManager TaskManager = TaskManager.Instance; // 引用唯一的 TaskManager 对象
    public ItemManager ItemManager = ItemManager.Instance; // 引用唯一的 ItemManager 对象
    public GameObject pawnPrefab; // Pawn 预设体，用于实例化 Pawn
    public Pawn SelectingPawn; // 当前被选中的 Pawn
    public List<Pawn> pawns = new List<Pawn>(); // 存储所有 Pawn 对象的列表
    public GameObject spawner; // 生成器对象，用于生成 Pawn

    public Tilemap landTilemap; // 地图的 Tilemap 对象，用于获取格子中心位置
    public GameObject content; // 地图的内容对象

    private void Awake(){
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }
    //初始时创建三个小人
    // private void Start(){
    //     CreatePawn(new Vector2(0, 0));
    //     CreatePawn(new Vector2(2, 0));
    //     CreatePawn(new Vector2(4, 0));
    // }

    public class Pawn{
        public int id;
        public bool isOnTask = false;
        public TaskManager.Task handlingTask = null;
        public float moveSpeed = 2.0f;
        public float workSpeed = 1.0f;

        public int capacity;//运载容量

        //public Vector2 position; 
        //需要存储什么样的位置？和transform.position作何区别？是一个快捷访问，还是存储其地格坐标？如果是后者，应该改用Vector3Int --cjh
        public ItemManager.Tool handlingTool;
        public List<TaskManager.Task> PawntaskList = new List<TaskManager.Task>();
        public GameObject Instance;

        public Pawn(int id, Vector3Int startPos, GameObject pawnPrefab)
        {
            this.id = id;

            Instance = GameObject.Instantiate(pawnPrefab,PawnManager.Instance.spawner.transform);
            Instance.name = $"Pawn_{id}";
            
            // 设置位置
            Vector3 worldPosition = PawnManager.Instance.landTilemap.GetCellCenterWorld(startPos);
            this.Instance.transform.position = worldPosition;
            // 消除缩放影响
            Vector3 contentLossyScale = PawnManager.Instance.content.transform.lossyScale;
            Vector3 contentLocalScale = PawnManager.Instance.content.transform.localScale;
            Vector3 totalScale = new Vector3(
            contentLocalScale.x / contentLossyScale.x,
            contentLocalScale.y / contentLossyScale.y,
            contentLocalScale.z / contentLossyScale.z
            );
            Instance.transform.localScale = totalScale ;
            //Instance.transform.localScale = new Vector3(0.5f, 0.5f, 1);

            // 获取已经挂在预制体上的 PawnInteractController 脚本
            PawnInteractController controller = Instance.GetComponent<PawnInteractController>();
            if (controller != null)
            {
                controller.pawn = this; // 关键点
                controller.landTilemap = PawnManager.Instance.landTilemap; 
                controller.content = PawnManager.Instance.content; 
            }
            else
            {
                Debug.LogWarning("PawnInteractController 没有挂载在 Pawn 预制体上！");
            }
        }
    }

    //根据工具增强属性修改移动速度和搬运容量
    //todo:增加放下工具的处理逻辑，重置基础属性

    public void GetToolAttribute(Pawn pawn, ItemManager.Tool tool){
        //暂定比例增强，可后续改动算法
        float baseSpeed = pawn.moveSpeed; 
        float speedModifier = 1 + (tool.enhancements[ItemManager.Tool.EnhanceType.Speed] / 100f);
        float actualSpeed = baseSpeed * speedModifier;
        pawn.moveSpeed = actualSpeed;
        //todo:搬运容量itemManager.tool.capacity尚未实现，暂时不修改搬运容量

    }

    public void CreatePawn(Vector3Int startPos)
    {
        int newId = pawns.Count + 1;
        Pawn newPawn = new Pawn(newId, startPos, pawnPrefab);
        pawns.Add(newPawn);
    }
    //根据位置创建小人，用于在部分任务中调用
    public void CreatePawnAtPosition(Vector3Int position){
        CreatePawn(position);
    }

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

    //由于暂定右键移动，暂时取消取消选择这部分，或者后续可以改成长按取消选择

    // public void deleteselectPawn(){
    //     if (Input.GetMouseButton(1)){
    //         SelectingPawn = null;
    //     }
    // }

    public void SetSelectingPawn(Pawn pawn){
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
        int removedCount = pawn.PawntaskList.RemoveAll(task => task.id == pawn.handlingTask.id);

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
            Debug.Log($"任务失败，移除 Task ID: {pawn.handlingTask.id}，尝试执行下一个任务");

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

}
