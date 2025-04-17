using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // 用于文件操作
using Newtonsoft.Json; // 用于JSON序列化和反序列化
//注意事项一：需要添加包管理器，安装Newtonsoft.Json包
//点击菜单栏 Window > Package Manager
//在Package Manager窗口左上角，点击"+"按钮
//选择"Add package by name"
//名称输入为com.unity.nuget.newtonsoft-json
//注意事项二：SLManager的start函数需要再相应的加载类执行完成后再执行，OnDestroy函数需要再相应的存储类摧毁前执行，不然会出现引用空指针的情况
//注意事项三：在使用测试函数的时候因为对应类的clear函数没有清除Dictionary的值，所以会出现ArgumentException: An item with the same key has already been added的错误，这个是正常现象，因为正常存储的时候不会发生这个错误
public class SLManager : MonoBehaviour
{
    public static SLManager Instance;
    #region "BuildManagerData"
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

    private string savePath => Path.Combine(Application.persistentDataPath, "BuildManagerData.json");

    // 保存 BuildManager 的静态数据
    public void SaveBuildManager()
    {
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

    // 测试保存和加载 BuildManager 数据的方法
    public void TestSaveAndLoadBuildManagerData()
    {
        Debug.Log("TestSaveAndLoadBuildManagerData called.");
        
        // 创建测试数据
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
    }
    #endregion

    #region "CropManagerData"
    [System.Serializable]
    public class CropManagerData
    {
        public List<CropData> cropList;

        [System.Serializable]
        public class CropData
        {
            public int id;
            public string name;
            public float lifetime;
        }
    }

    private string cropSavePath => Path.Combine(Application.persistentDataPath, "CropManagerData.json");

    public void SaveCropManager()
    {
        if (CropManager.Instance == null)
        {
            Debug.LogError("CropManager.Instance is null. Cannot save data.");
            return;
        }

        CropManagerData data = new CropManagerData
        {
            cropList = new List<CropManagerData.CropData>()
        };

        foreach (var crop in CropManager.Instance.cropList)
        {
            data.cropList.Add(new CropManagerData.CropData
            {
                id = crop.id,
                name = crop.name,
                lifetime = crop.lifetime
            });
        }

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(cropSavePath, json);
        Debug.Log($"CropManager data saved to {cropSavePath}");
    }

    // 加载 CropManager 的静态数据
    public void LoadCropManager()
    {
        if (!File.Exists(cropSavePath))
        {
            Debug.LogWarning("CropManager save file not found!");
            return;
        }

        if (CropManager.Instance == null)
        {
            Debug.LogError("CropManager.Instance is null. Cannot load data.");
            return;
        }

        string json = File.ReadAllText(cropSavePath);
        CropManagerData data = JsonConvert.DeserializeObject<CropManagerData>(json);

        CropManager.Instance.cropList.Clear();
        foreach (var cropData in data.cropList)
        {
            CropManager.Instance.cropList.Add(new CropManager.Crop
            {
                id = cropData.id,
                name = cropData.name,
                lifetime = cropData.lifetime
            });
        }

        Debug.Log("CropManager data loaded successfully.");
    }

    // 测试保存和加载 CropManager 数据的方法
    public void TestSaveAndLoadCropManagerData()
    {
        Debug.Log("TestSaveAndLoadCropManagerData called.");

        if (CropManager.Instance == null)
        {
            Debug.LogError("CropManager.Instance is null. Cannot test save and load.");
            return;
        }

        // 创建测试数据
        CropManager.Instance.cropList = new List<CropManager.Crop>
        {
            new CropManager.Crop { id = 0, name = "测试作物1", lifetime = 10.0f },
            new CropManager.Crop { id = 1, name = "测试作物2", lifetime = 5.0f }
        };

        // 保存数据
        SaveCropManager();

        // 清空现有数据
        CropManager.Instance.cropList.Clear();

        // 加载数据
        LoadCropManager();

        // 验证加载的数据
        foreach (var crop in CropManager.Instance.cropList)
        {
            Debug.Log($"Crop Name: {crop.name}, ID: {crop.id}, Lifetime: {crop.lifetime}");
        }
    }
    #endregion

    #region "ItemManagerData"
    [System.Serializable]
    public class ItemManagerData
    {
        public List<ToolData> tools;
        public List<MaterialData> materials;

        [System.Serializable]
        public class ToolData
        {
            public int id;
            public string name;
            public string texturePath;
            public int max_durability;
            public Dictionary<string, int> enhancements; // 使用字符串表示 EnhanceType
        }

        [System.Serializable]
        public class MaterialData
        {
            public int id;
            public string name;
            public string texturePath;
        }
    }

    private string itemSavePath => Path.Combine(Application.persistentDataPath, "ItemManagerData.json");

    public void SaveItemManager()
    {
        if (ItemManager.Instance == null)
        {
            Debug.LogError("ItemManager.Instance is null. Cannot save data.");
            return;
        }

        ItemManagerData data = new ItemManagerData
        {
            tools = new List<ItemManagerData.ToolData>(),
            materials = new List<ItemManagerData.MaterialData>()
        };

        // 保存 Tool 数据
        if (ItemManager.Instance.itemLists.TryGetValue(ItemManager.ItemType.Tool, out var tools))
        {
            foreach (var item in tools)
            {
                if (item is ItemManager.Tool tool)
                {
                    data.tools.Add(new ItemManagerData.ToolData
                    {
                        id = tool.id,
                        name = tool.name,
                        texturePath = tool.texture != null ? tool.texture.name : null,
                        max_durability = tool.max_durability,
                        enhancements = new Dictionary<string, int>()
                    });
                    foreach (var enhancement in tool.enhancements)
                    {
                        data.tools[data.tools.Count - 1].enhancements.Add(enhancement.Key.ToString(), enhancement.Value);
                    }
                }
            }
        }

        // 保存 Material 数据
        if (ItemManager.Instance.itemLists.TryGetValue(ItemManager.ItemType.Material, out var materials))
        {
            foreach (var item in materials)
            {
                if (item is ItemManager.Material material)
                {
                    data.materials.Add(new ItemManagerData.MaterialData
                    {
                        id = material.id,
                        name = material.name,
                        texturePath = material.texture != null ? material.texture.name : null
                    });
                }
            }
        }

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(itemSavePath, json);
        Debug.Log($"ItemManager data saved to {itemSavePath}");
    }

    public void LoadItemManager()
    {
        if (!File.Exists(itemSavePath))
        {
            Debug.LogWarning("ItemManager save file not found!");
            return;
        }

        if (ItemManager.Instance == null)
        {
            Debug.LogError("ItemManager.Instance is null. Cannot load data.");
            return;
        }

        string json = File.ReadAllText(itemSavePath);
        ItemManagerData data = JsonConvert.DeserializeObject<ItemManagerData>(json);

        // 清空现有数据
        ItemManager.Instance.itemLists[ItemManager.ItemType.Tool]?.Clear();
        ItemManager.Instance.itemLists[ItemManager.ItemType.Material]?.Clear();

        // 加载 Tool 数据
        foreach (var toolData in data.tools)
        {
            var tool = new ItemManager.Tool
            {
                id = toolData.id,
                name = toolData.name,
                texture = Resources.Load<Sprite>(toolData.texturePath),
                max_durability = toolData.max_durability,
                enhancements = new Dictionary<PawnManager.Pawn.EnhanceType, int>()
            };
            foreach (var enhancement in toolData.enhancements)
            {
                if (System.Enum.TryParse(enhancement.Key, out PawnManager.Pawn.EnhanceType enhanceType))
                {
                    tool.enhancements[enhanceType] = enhancement.Value;
                }
            }
            ItemManager.Instance.itemLists[ItemManager.ItemType.Tool].Add(tool);
        }

        // 加载 Material 数据
        foreach (var materialData in data.materials)
        {
            var material = new ItemManager.Material
            {
                id = materialData.id,
                name = materialData.name,
                texture = Resources.Load<Sprite>(materialData.texturePath)
            };
            ItemManager.Instance.itemLists[ItemManager.ItemType.Material].Add(material);
        }

        Debug.Log("ItemManager data loaded successfully.");
    }

    public void TestSaveAndLoadItemManagerData()
    {
        // 测试保存和加载 ItemManager 数据的方法
        Debug.Log("TestSaveAndLoadItemManagerData called.");

        if (ItemManager.Instance == null)
        {
            Debug.LogError("ItemManager.Instance is null. Cannot test save and load.");
            return;
        }

        // 创建测试数据
        ItemManager.Instance.itemLists[ItemManager.ItemType.Tool] = new List<ItemManager.Item>
        {
            new ItemManager.Tool
            {
                id = 1,
                name = "测试工具",
                texture = null,
                max_durability = 50,
                enhancements = new Dictionary<PawnManager.Pawn.EnhanceType, int>
                {
                    { PawnManager.Pawn.EnhanceType.Speed, 10 },
                    { PawnManager.Pawn.EnhanceType.Power, 20 }
                }
            }
        };

        ItemManager.Instance.itemLists[ItemManager.ItemType.Material] = new List<ItemManager.Item>
        {
            new ItemManager.Material
            {
                id = 2,
                name = "测试材料",
                texture = null
            }
        };

        // 保存数据
        SaveItemManager();

        // 清空现有数据
        ItemManager.Instance.itemLists[ItemManager.ItemType.Tool].Clear();
        ItemManager.Instance.itemLists[ItemManager.ItemType.Material].Clear();

        // 加载数据
        LoadItemManager();

        // 验证加载的数据
        foreach (var tool in ItemManager.Instance.itemLists[ItemManager.ItemType.Tool])
        {
            Debug.Log($"Tool Name: {tool.name}, ID: {tool.id}");
        }

        foreach (var material in ItemManager.Instance.itemLists[ItemManager.ItemType.Material])
        {
            Debug.Log($"Material Name: {material.name}, ID: {material.id}");
        }
    }
    #endregion

    #region "MapManagerData"
    [System.Serializable]
    public class MapManagerData
    {
        public List<MapData> mapDataList;

        [System.Serializable]
        public class MapData
        {
            public int x, y; // 格子坐标
            public int type; // 格子类型
            public bool can_walk;
            public bool can_build;
            public bool can_plant;
            public bool has_print; // 是否显示蓝图
            public bool has_building; // 是否有建筑
            public float fertility;
            public float humidity;
            public float light;
            public float walk_speed;
        }
    }

    private string mapSavePath => Path.Combine(Application.persistentDataPath, "MapManagerData.json");

    public void SaveMapManager()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogError("MapManager.Instance is null. Cannot save data.");
            return;
        }

