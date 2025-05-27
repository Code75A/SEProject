using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PawnInteractController : MonoBehaviour
{
    public PawnManager.Pawn pawn;
    private UnityEngine.Vector3 targetPosition;
    
    public bool isMoving = false;
    
    public Vector3Int fromCellPos;
    private Vector3Int targetCellPos;

    public Tilemap landTilemap;
    public GameObject content;

    void FixedUpdate(){
        // 移动逻辑
        if (isMoving)
        {
            //Debug.Log($"移动参数 | 速度: {pawn.moveSpeed} | 帧时间: {Time.deltaTime}");
            //Debug.Log($"当前位置: {transform.position} | 目标位置: {targetPosition}");
            float landform_walkspeed = MapManager.Instance.GetMapData(landTilemap.WorldToCell(transform.position)).walk_speed;

            float step = pawn.moveSpeed * Time.deltaTime * content.transform.localScale.x * landform_walkspeed;
            targetPosition = landTilemap.GetCellCenterWorld(targetCellPos);

            //float step = 1.0f;
            transform.position = UnityEngine.Vector3.MoveTowards(transform.position, targetPosition, step);
            // 到达目标
            if (UnityEngine.Vector3.Distance(transform.position, targetPosition) < 0.0005f)
            {
                isMoving = false;
                PawnManager.Instance.ResolveTask(pawn);

                //Debug.Log($"到达目标位置: {targetPosition}");
                
            }
        }
        
    }
    // 移动函数
    //传入目标世界坐标移动到指定位置
    //todo:移动过程中不可通行区域判定，地块速度影响等
    // public void MovePawnToPosition(Vector3 targetWorldPos,PawnManager.Pawn targetpawn)
    // {
        
    //     // 转换世界坐标到格子坐标
    //     //Vector3Int targetCellPos = MapManager.Instance.GetCellPosFromWorld(targetWorldPos);
        
    //     // 检查目标位置是否可通行
    //     if (!MapManager.Instance.IsWalkable(targetCellPos) || 
    //         MapManager.Instance.HasPawnAt(targetCellPos))
    //     {
    //         Debug.LogWarning($"目标位置不可通行: {targetWorldPos}");
    //         return;
    //     }

    //     // 设置移动参数
    //     targetPosition = targetWorldPos;
    //     targetPosition.z = 0; // 确保Z坐标为0
    //     //targetCellPos = MapManager.Instance.GetCellPosFromWorld(targetPosition);
    //     isMoving = true;
        
    //     Debug.Log($"开始移动Pawn到位置: {targetPosition}");
    // }

    // 仅移动，不设置任务
    public void MovePawnToPosition(Vector3Int targetCellPos, PawnManager.Pawn targetPawn){
        if (targetPawn == null){
            Debug.LogWarning("目标Pawn为空，无法移动！");
            return;
        }
        // 检查目标位置是否可通行
        if (!MapManager.Instance.IsWalkable(targetCellPos) || 
            MapManager.Instance.HasPawnAt(targetCellPos)){

            Debug.LogWarning($"目标位置不可通行: {targetPosition}");
            Debug.LogWarning(MapManager.Instance.GetMapData(targetCellPos));

            return;
        }

        // 设置移动参数
        isMoving = true;
        this.targetCellPos  = targetCellPos;
        targetPosition = landTilemap.GetCellCenterWorld(targetCellPos);

    }
    public void MovePawnToPositionByPlayer(Vector3Int onMouseCellPos){
        // 判断目标格子是否可以通行并且没有Pawn
        if (MapManager.Instance.IsWalkable(onMouseCellPos) && !MapManager.Instance.HasPawnAt(onMouseCellPos)){
            isMoving = true;

            if (pawn != null){
                if(pawn.handlingTask != null){
                    if(pawn.handlingTask.type != TaskManager.TaskTypes.Move){
                        //TODO 处理其它目前正在处理的任务（返还给taskManger）
                    }
                    else{
                        MapManager.Instance.SetPawnState(pawn.handlingTask.target_position, false);
                    }
                }
            
                pawn.handlingTask = new TaskManager.Task(onMouseCellPos, TaskManager.TaskTypes.Move, 0, -1, -1);

                PawnManager.Instance.HandleTask(pawn);
            }
        }
        else{
            Debug.Log("目标位置不可通行或已有其他角色！");
        }
    }



}