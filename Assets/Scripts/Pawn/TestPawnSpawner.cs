using UnityEngine;
public class TestPawnSpawner : MonoBehaviour
{
    void Update(){
        // 检查 P 键是否按下
        if (Input.GetKeyDown(KeyCode.P)){
            Debug.Log("P 键被按下");

            // 生成一个随机位置的 Pawn
            Vector3Int randomPos = new Vector3Int(32+Random.Range(-5, 5), 32+Random.Range(-5, 5),0);

            // 调用 PawnManager 的 CreatePawn 方法
            if (PawnManager.Instance != null){
                PawnManager.Instance.CreatePawn(randomPos);
                Debug.Log($"在位置 {randomPos} 创建 Pawn");
            }
            else{
                Debug.LogError(" PawnManager.Instance 为空，请检查 PawnManager 绑定！");
            }
        }
    }
}
