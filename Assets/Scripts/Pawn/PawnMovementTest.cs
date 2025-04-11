using UnityEngine;

public class PawnMovementTester : MonoBehaviour
{
    [Header("必填项")]
    public PawnInteractController pawnController;
    
    [Header("测试路径点")]
    public Vector3[] testPositions = {
        new Vector3(50, 30, 0),
        new Vector3(80, 1, 0),
        new Vector3(20, 40, 0)
    };

    [Header("调试设置")]
    public float moveDelay = 1.0f;    // 每个点之间的移动间隔
    public bool loopMovement = true; // 是否循环移动
    public KeyCode triggerKey = KeyCode.T; // 触发键

    private int currentIndex = 0;
    private bool isTesting = false;

    void Update()
    {
        if (Input.GetKeyDown(triggerKey))
        {
            StartTesting();
        }
    }

    public void StartTesting()
    {
        if (pawnController == null)
        {
            Debug.LogError("未指定PawnController！");
            return;
        }

        isTesting = true;
        currentIndex = 0;
        MoveToNextPosition();
    }

    void MoveToNextPosition()
    {
        if (!isTesting || currentIndex >= testPositions.Length) return;

        Vector3 target = testPositions[currentIndex];
        Debug.Log($"开始移动测试 #{currentIndex+1} → ({target.x}, {target.y})");
        
        pawnController.MovePawnToPosition(target);
        currentIndex++;

        // 自动继续或循环
        if (currentIndex < testPositions.Length)
        {
            Invoke("MoveToNextPosition", moveDelay);
        }
        else if (loopMovement)
        {
            Invoke("StartTesting", moveDelay);
        }
    }

    // // 可视化路径（仅在编辑器显示）
    // void OnDrawGizmosSelected()
    // {
    //     if (testPositions == null || testPositions.Length < 2) return;

    //     Gizmos.color = Color.cyan;
    //     for (int i = 0; i < testPositions.Length - 1; i++)
    //     {
    //         Gizmos.DrawLine(testPositions[i], testPositions[i+1]);
    //         Gizmos.DrawSphere(testPositions[i], 0.2f);
    //     }
    //     Gizmos.DrawSphere(testPositions[testPositions.Length-1], 0.2f);
    // }
}