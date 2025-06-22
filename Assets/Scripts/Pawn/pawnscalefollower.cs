
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PawnScaleFollower : MonoBehaviour 
{
    private Vector3 baseScale;
    private Transform tilemapTransform;
    private SpriteRenderer spriteRenderer;
    private float lastVisibleTime;

    void Start() 
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        tilemapTransform = MapManager.Instance.landTilemap.transform;
        baseScale = transform.localScale;
        
        // 初始化渲染设置
        spriteRenderer.forceRenderingOff = false;
        lastVisibleTime = Time.time;
    }

    void Update() 
    {
        // 安全缩放计算
        if (tilemapTransform != null) 
        {
            Vector3 newScale = new Vector3(
                baseScale.x * tilemapTransform.lossyScale.x,
                baseScale.y * tilemapTransform.lossyScale.y,
                baseScale.z
            );
            
            // 缩放限制保护
            newScale.x = Mathf.Clamp(newScale.x, 0.1f, 10f);
            newScale.y = Mathf.Clamp(newScale.y, 0.1f, 10f);
            
            transform.localScale = newScale;
        }

        // // 消失保护机制
        // if (spriteRenderer.isVisible) 
        // {
        //     lastVisibleTime = Time.time;
        // }
        // else if (Time.time - lastVisibleTime > 0.5f) 
        // {
        //     Debug.LogWarning("Pawn异常消失，自动重置位置");
        //     ResetPosition();
        // }
    }

    // void ResetPosition()
    // {
    //     // 确保Z轴在摄像机范围内
    //     Vector3 newPos = transform.position;
    //     newPos.z = 0;
    //     transform.position = newPos;
        
    //     // 强制渲染一帧
    //     spriteRenderer.forceRenderingOff = false;
    //     lastVisibleTime = Time.time;
    // }

    // void OnBecameInvisible() 
    // {
    //     Debug.Log("Pawn离开视口，尝试自动修复");
    //     ResetPosition();
    // }
}