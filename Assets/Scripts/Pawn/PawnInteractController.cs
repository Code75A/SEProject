using UnityEngine;
using UnityEngine.EventSystems;

public class PawnInteractController : MonoBehaviour{
    public PawnManager.Pawn pawn;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private Vector3Int targetCellPos; // 目标的地图格子坐标

    void Update(){
        // 只控制当前选中的 Pawn 移动
        if (PawnManager.Instance.SelectingPawn == pawn){
            // 检测右键点击，设置目标点
            if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject()){
                // 获取鼠标点击的世界坐标
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
                Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

                // 排除带有 Collider2D 的 Sprite 的情况
                RaycastHit2D hitSprite = Physics2D.Raycast(mousePos2D, Vector2.zero, 0f);
                if (hitSprite.collider != null){
                    // 如果点击的是带有Collider2D的物体（例如 UI），则不处理
                    return; 
                }

                // 获取目标格子的世界坐标并转换为格子坐标
                targetPosition = mouseWorldPos;
                targetCellPos = MapManager.Instance.GetCellPosFromWorld(targetPosition);

                // 判断目标格子是否可以通行并且没有 Pawn
                if (MapManager.Instance.IsWalkable(targetCellPos) && !MapManager.Instance.HasPawnAt(targetCellPos)){
                    isMoving = true;
                    Debug.Log($"Pawn_{pawn.id} 正在移动到 {targetPosition}");
                }
                else{
                    // 如果目标位置不可通行或已经有 Pawn，取消移动
                    Debug.Log("目标位置不可通行或已有其他角色！");
                }
            }
        }

        // 进行移动
        if (isMoving){
            float step = pawn.moveSpeed * Time.deltaTime;

            // 计算从当前位置到目标位置的距离
            float distance = Vector3.Distance(transform.position, targetPosition);

            // 如果角色尚未到达目标，继续移动
            if (distance > 0.05f){
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

                // 更新地图上的格子状态：起始格子清除 Pawn，目标格子设置为有 Pawn
                Vector3Int currentCellPos = MapManager.Instance.GetCellPosFromWorld(transform.position);
                if(currentCellPos != targetCellPos){
                    MapManager.Instance.SetPawnState(currentCellPos, false); // 移除当前格子的 Pawn
                }
            }
            else{
                // 到达目标后停止移动
                isMoving = false;

                // 最终确认目标格子的状态
                MapManager.Instance.SetPawnState(targetCellPos, true);
            }
        }
    }

    // 选中此角色
    public void SetSelectingPawn(){
        if (pawn != null && PawnManager.Instance != null){
            PawnManager.Instance.SelectingPawn = pawn;
            Debug.Log($"选中 Pawn ID: {pawn.id}");
        }
        else {
            Debug.LogWarning("PawnManager.Instance 未找到或 Pawn 为空！");
        }
    }

    private void OnMouseDown(){
        Debug.Log($"点击了 Pawn：{name}");
        SetSelectingPawn();
    }

    void Start(){
        // 获取 Tilemap 每个格子的尺寸
        Vector3 cellSize = MapManager.Instance.landTilemap.cellSize;

        // 获取 Tilemap 的缩放（这可能会影响实际格子在世界中的大小）
        Vector3 mapScale = MapManager.Instance.landTilemap.transform.lossyScale;

        // 计算缩放后的实际世界单位大小
        float worldCellWidth = cellSize.x * mapScale.x;
        float worldCellHeight = cellSize.y * mapScale.y;

        // 设置 Pawn 的本地缩放（注意：我们不希望改动 Z 轴）
        transform.localScale = new Vector3(worldCellWidth, worldCellHeight, transform.localScale.z);

        Debug.Log($"Pawn 缩放到适配 tile: {transform.localScale}");
    }

}
