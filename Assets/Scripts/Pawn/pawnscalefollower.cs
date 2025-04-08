using UnityEngine;
using UnityEngine.Tilemaps;

public class PawnScaleFollower : MonoBehaviour{
    private Vector3 baseScale; // Pawn 原始缩放
    private Transform tilemapTransform;
    private Vector3 initialPositionInMap; // Pawn 在地图上的初始相对位置

    void Start(){
        tilemapTransform = MapManager.Instance.landTilemap.transform;
        baseScale = transform.localScale; // 初始 scale
        // 记录 Pawn 在地图上的相对位置
        initialPositionInMap = tilemapTransform.InverseTransformPoint(transform.position);
    }

    void Update(){
        Vector3 mapScale = tilemapTransform.lossyScale;
        Vector3 cellSize = MapManager.Instance.landTilemap.cellSize;

        // 根据地图缩放重新调整 Pawn 的缩放
        transform.localScale = new Vector3(
            baseScale.x * mapScale.x * cellSize.x,
            baseScale.y * mapScale.y * cellSize.y,
            baseScale.z
        );

        // 计算 Pawn 在世界坐标系中的位置，保持相对位置不变
        Vector3 worldPosition = tilemapTransform.TransformPoint(initialPositionInMap);
        transform.position = worldPosition;
    }
}

