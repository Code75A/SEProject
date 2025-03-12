using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SLManager : MonoBehaviour
{
    public static SLManager Instance;
    class gameData
    {
        //TODO: 填入需要存储的数据
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Save(){
        gameData data = new gameData();
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveData", json);
    }
    public void Load(){
        string json = PlayerPrefs.GetString("SaveData");
        gameData data = JsonUtility.FromJson<gameData>(json);
        if (data == null)
        {
            return;
        }
    }
}
