using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//TODO: 将之函数化
//if (pawn.handlingTask != null)
// if (pawn.handlingTask.type != TaskManager.TaskTypes.Move)
//  //TODO 处理其它目前正在处理的任务（返还给taskManger）
// else
//  MapManager.Instance.SetWillPawnState(pawn.handlingTask.target_position, false);

public class PawnInteractController : MonoBehaviour
{
    public PawnManager.Pawn pawn;
    private UnityEngine.Vector3 targetPosition;

    public bool isMoving = false;

    public Vector3Int fromCellPos;
    private Vector3Int targetCellPos;

    public Tilemap landTilemap;
    public GameObject content;

    void FixedUpdate()
    {
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

    private bool PawnCanMoveTo(Vector3Int targetCellPos)
    {
        return MapManager.Instance.IsWalkable(targetCellPos) &&
        !MapManager.Instance.HasPawnAt(targetCellPos) && !MapManager.Instance.WillHasPawnAt(targetCellPos);
    }

    // 仅移动，不设置任务
    public void MovePawnToPosition(Vector3Int targetCellPos, PawnManager.Pawn targetPawn)
    {
        if (targetPawn == null)
        {
            Debug.LogWarning("目标Pawn为空，无法移动！");
            return;
        }
        Debug.LogWarning($"尝试移动Pawn到位置: {targetCellPos}");
        // 检查目标位置是否可通行
        if (!PawnCanMoveTo(targetCellPos))
        {
            if (!MapManager.Instance.IsWalkable(targetCellPos))
            {
                Debug.LogWarning("目标位置不可通行1");
            }
            if (MapManager.Instance.HasPawnAt(targetCellPos))
            {
                Debug.LogWarning("目标位置不可通行2");
            }
            Debug.LogWarning($"目标位置不可通行: {targetPosition}");
            Debug.LogWarning(MapManager.Instance.GetMapData(targetCellPos));

            return;
        }

        Debug.Log($"开始移动Pawn到位置: {targetCellPos}");
        // 设置移动参数
        isMoving = true;
        this.targetCellPos = targetCellPos;
        targetPosition = landTilemap.GetCellCenterWorld(targetCellPos);

    }
    //移动且
    #region 由玩家指派任务
    public void MovePawnToPositionByPlayer(Vector3Int onMouseCellPos)
    {
        // 判断目标格子是否可以通行并且没有Pawn
        if (PawnCanMoveTo(onMouseCellPos))
        {
            isMoving = true;

            if (pawn != null)
            {
                if (pawn.handlingTask != null)
                {
                    if (pawn.handlingTask.type != TaskManager.TaskTypes.Move)
                    {
                        //TODO 处理其它目前正在处理的任务（返还给taskManger）
                    }
                    else
                    {
                        MapManager.Instance.SetWillPawnState(pawn.handlingTask.target_position, false);
                    }
                }

                List<Vector3Int> path = MapManager.Instance.FindPath(fromCellPos, onMouseCellPos);
                if (path.Count > 0)
                {
                    pawn.handlingTask = new TaskManager.Task(path[0], TaskManager.TaskTypes.Move, 0, -1, -1);
                    if (path.Count > 1)
                    {
                        for (int i = 1; i < path.Count; i++)
                            PawnManager.Instance.AddPawnTask(pawn, new TaskManager.Task(path[i], TaskManager.TaskTypes.Move, 0, -1, -1));
                    }
                }
                else
                {
                    Debug.LogWarning("无法找到路径，目标位置不可达");
                }



                PawnManager.Instance.HandleTask(pawn);
            }
        }
        else
        {
            Debug.Log("目标位置不可通行或已有其他角色！");
        }
    }
    public void GrowCropInPositionByPlayer(Vector3Int onMouseCellPos)
    {
        if (MapManager.Instance.IsPlantable(onMouseCellPos) && !MapManager.Instance.HasItemAt(onMouseCellPos))
        {
            if (pawn != null)
            {
                if (pawn.handlingTask != null)
                {
                    if (pawn.handlingTask.type != TaskManager.TaskTypes.Move)
                    {
                        //TODO 处理其它目前正在处理的任务（返还给taskManger）
                    }
                    else
                    {
                        MapManager.Instance.SetWillPawnState(pawn.handlingTask.target_position, false);
                    }
                }

                pawn.handlingTask = new TaskManager.Task(onMouseCellPos, TaskManager.TaskTypes.Plant, 0, -1, -1);

                PawnManager.Instance.HandleTask(pawn);
            }
        }
        else
        {
            Debug.Log("目标位置不可种植或已被占用！");
        }
    }
    public void HarvestAtPositionByPlayer(Vector3Int onMouseCellPos)
    {
        if (MapManager.Instance.HasItemAt(onMouseCellPos))
        {
            if (pawn != null)
            {
                if (MapManager.Instance.HasCropAt(onMouseCellPos))
                {
                    Debug.Log($"has");
                    ItemInstanceManager.CropInstance cropInstance = MapManager.Instance.GetMapData(onMouseCellPos).item as ItemInstanceManager.CropInstance;
                    if (cropInstance.IsMature())
                    {
                        Debug.Log($"mature");
                        if (pawn.handlingTask != null)
                        {
                            if (pawn.handlingTask.type != TaskManager.TaskTypes.Move)
                            {
                                //TODO 处理其它目前正在处理的任务（返还给taskManger）
                            }
                            else
                            {
                                MapManager.Instance.SetWillPawnState(pawn.handlingTask.target_position, false);
                            }
                        }
                        Debug.Log($"add");
                        TaskManager.Instance.AddTask(onMouseCellPos, TaskManager.TaskTypes.Harvest);
                        //pawn.handlingTask = new TaskManager.Task(onMouseCellPos, TaskManager.TaskTypes.Harvest, task_id: 0, id: cropInstance.GetModelId());
                        //StartCoroutine(PawnManager.Instance.HandleHarvestTask(pawn));
                        //PawnManager.Instance.HandleTask(pawn);
                    }
                }
            }
        }
        else
        {
            Debug.Log("目标位置不可收获或没有作物！");
        }
    }
    #endregion
}