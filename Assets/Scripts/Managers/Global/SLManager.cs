using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // 用于文件操作
using Newtonsoft.Json; // 用于JSON序列化和反序列化
using System.Linq; // 用于列表操作
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
    // #region "BuildManagerData"
    // [System.Serializable]
    
    // public class BuildManagerData
    // {
    //     public Dictionary<BuildingType, List<BuildingData>> buildingLists;

    //     [System.Serializable]
    //     public class BuildingData
    //     {
    //         public int id;
    //         public string name;
    //         public string texturePath; // 存储纹理路径
    //         public BuildingType type;
    //         public int width, height;
    //         public int durability;
    //         public bool can_build;
    //         public bool can_walk;
    //         public bool can_plant;
    //         public List<KeyValuePair<int, int>> material_list;
    //     }
    // }

    // private string savePath => Path.Combine(Application.persistentDataPath, "BuildManagerData.json");

    // // 保存 BuildManager 的静态数据
    // public void SaveBuildManager()
    // {
    //     BuildManagerData data = new BuildManagerData
    //     {
    //         buildingLists = new Dictionary<BuildingType, List<BuildManagerData.BuildingData>>()
    //     };

    //     foreach (var kvp in BuildManager.Instance.buildingLists)
    //     {
    //         var buildingDataList = new List<BuildManagerData.BuildingData>();
    //         foreach (var building in kvp.Value)
    //         {
    //             buildingDataList.Add(new BuildManagerData.BuildingData
    //             {
    //                 id = building.id,
    //                 name = building.build_name,
    //                 texturePath = building.texture != null ? building.texture.name : null, // 假设纹理名可用作路径
    //                 type = building.type,
    //                 width = building.width,
    //                 height = building.height,
    //                 durability = building.durability,
    //                 can_build = building.can_build,
    //                 can_walk = building.can_walk,
    //                 can_plant = building.can_plant,
    //                 material_list = building.material_list
    //             });
    //         }
    //         data.buildingLists[kvp.Key] = buildingDataList;
    //     }

    //     string json = JsonConvert.SerializeObject(data, Formatting.Indented);
    //     File.WriteAllText(savePath, json);
    //     Debug.Log($"BuildManager data saved to {savePath}");
    // }

    // // 加载 BuildManager 的静态数据
    // public void LoadBuildManager()
    // {
    //     if (!File.Exists(savePath))
    //     {
    //         Debug.LogWarning("Save file not found!");
    //         return;
    //     }

    //     string json = File.ReadAllText(savePath);
    //     BuildManagerData data = JsonConvert.DeserializeObject<BuildManagerData>(json);

    //     foreach (var kvp in data.buildingLists)
    //     {
    //         if (!BuildManager.Instance.buildingLists.ContainsKey(kvp.Key))
    //             BuildManager.Instance.buildingLists[kvp.Key] = new List<Building>();

    //         foreach (var buildingData in kvp.Value)
    //         {
    //             BuildManager.Instance.buildingLists[kvp.Key].Add(new Building
    //             {
    //                 id = buildingData.id,
    //                 build_name = buildingData.name,
    //                 texture = Resources.Load<Sprite>(buildingData.texturePath), // 假设纹理存储在 Resources 文件夹
    //                 type = buildingData.type,
    //                 width = buildingData.width,
    //                 height = buildingData.height,
    //                 durability = buildingData.durability,
    //                 can_build = buildingData.can_build,
    //                 can_walk = buildingData.can_walk,
    //                 can_plant = buildingData.can_plant,
    //                 material_list = buildingData.material_list
    //             });
    //         }
    //     }

    //     Debug.Log("BuildManager data loaded successfully.");
    // }

    // // 测试保存和加载 BuildManager 数据的方法
    // public void TestSaveAndLoadBuildManagerData()
    // {
    //     Debug.Log("TestSaveAndLoadBuildManagerData called.");
        
    //     // 创建测试数据
    //     BuildManager.Instance.buildingLists = new Dictionary<BuildingType, List<Building>>();

    //     BuildManager.Instance.buildingLists[BuildingType.Dev] = new List<Building>
    //     {
    //         new Building
    //         {
    //             id = 1,
    //             build_name = "Test Dev Building",
    //             texture = null, // 假设没有纹理
    //             type = BuildingType.Dev,
    //             width = 5,
    //             height = 5,
    //             durability = 100,
    //             can_build = true,
    //             can_walk = false,
    //             can_plant = false,
    //             material_list = new List<KeyValuePair<int, int>> { new KeyValuePair<int, int>(1, 10) }
    //         }
    //     };

    //     // 保存数据
    //     SaveBuildManager();

    //     // 清空现有数据
    //     BuildManager.Instance.buildingLists.Clear();

    //     // 加载数据
    //     LoadBuildManager();
    //     Debug.Log("Data loaded from file.");

    //     // 验证加载的数据
    //     foreach (var kvp in BuildManager.Instance.buildingLists)
    //     {
    //         Debug.Log($"Building Type: {kvp.Key}");
    //         foreach (var building in kvp.Value)
    //         {
    //             Debug.Log($"Building Name: {building.build_name}, ID: {building.id}, Durability: {building.durability}");
    //         }
    //     }
    // }
    // #endregion

    // #region "CropManagerData"
    // [System.Serializable]
    // public class CropManagerData
    // {
    //     public List<CropData> cropList;

    //     [System.Serializable]
    //     public class CropData
    //     {
    //         public int id;
    //         public string name;
    //         public float lifetime;
    //     }
    // }

    // private string cropSavePath => Path.Combine(Application.persistentDataPath, "CropManagerData.json");

    // public void SaveCropManager()
    // {
    //     if (CropManager.Instance == null)
    //     {
    //         Debug.LogError("CropManager.Instance is null. Cannot save data.");
    //         return;
    //     }

    //     CropManagerData data = new CropManagerData
    //     {
    //         cropList = new List<CropManagerData.CropData>()
    //     };

    //     foreach (var crop in CropManager.Instance.cropList)
    //     {
    //         data.cropList.Add(new CropManagerData.CropData
    //         {
    //             id = crop.id,
    //             name = crop.name,
    //             lifetime = crop.lifetime
    //         });
    //     }

    //     string json = JsonConvert.SerializeObject(data, Formatting.Indented);
    //     File.WriteAllText(cropSavePath, json);
    //     Debug.Log($"CropManager data saved to {cropSavePath}");
    // }

    // // 加载 CropManager 的静态数据
    // public void LoadCropManager()
    // {
    //     if (!File.Exists(cropSavePath))
    //     {
    //         Debug.LogWarning("CropManager save file not found!");
    //         return;
    //     }

    //     if (CropManager.Instance == null)
    //     {
    //         Debug.LogError("CropManager.Instance is null. Cannot load data.");
    //         return;
    //     }

    //     string json = File.ReadAllText(cropSavePath);
    //     CropManagerData data = JsonConvert.DeserializeObject<CropManagerData>(json);

    //     CropManager.Instance.cropList.Clear();
    //     foreach (var cropData in data.cropList)
    //     {
    //         CropManager.Instance.cropList.Add(new CropManager.Crop
    //         {
    //             id = cropData.id,
    //             name = cropData.name,
    //             lifetime = cropData.lifetime
    //         });
    //     }

    //     Debug.Log("CropManager data loaded successfully.");
    // }

    // // 测试保存和加载 CropManager 数据的方法
    // public void TestSaveAndLoadCropManagerData()
    // {
    //     Debug.Log("TestSaveAndLoadCropManagerData called.");

    //     if (CropManager.Instance == null)
    //     {
    //         Debug.LogError("CropManager.Instance is null. Cannot test save and load.");
    //         return;
    //     }

    //     // 创建测试数据
    //     CropManager.Instance.cropList = new List<CropManager.Crop>
    //     {
    //         new CropManager.Crop { id = 0, name = "测试作物1", lifetime = 10.0f },
    //         new CropManager.Crop { id = 1, name = "测试作物2", lifetime = 5.0f }
    //     };

    //     // 保存数据
    //     SaveCropManager();

    //     // 清空现有数据
    //     CropManager.Instance.cropList.Clear();

    //     // 加载数据
    //     LoadCropManager();

    //     // 验证加载的数据
    //     foreach (var crop in CropManager.Instance.cropList)
    //     {
    //         Debug.Log($"Crop Name: {crop.name}, ID: {crop.id}, Lifetime: {crop.lifetime}");
    //     }
    // }
    // #endregion

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


    #region "TaskManagerData"
    [System.Serializable]
    public class TaskManagerData
    {
        public List<TaskData> availableTasks;
        public List<TaskData> inavailableTasks;

        [System.Serializable]
        public class TaskData
        {
            public Vector3Int target_position;
            public TaskManager.TaskTypes type;
            public int task_id;
            public int id;
            public int amount;
            public int materialType;
        }
    }

    private string taskSavePath => Path.Combine(Application.persistentDataPath, "TaskManagerData.json");

    public void SaveTaskManager()
    {
        if (TaskManager.Instance == null)
        {
            Debug.LogError("TaskManager.Instance is null. Cannot save data.");
            return;
        }

        TaskManagerData data = new TaskManagerData
        {
            availableTasks = TaskManager.Instance.availableTaskList.Select(task => new TaskManagerData.TaskData
            {
                target_position = task.target_position,
                type = task.type,
                task_id = task.task_id,
                id = task.id,
                amount = task.amount,
                materialType = task.materialType
            }).ToList(),
            inavailableTasks = TaskManager.Instance.inavailableTaskList.Select(task => new TaskManagerData.TaskData
            {
                target_position = task.target_position,
                type = task.type,
                task_id = task.task_id,
                id = task.id,
                amount = task.amount,
                materialType = task.materialType
            }).ToList()
        };

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(taskSavePath, json);
        Debug.Log($"TaskManager data saved to {taskSavePath}");
    }

    public void LoadTaskManager()
    {
        if (!File.Exists(taskSavePath))
        {
            Debug.LogWarning("TaskManager save file not found!");
            return;
        }

        if (TaskManager.Instance == null)
        {
            Debug.LogError("TaskManager.Instance is null. Cannot load data.");
            return;
        }

        string json = File.ReadAllText(taskSavePath);
        TaskManagerData data = JsonConvert.DeserializeObject<TaskManagerData>(json);

        TaskManager.Instance.availableTaskList.Clear();
        TaskManager.Instance.inavailableTaskList.Clear();

        foreach (var taskData in data.availableTasks)
        {
            TaskManager.Instance.availableTaskList.Add(new TaskManager.Task(
                position: taskData.target_position,
                type: taskData.type,
                task_id: taskData.task_id,
                id: taskData.id,
                amount: taskData.amount,
                materialType: taskData.materialType
            ));
        }

        foreach (var taskData in data.inavailableTasks)
        {
            TaskManager.Instance.inavailableTaskList.Add(new TaskManager.Task(
                position: taskData.target_position,
                type: taskData.type,
                task_id: taskData.task_id,
                id: taskData.id,
                amount: taskData.amount,
                materialType: taskData.materialType
            ));
        }

        Debug.Log("TaskManager data loaded successfully.");
    }

    public void TestSaveAndLoadTaskManagerData()
    {
        Debug.Log("TestSaveAndLoadTaskManagerData called.");

        if (TaskManager.Instance == null)
        {
            Debug.LogError("TaskManager.Instance is null. Cannot test save and load.");
            return;
        }

        // 创建测试数据
        TaskManager.Instance.availableTaskList = new List<TaskManager.Task>
        {
            new TaskManager.Task(new Vector3Int(0, 0, 0), TaskManager.TaskTypes.Build, 1, 101, 5, -1),
            new TaskManager.Task(new Vector3Int(1, 1, 1), TaskManager.TaskTypes.Plant, 2, 102, 10, -1)
        };

        TaskManager.Instance.inavailableTaskList = new List<TaskManager.Task>
        {
            new TaskManager.Task(new Vector3Int(2, 2, 2), TaskManager.TaskTypes.Harvest, 3, 103, 15, -1)
        };

        // 保存数据
        SaveTaskManager();

        // 清空现有数据
        TaskManager.Instance.availableTaskList.Clear();
        TaskManager.Instance.inavailableTaskList.Clear();

        // 加载数据
        LoadTaskManager();

        // 验证加载的数据
        foreach (var task in TaskManager.Instance.availableTaskList)
        {
            Debug.Log($"Available Task - ID: {task.task_id}, Type: {task.type}, Position: {task.target_position}, id: {task.id}, amount: {task.amount}, materialType: {task.materialType}");
        }

        foreach (var task in TaskManager.Instance.inavailableTaskList)
        {
            Debug.Log($"Inavailable Task - ID: {task.task_id}, Type: {task.type}, Position: {task.target_position}, id: {task.id}, amount: {task.amount}, materialType: {task.materialType}");
        }
    }
    #endregion

    
    #region "PawnManagerData"
    [System.Serializable]
    public class PawnManagerData
    {
        public List<PawnData> pawns;

        [System.Serializable]
        public class PawnData
        {
            public int id;
            public bool isOnTask;
            public TaskData handlingTask;
            public float moveSpeed;
            public float workSpeed;
            public int capacity;
            public ToolData handlingTool;
            public List<TaskData> pawnTaskList;
            public SerializableVector3 position;
        }

        [System.Serializable]
        public class TaskData
        {
            public Vector3Int target_position;
            public TaskManager.TaskTypes type;
            public int task_id;
            public int id;
            public int amount;
            public int materialType;
        }

        [System.Serializable]
        public class ToolData
        {
            public int id;
            public string name;
            public string texturePath;
            public int max_durability;
            public Dictionary<string, int> enhancements;
        }

        [System.Serializable]
        public class SerializableVector3
        {
            public float x;
            public float y;
            public float z;

            public SerializableVector3() { }
            public SerializableVector3(Vector3 vector)
            {
                x = vector.x;
                y = vector.y;
                z = vector.z;
            }
            public Vector3 ToVector3()
            {
                return new Vector3(x, y, z);
            }
        }
    }

    private string pawnSavePath => Path.Combine(Application.persistentDataPath, "PawnManagerData.json");

    public void SavePawnManager()
    {
        if (PawnManager.Instance == null)
        {
            Debug.LogError("PawnManager.Instance is null. Cannot save data.");
            return;
        }

        PawnManagerData data = new PawnManagerData
        {
            pawns = new List<PawnManagerData.PawnData>()
        };

        foreach (var pawn in PawnManager.Instance.pawns)
        {
            var pawnData = new PawnManagerData.PawnData
            {
                id = pawn.id,
                isOnTask = pawn.isOnTask,
                handlingTask = pawn.handlingTask != null ? new PawnManagerData.TaskData
                {
                    target_position = pawn.handlingTask.target_position,
                    type = pawn.handlingTask.type,
                    task_id = pawn.handlingTask.task_id,
                    id = pawn.handlingTask.id,
                    amount = pawn.handlingTask.amount,
                    materialType = pawn.handlingTask.materialType
                } : null,
                moveSpeed = pawn.moveSpeed,
                workSpeed = pawn.workSpeed,
                capacity = pawn.capacity,
                handlingTool = pawn.handlingTool != null ? new PawnManagerData.ToolData
                {
                    id = pawn.handlingTool.id,
                    name = pawn.handlingTool.name,
                    texturePath = pawn.handlingTool.texture != null ? pawn.handlingTool.texture.name : null,
                    max_durability = pawn.handlingTool.max_durability,
                    enhancements = pawn.handlingTool.enhancements.ToDictionary(e => e.Key.ToString(), e => e.Value)
                } : null,
                pawnTaskList = pawn.PawntaskList.Select(task => new PawnManagerData.TaskData
                {
                    target_position = task.target_position,
                    type = task.type,
                    task_id = task.task_id,
                    id = task.id,
                    amount = task.amount,
                    materialType = task.materialType
                }).ToList(),
                position = pawn.Instance != null ? new PawnManagerData.SerializableVector3(pawn.Instance.transform.position) : new PawnManagerData.SerializableVector3(Vector3.zero)
            };

            data.pawns.Add(pawnData);
        }

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(pawnSavePath, json);
        Debug.Log($"PawnManager data saved to {pawnSavePath}");
    }

    public void LoadPawnManager()
    {
        if (!File.Exists(pawnSavePath))
        {
            Debug.LogWarning("PawnManager save file not found!");
            return;
        }

        if (PawnManager.Instance == null)
        {
            Debug.LogError("PawnManager.Instance is null. Cannot load data.");
            return;
        }

        string json = File.ReadAllText(pawnSavePath);
        PawnManagerData data = JsonConvert.DeserializeObject<PawnManagerData>(json);

        PawnManager.Instance.pawns.Clear();

        foreach (var pawnData in data.pawns)
        {
            var pawn = new PawnManager.Pawn(pawnData.id)
            {
                isOnTask = pawnData.isOnTask,
                handlingTask = pawnData.handlingTask != null ? new TaskManager.Task(
                    position: pawnData.handlingTask.target_position,
                    type: pawnData.handlingTask.type,
                    task_id: pawnData.handlingTask.task_id,
                    id: pawnData.handlingTask.id,
                    amount: pawnData.handlingTask.amount,
                    materialType: pawnData.handlingTask.materialType
                ) : null,
                moveSpeed = pawnData.moveSpeed,
                workSpeed = pawnData.workSpeed,
                capacity = pawnData.capacity,
                handlingTool = pawnData.handlingTool != null ? new ItemManager.Tool
                {
                    id = pawnData.handlingTool.id,
                    name = pawnData.handlingTool.name,
                    texture = Resources.Load<Sprite>(pawnData.handlingTool.texturePath),
                    max_durability = pawnData.handlingTool.max_durability,
                    enhancements = pawnData.handlingTool.enhancements != null
                        ? pawnData.handlingTool.enhancements.ToDictionary(
                            e => (PawnManager.Pawn.EnhanceType)System.Enum.Parse(typeof(PawnManager.Pawn.EnhanceType), e.Key),
                            e => e.Value)
                        : new Dictionary<PawnManager.Pawn.EnhanceType, int>()
                } : null,
                PawntaskList = pawnData.pawnTaskList != null ? pawnData.pawnTaskList.Select(task => new TaskManager.Task(
                    position: task.target_position,
                    type: task.type,
                    task_id: task.task_id,
                    id: task.id,
                    amount: task.amount,
                    materialType: task.materialType
                )).ToList() : new List<TaskManager.Task>()
            };

            // 实例化Pawn对象并设置位置
            PawnManager.Instance.InstantiatePawn(pawn, Vector3Int.zero);
            if (pawn.Instance != null && pawnData.position != null)
            {
                pawn.Instance.transform.position = pawnData.position.ToVector3();
            }

            PawnManager.Instance.pawns.Add(pawn);
        }

        Debug.Log("PawnManager data loaded successfully.");
    }

    public void TestSaveAndLoadPawnManagerData()
    {
        Debug.Log("TestSaveAndLoadPawnManagerData called.");

        if (PawnManager.Instance == null)
        {
            Debug.LogError("PawnManager.Instance is null. Cannot test save and load.");
            return;
        }

        // 创建测试数据
        PawnManager.Instance.pawns = new List<PawnManager.Pawn>
        {
            new PawnManager.Pawn(1)
            {
                isOnTask = true,
                handlingTask = new TaskManager.Task(new Vector3Int(1, 1, 1), TaskManager.TaskTypes.Build, 1, 101, 5, -1),
                moveSpeed = 3.0f,
                workSpeed = 2.0f,
                capacity = 100,
                handlingTool = new ItemManager.Tool
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
                },
                PawntaskList = new List<TaskManager.Task>
                {
                    new TaskManager.Task(new Vector3Int(2, 2, 2), TaskManager.TaskTypes.Plant, 2, 102, 10, -1)
                }
            }
        };

        // 保存数据
        SavePawnManager();

        // 清空现有数据
        PawnManager.Instance.pawns.Clear();

        // 加载数据
        LoadPawnManager();

        // 验证加载的数据
        foreach (var pawn in PawnManager.Instance.pawns)
        {
            Debug.Log($"Pawn ID: {pawn.id}, Task Count: {pawn.PawntaskList.Count}");
        }
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
        //TestSaveAndLoadTaskManagerData(); // 调用测试方法
        //TestSaveAndLoadPawnManagerData(); // 调用测试方法
    }
    void OnDestroy()
    {
        // 在销毁时保存数据
        //SaveBuildManager();
        //SaveCropManager();
        //SaveItemManager();
        //SaveMapManager();
        //SaveTaskManager();
        //SavePawnManager();
        //Debug.Log("Data saved on destroy.");
    }

    // 保存 CropManager 的静态数据
    
}
