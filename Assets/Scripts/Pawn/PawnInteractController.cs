using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PawnInteractController : MonoBehaviour
{
    public PawnManager.Pawn pawn;
    private UnityEngine.Vector3 targetPosition;
    
    private bool isMoving = false;
    
    public Vector3Int fromCellPos;
    private Vector3Int targetCellPos;

    public Tilemap landTilemap;
    public GameObject content;

    void Update(){
        //自检极端情况
        if(pawn == null){
            Debug.LogError("Error: 存在无主PawnInteractController");
            return;
        }

        // 只控制当前选中的Pawn移动
        if (PawnManager.Instance.SelectingPawn == pawn){
            // 调试日志（仅pawn.id == 1时显示）
            // if (pawn != null && pawn.id == 1)
            // {
            //     Debug.Log($"Pawn 1选中状态: {PawnManager.Instance.SelectingPawn == pawn}");
            // }

            // 检测右键点击
            //if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject())
            //后续仍需测试：todo: 右键点击时，如果点击了UI元素（例如按钮），则不处理移动逻辑。但如果加入ui判定，点击地图移动无效

            if (Input.GetMouseButtonDown(1)){
                UnityEngine.Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int onMouseCellPos = landTilemap.WorldToCell(mouseWorldPos);
                //targetPosition = landTilemap.GetCellCenterWorld(targetCellPos);
                //targetPosition.z = 0; // 确保Z轴为0，避免移动时穿过地面
                // 调试信息
                if (pawn != null){
                    Debug.Log($"右键点击坐标: {mouseWorldPos}");
                    Debug.Log($"目标格子: {targetCellPos}");
                    Debug.Log($"可通行: {MapManager.Instance.IsWalkable(targetCellPos)}");
                    Debug.Log($"已有Pawn: {MapManager.Instance.HasPawnAt(targetCellPos)}");
                }

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


    }

    void FixedUpdate(){
        // 移动逻辑
        if (isMoving)
        {
            //Debug.Log($"移动参数 | 速度: {pawn.moveSpeed} | 帧时间: {Time.deltaTime}");
            //Debug.Log($"当前位置: {transform.position} | 目标位置: {targetPosition}");

            float step = pawn.moveSpeed * Time.deltaTime * content.transform.localScale.x;
            targetPosition = landTilemap.GetCellCenterWorld(targetCellPos);

            //float step = 1.0f;
            transform.position = UnityEngine.Vector3.MoveTowards(transform.position, targetPosition, step);

            // 更新地图格子状态
            //Vector3Int currentCellPos = MapManager.Instance.GetCellPosFromWorld(transform.position);

            // if (currentCellPos != targetCellPos)
            // {
            //     MapManager.Instance.SetPawnState(currentCellPos, false);
            // }

            // 到达目标s
            if (UnityEngine.Vector3.Distance(transform.position, targetPosition) < 0.0005f)
            {
                isMoving = false;
                PawnManager.Instance.ResolveTask(pawn);

                Debug.Log($"到达目标位置: {targetPosition}");
                
            }
        }
        
    }
    public void SetSelectingPawn()
    {
        if (PawnManager.Instance != null)
        {
            PawnManager.Instance.SelectingPawn = pawn;
            Debug.Log($"选中Pawn ID: {pawn?.id}");
        }
    }

    private void OnMouseDown()
    {
        SetSelectingPawn();
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

    public void MovePawnToPosition(Vector3Int targetCellPos, PawnManager.Pawn targetPawn){
        if (targetPawn == null){
            Debug.LogWarning("目标Pawn为空，无法移动！");
            return;
        }

        
        // 转换世界坐标到格子坐标
        //Vector3Int targetCellPos = MapManager.Instance.GetCellPosFromWorld(targetWorldPos);

        // 检查目标位置是否可通行
        if (!MapManager.Instance.IsWalkable(targetCellPos) || 
            MapManager.Instance.HasPawnAt(targetCellPos)){

            Debug.LogWarning($"目标位置不可通行: {targetPosition}");
            Debug.LogWarning(MapManager.Instance.GetMapData(targetCellPos));

            return;
        }

        // 设置移动参数
        //targetWorldPos.z = 0; // 确保Z坐标为0
        this.targetCellPos  = targetCellPos;
        targetPosition = landTilemap.GetCellCenterWorld(targetCellPos);

    }



}