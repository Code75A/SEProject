using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // 用于文件操作
using Newtonsoft.Json; // 用于JSON序列化和反序列化
//需要添加包管理器，安装Newtonsoft.Json包
//点击菜单栏 Window > Package Manager
//在Package Manager窗口左上角，点击"+"按钮
//选择"Add package by name"
//名称输入为com.unity.nuget.newtonsoft-json
public class SLManager : MonoBehaviour
{
    public static SLManager Instance;
    [System.Serializable]
    public class BuildManagerData
    {
        public Dictionary<BuildManager.BuildingType, List<BuildingData>> buildingLists;

        [System.Serializable]
        public class BuildingData
        {
            public int id;
            public string name;
            public string texturePath; // 存储纹理路径
            public BuildManager.BuildingType type;
            public int width, height;
            public int durability;
            public bool can_build;
            public bool can_walk;
            public bool can_plant;
            public List<KeyValuePair<int, int>> material_list;
        }
    }
    public class PawnManagerData{

    }

    private string savePath => Path.Combine(Application.persistentDataPath, "BuildManagerData.json");

    // 保存 BuildManager 的静态数据
    public void SaveBuildManager(){
        BuildManagerData data = new BuildManagerData
        {
            buildingLists = new Dictionary<BuildManager.BuildingType, List<BuildManagerData.BuildingData>>()
        };

        foreach (var kvp in BuildManager.Instance.buildingLists)
        {
            var buildingDataList = new List<BuildManagerData.BuildingData>();
            foreach (var building in kvp.Value)
            {
                buildingDataList.Add(new BuildManagerData.BuildingData
                {
                    id = building.id,
                    name = building.name,
                    texturePath = building.texture != null ? building.texture.name : null, // 假设纹理名可用作路径
                    type = building.type,
                    width = building.width,
                    height = building.height,
                    durability = building.durability,
                    can_build = building.can_build,
                    can_walk = building.can_walk,
                    can_plant = building.can_plant,
                    material_list = building.material_list
                });
            }
            data.buildingLists[kvp.Key] = buildingDataList;
        }

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(savePath, json);
        Debug.Log($"BuildManager data saved to {savePath}");
    }

    // 加载 BuildManager 的静态数据
    public void LoadBuildManager()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Save file not found!");
            return;
        }

        string json = File.ReadAllText(savePath);
        BuildManagerData data = JsonConvert.DeserializeObject<BuildManagerData>(json);

        foreach (var kvp in data.buildingLists)
        {
            if (!BuildManager.Instance.buildingLists.ContainsKey(kvp.Key))
                BuildManager.Instance.buildingLists[kvp.Key] = new List<BuildManager.Building>();

            foreach (var buildingData in kvp.Value)
            {
                BuildManager.Instance.buildingLists[kvp.Key].Add(new BuildManager.Building
                {
                    id = buildingData.id,
                    name = buildingData.name,
                    texture = Resources.Load<Sprite>(buildingData.texturePath), // 假设纹理存储在 Resources 文件夹
                    type = buildingData.type,
                    width = buildingData.width,
                    height = buildingData.height,
                    durability = buildingData.durability,
                    can_build = buildingData.can_build,
                    can_walk = buildingData.can_walk,
                    can_plant = buildingData.can_plant,
                    material_list = buildingData.material_list
                });
            }
        }

        Debug.Log("BuildManager data loaded successfully.");
    }

    // Awake 在脚本实例化时调用
    void Awake(){
        //Debug.Log("SLManagerLocal Awake called.");
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


    void Start()
    {
        /* //测试用例
        // 创建测试数据
        Debug.Log("SLManagerLocal Start called.");
        BuildManager.Instance.buildingLists = new Dictionary<BuildManager.BuildingType, List<BuildManager.Building>>();
        

        BuildManager.Instance.buildingLists[BuildManager.BuildingType.Dev] = new List<BuildManager.Building>
        {
            new BuildManager.Building
            {
                id = 1,
                name = "Test Dev Building",
                texture = null, // 假设没有纹理
                type = BuildManager.BuildingType.Dev,
                width = 5,
                height = 5,
                durability = 100,
                can_build = true,
                can_walk = false,
                can_plant = false,
                material_list = new List<KeyValuePair<int, int>> { new KeyValuePair<int, int>(1, 10) }
            }
        };

        // 保存数据
        SaveBuildManager();

        // 清空现有数据
        BuildManager.Instance.buildingLists.Clear();

        // 加载数据
        LoadBuildManager();
        Debug.Log("Data loaded from file.");
        // 验证加载的数据
        foreach (var kvp in BuildManager.Instance.buildingLists)
        {
            Debug.Log($"Building Type: {kvp.Key}");
            foreach (var building in kvp.Value)
            {
                Debug.Log($"Building Name: {building.name}, ID: {building.id}, Durability: {building.durability}");
            }
        }
        */
    }

}
