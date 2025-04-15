using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PawnInteractController : MonoBehaviour
{
    public PawnManager.Pawn pawn;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private Vector3Int targetCellPos;

    public Tilemap landTilemap;
    public GameObject content;

    void Update()
    {
        // 只控制当前选中的Pawn移动
        if (PawnManager.Instance.SelectingPawn == pawn)
        {
            // 调试日志（仅pawn.id == 1时显示）
            // if (pawn != null && pawn.id == 1)
            // {
            //     Debug.Log($"Pawn 1选中状态: {PawnManager.Instance.SelectingPawn == pawn}");
            // }

            // 检测右键点击
            //if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject())

            //后续仍需测试：todo: 右键点击时，如果点击了UI元素（例如按钮），则不处理移动逻辑。但如果加入ui判定，点击地图移动无效

            if (Input.GetMouseButtonDown(1))
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                targetCellPos = landTilemap.WorldToCell(mouseWorldPos);

                targetPosition = landTilemap.GetCellCenterWorld(targetCellPos);
                //targetPosition.z = 0; // 确保Z轴为0，避免移动时穿过地面
                // 调试信息
                if (pawn != null)
                {
                    Debug.Log($"右键点击坐标: {mouseWorldPos}");
                    Debug.Log($"目标格子: {targetCellPos}");
                    Debug.Log($"可通行: {MapManager.Instance.IsWalkable(targetCellPos)}");
                    Debug.Log($"已有Pawn: {MapManager.Instance.HasPawnAt(targetCellPos)}");
                }

                // 判断目标格子是否可以通行并且没有Pawn
                if (MapManager.Instance.IsWalkable(targetCellPos) && !MapManager.Instance.HasPawnAt(targetCellPos))
                {
                    isMoving = true;
                    if (pawn != null && pawn.id == 1)
                    {
                        Debug.Log($"开始移动到: {targetPosition}");
                    }
                }
                else
                {
                    if (pawn != null && pawn.id == 1)
                    {
                        Debug.Log("目标位置不可通行或已有其他角色！");
                    }
                }
            }

        }

        // 移动逻辑
        if (isMoving)
        {
            //Debug.Log($"移动参数 | 速度: {pawn.moveSpeed} | 帧时间: {Time.deltaTime}");
            Debug.Log($"当前位置: {transform.position} | 目标位置: {targetPosition}");

            float step = pawn.moveSpeed * Time.deltaTime * content.transform.localScale.x;
            targetPosition = landTilemap.GetCellCenterWorld(targetCellPos);

            //float step = 1.0f;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);


            // Vector3 newPos = Vector3.MoveTowards(transform.position, 
            //         new Vector3(targetPosition.x, targetPosition.y, transform.position.z),
            //       step);
            // Debug.Log($"新位置: {newPos} | 实际移动距离: {Vector3.Distance(transform.position, newPos)}");
            // transform.position = newPos;

            // 更新地图格子状态
            Vector3Int currentCellPos = MapManager.Instance.GetCellPosFromWorld(transform.position);
            if (currentCellPos != targetCellPos)
            {
                MapManager.Instance.SetPawnState(currentCellPos, false);
            }

            // 到达目标
            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                isMoving = false;
                MapManager.Instance.SetPawnState(targetCellPos, true);             
                if (pawn != null){
                    Debug.Log($"到达目标位置: {targetPosition}");
                }
            }
        }
        
        // 检测Q键取消选中Pawn
        if (Input.GetKeyDown(KeyCode.Q)){
            PawnManager.Instance.SelectingPawn = null;
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
    public void MovePawnToPosition(Vector3 targetWorldPos)
    {
        
        // 转换世界坐标到格子坐标
        //Vector3Int targetCellPos = MapManager.Instance.GetCellPosFromWorld(targetWorldPos);
        
        // 检查目标位置是否可通行
        if (!MapManager.Instance.IsWalkable(targetCellPos) || 
            MapManager.Instance.HasPawnAt(targetCellPos))
        {
            Debug.LogWarning($"目标位置不可通行: {targetWorldPos}");
            return;
        }

        // 设置移动参数
        targetPosition = targetWorldPos;
        targetPosition.z = 0; // 确保Z坐标为0
        //targetCellPos = MapManager.Instance.GetCellPosFromWorld(targetPosition);
        isMoving = true;
        
        Debug.Log($"开始移动Pawn到位置: {targetPosition}");
    }

    void Start()
    {
        // // 初始化Pawn大小适配Tilemap格子
        // Vector3 cellSize = MapManager.Instance.landTilemap.cellSize;
        // Vector3 mapScale = MapManager.Instance.landTilemap.transform.lossyScale;
        // transform.localScale = new Vector3(cellSize.x * mapScale.x, cellSize.y * mapScale.y, transform.localScale.z);
    }
}