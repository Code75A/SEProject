
using UnityEngine;

public class PawnInteractController : MonoBehaviour
{
    // 2.3.1 外部引用 Pawn 类对象
    public PawnManager.Pawn pawn; // 直接引用 Pawn 类对象

    // 2.3.3 设置 PawnManager 的 selectingPawn 为当前 pawn 对象
    public void SetSelectingPawn(){
        if (pawn != null && PawnManager.Instance != null){
            // 将当前 Pawn 设置为 selectingPawn
            PawnManager.Instance.SelectingPawn = pawn;
            Debug.Log($"选中 Pawn ID: {pawn.id}");
        }
        else{
            Debug.LogWarning("PawnManager.Instance 未找到或 Pawn 为空！");
        }
    }

    // 检测鼠标点击事件，设置选中 Pawn
    private void OnMouseDown(){
        SetSelectingPawn();
    }
}
