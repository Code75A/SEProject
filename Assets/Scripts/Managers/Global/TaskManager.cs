using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using System.Linq;
public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }
    public PawnManager pawnManager; // 引用唯一的 PawnManager 对象



    public enum TaskTypes{
        Build, // 建造
        Plant, // 种植
        Harvest, // 收割
        Total // 用于获取任务种类总数
    }

    public class Task{
        public Vector3Int position; // 完成此任务的地点
        public TaskTypes type; // 任务类型

        public int id;//任务id，每一个任务的唯一标识

        public int MaterialId;//需要材料的id

        public int MaterialAmount; //需要材料的数量
        public Task(Vector3Int position,TaskTypes type,int id,int materialId,int materialAmount,int materialType){
            this.position = position;
            this.type = type;
            this.id = id;
            this.MaterialId = materialId;
            this.MaterialAmount = materialAmount;
        }

    }
    //todo:添加任务时自动为任务分配id的taskid生成器
    private int TaskIdUpdate(){
        //todo:此处应有逻辑生成id，此处仅为示意
        
        return 0;
    }
    public List<Task> availableTaskList = new List<Task>(); // 满足条件的任务列表
    public List<Task> inavailableTaskList = new List<Task>(); // 不满足条件的任务列表

    private const int MAX_TASKS_PER_FRAME = 5; // 每帧检测任务上限

    void Awake(){
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }
    
    // Start is called before the first frame update
    void Start(){
        pawnManager = FindObjectOfType<PawnManager>(); // 获取 PawnManager 实例
    }

    // Update is called once per frame
    void Update() {
        // 检测并分配 availableTaskList 中的任务
        for (int i = 0; i < MAX_TASKS_PER_FRAME; i++) {
            if(availableTaskList.Count == 0) break; // 防止 availableTaskList 为空时的异常
            Task task = availableTaskList[0];
            PawnManager.Pawn availablePawn = pawnManager.GetAvailablePawn();
            if (availablePawn != null) {
                availablePawn.isOnTask = true;
                availablePawn.handlingTask = task;
                availableTaskList.RemoveAt(0);
            } else {
                inavailableTaskList.Add(task);
                availableTaskList.RemoveAt(0);
            }
        }

        // 检测并处理 inavailableTaskList 中的任务
        for (int i = 0; i < MAX_TASKS_PER_FRAME; i++) {
            if(inavailableTaskList.Count == 0) break; // 防止 inavailableTaskList 为空时的异常
            Task task = inavailableTaskList[0];
            PawnManager.Pawn availablePawn = pawnManager.GetAvailablePawn();
            if (availablePawn != null) {
                availablePawn.isOnTask = true;
                availablePawn.handlingTask = task;
                inavailableTaskList.RemoveAt(0);
            } else {
                inavailableTaskList.Add(task); // 将任务移到列表尾部
                inavailableTaskList.RemoveAt(0); // 移除列表首部的任务                
            }
        }
    }

    // public void AddTask(Vector3Int pos, TaskTypes type) {
    //     Task newTask = new Task {
    //         position = pos,
    //         type = type
    //     };
    //     availableTaskList.Add(newTask);
    // }
    public void AddTask(Vector3Int pos, TaskTypes type, int materialId = 0, int materialAmount = 0) {
        int newTaskId = TaskIdUpdate(); // 需要实现ID生成器
        
        Task newTask = new Task(
            position: pos,
            type: type,
            id: newTaskId,
            materialId: materialId,
            materialAmount: materialAmount,
            materialType: -1 // 默认为-1，表示不需要材料或材料类型未知
        );
        availableTaskList.Add(newTask);
    }
}
