using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
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
    }

    private List<Task> availableTaskList = new List<Task>(); // 满足条件的任务列表
    private List<Task> inavailableTaskList = new List<Task>(); // 不满足条件的任务列表

    private const int MAX_TASKS_PER_FRAME = 5; // 每帧检测任务上限

    // Start is called before the first frame update
    void Start(){
        pawnManager = FindObjectOfType<PawnManager>(); // 获取 PawnManager 实例
    }

    // Update is called once per frame
    void Update(){
        // 检测并分配 availableTaskList 中的任务
        for (int i = 0; i < Mathf.Min(MAX_TASKS_PER_FRAME, availableTaskList.Count); i++){
            Task task = availableTaskList[i];
            PawnManager.Pawn availablePawn = pawnManager.GetAvailablePawn();
            if (availablePawn != null){
                availablePawn.isOnTask = true;
                availablePawn.handlingTask = task;
                availableTaskList.RemoveAt(i);
                i--; // 调整索引以处理移除元素后的列表
            }
            else{
                inavailableTaskList.Add(task);
                availableTaskList.RemoveAt(i);
                i--; // 调整索引以处理移除元素后的列表
            }
        }

        // 检测并处理 inavailableTaskList 中的任务
        for (int i = 0; i < Mathf.Min(MAX_TASKS_PER_FRAME, inavailableTaskList.Count); i++){
            Task task = inavailableTaskList[i];
            PawnManager.Pawn availablePawn = pawnManager.GetAvailablePawn();
            if (availablePawn != null){
                availablePawn.isOnTask = true;
                availablePawn.handlingTask = task;
                inavailableTaskList.RemoveAt(i);
                i--; // 调整索引以处理移除元素后的列表
            }
            else{
                inavailableTaskList.Add(task); // 将任务移到列表尾部
                inavailableTaskList.RemoveAt(i);
                i--; // 调整索引以处理移除元素后的列表
            }
        }
    }
}
