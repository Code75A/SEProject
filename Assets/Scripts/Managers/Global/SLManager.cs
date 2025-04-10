using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SLManager : MonoBehaviour{
    public static SLManager Instance;

    [System.Serializable]
    class gameData{
        public PawnManager pawnManager; // 存储 PawnManager 的数据
    }

    void Awake(){
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(gameObject);
        }
    }

    public void Save(){
        gameData data = new gameData();
        data.pawnManager = PawnManager.Instance; // 将 PawnManager 的数据存入 gameData

        string json = JsonUtility.ToJson(data, true); // 序列化为 JSON
        PlayerPrefs.SetString("SaveData", json); // 存储到 PlayerPrefs
        Debug.Log("保存成功: " + json);
    }

    public void Load(){
        string json = PlayerPrefs.GetString("SaveData");
        if (string.IsNullOrEmpty(json)){
            Debug.LogWarning("没有找到保存的数据！");
            return;
        }

        gameData data = JsonUtility.FromJson<gameData>(json); // 反序列化 JSON
        if (data == null || data.pawnManager == null){
            Debug.LogWarning("加载失败，数据为空！");
            return;
        }

        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data.pawnManager), PawnManager.Instance); // 将数据覆盖到 PawnManager

        // 重新实例化 Pawn 的 GameObject
        foreach (var pawn in PawnManager.Instance.pawns){
            pawn.Instance = GameObject.Instantiate(PawnManager.Instance.pawnPrefab, pawn.position, Quaternion.identity);
            pawn.Instance.name = $"Pawn_{pawn.id}";
            PawnInteractController controller = pawn.Instance.AddComponent<PawnInteractController>();
            controller.pawn = pawn;
        }

        Debug.Log("加载成功: " + json);
    }
}