        MapManagerData data = new MapManagerData
        {
            mapDataList = new List<MapManagerData.MapData>()
        };

        for (int x = 0; x < MapManager.MAP_SIZE; x++)
        {
            for (int y = 0; y < MapManager.MAP_SIZE; y++)
            {
                var mapData = MapManager.Instance.mapDatas[x, y];
                data.mapDataList.Add(new MapManagerData.MapData
                {
                    x = x,
                    y = y,
                    type = (int)mapData.type,
                    can_walk = mapData.can_walk,
                    can_build = mapData.can_build,
                    can_plant = mapData.can_plant,
                    has_print = mapData.has_print,
                    has_building = mapData.has_building,
                    fertility = mapData.fertility,
                    humidity = mapData.humidity,
                    light = mapData.light,
                    walk_speed = mapData.walk_speed
                });
            }
        }

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(mapSavePath, json);
        Debug.Log($"MapManager data saved to {mapSavePath}");
    }

    public void LoadMapManager()
    {
        if (!File.Exists(mapSavePath))
        {
            Debug.LogWarning("MapManager save file not found!");
            return;
        }

        if (MapManager.Instance == null)
        {
            Debug.LogError("MapManager.Instance is null. Cannot load data.");
            return;
        }

        string json = File.ReadAllText(mapSavePath);
        MapManagerData data = JsonConvert.DeserializeObject<MapManagerData>(json);

        foreach (var mapData in data.mapDataList)
        {
            var mapCell = MapManager.Instance.mapDatas[mapData.x, mapData.y];
            mapCell.type = (MapManager.tileTypes)mapData.type;
            mapCell.texture = MapManager.Instance.tiles[mapData.type];
            mapCell.can_walk = mapData.can_walk;
            mapCell.can_build = mapData.can_build;
            mapCell.can_plant = mapData.can_plant;
            mapCell.has_print = mapData.has_print;
            mapCell.has_building = mapData.has_building;
            mapCell.fertility = mapData.fertility;
            mapCell.humidity = mapData.humidity;
            mapCell.light = mapData.light;
            mapCell.walk_speed = mapData.walk_speed;
        }

        MapManager.Instance.GenerateMapTiles(); // 重新生成地图瓦片
        Debug.Log("MapManager data loaded successfully.");
    }

    public void TestSaveAndLoadMapManagerData()
    {
        Debug.Log("TestSaveAndLoadMapManagerData called.");

        if (MapManager.Instance == null)
        {
            Debug.LogError("MapManager.Instance is null. Cannot test save and load.");
            return;
        }

        // 修改部分地图数据作为测试
        MapManager.Instance.mapDatas[0, 0].type = MapManager.tileTypes.water;
        MapManager.Instance.mapDatas[0, 0].can_walk = false;
        MapManager.Instance.mapDatas[0, 0].has_print = true;
        MapManager.Instance.mapDatas[0, 0].has_building = true;

        // 保存数据
        SaveMapManager();

        // 清空现有数据
        MapManager.Instance.GenerateMapData();

        // 加载数据
        LoadMapManager();

        // 验证加载的数据
        var mapData = MapManager.Instance.mapDatas[0, 0];
        Debug.Log($"Tile at (0, 0): Type = {mapData.type}, CanWalk = {mapData.can_walk}, HasPrint = {mapData.has_print}, HasBuilding = {mapData.has_building}");
    }
    #endregion

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

    void Start()
    {
        //代码运行的时候可能会出现ArgumentException: An item with the same key has already been added
        //这个是正常现象，因为在测试的时候会重复添加相同的key
        //TestSaveAndLoadBuildManagerData();
        //TestSaveAndLoadCropManagerData(); // 调用测试方法
        //TestSaveAndLoadItemManagerData(); // 调用测试方法
        //TestSaveAndLoadMapManagerData(); // 调用测试方法
    }
    void OnDestroy()
    {
        // 在销毁时保存数据
        //SaveBuildManager();
        //SaveCropManager();
        //SaveItemManager();
        //SaveMapManager();
    }

    // 保存 CropManager 的静态数据
    
}
