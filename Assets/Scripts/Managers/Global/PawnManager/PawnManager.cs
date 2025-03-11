
using System.Collections;
using System.Collections.Generic;
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
        public Task handlingTask = null; // 记录 Pawn 当前正在处理的任务  
        public float moveSpeed = 1.0f; // 移动速度（默认 100%）
        public float workSpeed = 1.0f; // 工作速度（默认 100%）
        public Vector2 position; // 当前位置
        public Tool handlingTool; // 当前所持工具  
    }

    // 添加新的 Pawn，并将其实例化
    public void AddPawn(Vector2 spawnPosition){
        GameObject newPawnObj = Instantiate(pawnPrefab, spawnPosition, Quaternion.identity); // 实例化 Pawn 预设体
        Pawn newPawn = new Pawn { id = pawns.Count, position = spawnPosition }; // 创建新 Pawn 并分配唯一 ID
        pawns.Add(newPawn); // 将 Pawn 添加到列表
        
        // 如果当前没有选择 Pawn，则默认选中第一个创建的 Pawn
        if (SelectingPawn == null){
            SelectingPawn = newPawn;
        }
    }

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
}
