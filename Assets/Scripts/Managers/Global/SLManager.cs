using System;
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
    #region "BuildManagerData"
    [System.Serializable]
    
    public class BuildManagerData
    {
        public Dictionary<BuildingType, List<BuildingData>> buildingLists;

        [System.Serializable]
        public class BuildingData
        {
            public int id;
            public string name;
            public string texturePath; // 存储纹理路径
            public BuildingType type;
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
            buildingLists = new Dictionary<BuildingType, List<BuildManagerData.BuildingData>>()
        };

        foreach (var kvp in BuildManager.Instance.buildingLists)
        {
            var buildingDataList = new List<BuildManagerData.BuildingData>();
            foreach (var building in kvp.Value)
            {
                buildingDataList.Add(new BuildManagerData.BuildingData
                {
                    id = building.id,
                    name = building.build_name,
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
                BuildManager.Instance.buildingLists[kvp.Key] = new List<Building>();

            foreach (var buildingData in kvp.Value)
            {
                BuildManager.Instance.buildingLists[kvp.Key].Add(new Building
                {
                    id = buildingData.id,
                    build_name = buildingData.name,
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
        BuildManager.Instance.buildingLists = new Dictionary<BuildingType, List<Building>>();

        BuildManager.Instance.buildingLists[BuildingType.Dev] = new List<Building>
        {
            new Building
            {
                id = 1,
                build_name = "Test Dev Building",
                texture = null, // 假设没有纹理
                type = BuildingType.Dev,
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
                Debug.Log($"Building Name: {building.build_name}, ID: {building.id}, Durability: {building.durability}");
            }
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
    public class MapManagerData
    {
        public List<SerializableMapData> tiles = new();
        public bool randomSeed;
        public int seed;
        public float lac;
        public float minValue;
        public float maxValue;

        public class SerializableMapData
        {
            public int x, y;                       // 坐标
            public int type;                       // MapManager.tileTypes 枚举索引

            public bool has_item;
            public ItemInstanceData item;

            public bool has_pawn;
            public bool will_has_pawn;

            public bool can_walk;
            public bool can_build;
            public bool can_plant;

            public float fertility;
            public float humidity;
            public float light;
            public float walk_speed;
        }

        public class ItemInstanceData
        {
            public int item_id;                    // 物品/建筑 id
            public int amount;                     // 堆叠数量
            public string item_type;               // ItemInstanceManager.ItemInstanceType 字符串
        }
    }

    /* ----------------------------------------------------------------------
     *  文件路径
     * ------------------------------------------------------------------ */
    private string MapSavePath =>
        Path.Combine(Application.persistentDataPath, "MapManagerData.json");

    /* ----------------------------------------------------------------------
     *  保存 MapManager
     * ------------------------------------------------------------------ */
    public void SaveMapManager()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogError("MapManager.Instance is null – cannot save map data.");
            return;
        }

        MapManagerData saveData = new()
        {
            randomSeed = MapManager.Instance.random_seed,
            seed       = MapManager.Instance.seed,
            lac        = MapManager.Instance.lac,
            minValue   = MapManager.Instance.min_value,
            maxValue   = MapManager.Instance.max_value
        };

        for (int x = 0; x < MapManager.MAP_SIZE; x++)
        {
            for (int y = 0; y < MapManager.MAP_SIZE; y++)
            {
                MapManager.MapData src = MapManager.Instance.mapDatas[x, y];
                if (src == null) continue; // 异常保护

                MapManagerData.SerializableMapData dst = new()
                {
                    x = x,
                    y = y,
                    type = (int)src.type,

                    has_item      = src.has_item,
                    has_pawn      = src.has_pawn,
                    will_has_pawn = src.will_has_pawn,

                    can_walk  = src.can_walk,
                    can_build = src.can_build,
                    can_plant = src.can_plant,

                    fertility  = src.fertility,
                    humidity   = src.humidity,
                    light      = src.light,
                    walk_speed = src.walk_speed
                };

                if (src.item != null)
                {
                    int amount = 1;
                    // 只有 MaterialInstance 才有 GetAmount 方法
                    if (src.item is ItemInstanceManager.MaterialInstance materialInstance)
                    {
                        amount = materialInstance.GetAmount();
                    }
                    // 其他类型可根据需要设置 amount，默认 1
                    dst.item = new MapManagerData.ItemInstanceData
                    {
                        item_id   = src.item.item_id,
                        amount    = amount,
                        item_type = src.item.GetType().Name // 例如 CropInstance
                    };
                }

                saveData.tiles.Add(dst);
            }
        }

        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(MapSavePath, json);
        Debug.Log($"[SLManager] Map data saved → {MapSavePath}");
    }

    /* ----------------------------------------------------------------------
     *  加载 MapManager
     * ------------------------------------------------------------------ */
    public void LoadMapManager()
    {
        if (!File.Exists(MapSavePath))
        {
            Debug.LogWarning("Map save file not found – aborting load.");
            return;
        }
        if (MapManager.Instance == null)
        {
            Debug.LogError("MapManager.Instance is null – cannot load map data.");
            return;
        }

        string json = File.ReadAllText(MapSavePath);
        MapManagerData data = JsonConvert.DeserializeObject<MapManagerData>(json);
        if (data == null)
        {
            Debug.LogError("Failed to deserialize map data.");
            return;
        }

        /* ---- 还原基础参数 ---- */
        MapManager.Instance.random_seed = data.randomSeed;
        MapManager.Instance.seed        = data.seed;
        MapManager.Instance.lac         = data.lac;
        MapManager.Instance.min_value   = data.minValue;
        MapManager.Instance.max_value   = data.maxValue;

        /* ---- 逐格加载 ---- */
        foreach (var tile in data.tiles)
        {
            int x = tile.x;
            int y = tile.y;
            if (x < 0 || x >= MapManager.MAP_SIZE || y < 0 || y >= MapManager.MAP_SIZE) continue;

            MapManager.MapData dst = MapManager.Instance.mapDatas[x, y] ?? new MapManager.MapData();
            MapManager.Instance.mapDatas[x, y] = dst;

            dst.position = new Vector3Int(x, y, 0);
            dst.type     = (MapManager.tileTypes)tile.type;
            dst.texture  = MapManager.Instance.tiles[(int)dst.type];

            dst.has_item      = tile.has_item;
            dst.has_pawn      = tile.has_pawn;
            dst.will_has_pawn = tile.will_has_pawn;

            dst.can_walk  = tile.can_walk;
            dst.can_build = tile.can_build;
            dst.can_plant = tile.can_plant;

            dst.fertility  = tile.fertility;
            dst.humidity   = tile.humidity;
            dst.light      = tile.light;
            dst.walk_speed = tile.walk_speed;

            /* ---- 还原 item ---- */
            if (tile.item != null)
            {
                ItemInstanceManager.ItemInstanceType instType = (ItemInstanceManager.ItemInstanceType)Enum.Parse(
                    typeof(ItemInstanceManager.ItemInstanceType),
                    tile.item.item_type); // 不要Replace

                dst.item = ItemInstanceManager.Instance.SpawnItem(
                    dst.position,
                    tile.item.item_id,
                    instType,
                    tile.item.amount);
            }
            else if (dst.item != null)
            {
                // 原本有 item 但存档说没有 → 清理
                ItemInstanceManager.Instance.DestroyItem(dst.item, ItemInstanceManager.DestroyMode.RemainNone);
                dst.item = null;
            }

            /* ---- 更新瓦片外观 ---- */
            MapManager.Instance.landTilemap.SetTile(dst.position, dst.texture);

            /* ---- 更新 walkVectors ---- */
            MapManager.Instance.walkVectors[x, y] = dst.can_walk;
        }

        Debug.Log("[SLManager] Map data loaded successfully.");
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
            new TaskManager.Task(new Vector3Int(1, 1, 1), TaskManager.TaskTypes.PlantALL, 2, 102, 10, -1)
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
            public int instantCapacity; // 新增
            public int materialId;      // 新增
            public int materialAmount;  // 新增
            public int materialType;    // 新增（枚举转int存储）
            public AttributeData attributes; // 新增
            public ToolData handlingTool;
            public List<TaskData> pawnTaskList;
            public SerializableVector3 position;
        }

        [System.Serializable]
        public class AttributeData // 新增
        {
            public int plant;
            public int harvest;
            public int build;
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
                instantCapacity = pawn.instantCapacity, // 新增
                materialId = pawn.materialId,           // 新增
                materialAmount = pawn.materialAmount,   // 新增
                materialType = (int)pawn.materialType,  // 新增
                attributes = new PawnManagerData.AttributeData // 新增
                {
                    plant = pawn.attributes.plant,
                    harvest = pawn.attributes.harvest,
                    build = pawn.attributes.build
                },
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
                instantCapacity = pawnData.instantCapacity, // 新增
                materialId = pawnData.materialId,           // 新增
                materialAmount = pawnData.materialAmount,   // 新增
                materialType = (ItemInstanceManager.ItemInstanceType)pawnData.materialType, // 新增
                attributes = pawnData.attributes != null ? new PawnManager.attribute(
                    pawnData.attributes.plant,
                    pawnData.attributes.harvest,
                    pawnData.attributes.build
                ) : new PawnManager.attribute(0, 0, 0), // 新增
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
                    new TaskManager.Task(new Vector3Int(2, 2, 2), TaskManager.TaskTypes.PlantALL, 2, 102, 10, -1)
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

    #region "ItemInstanceManagerData"
    [System.Serializable]
    public class ItemInstanceManagerData
    {
        public List<ItemInstanceData> instances;

        [System.Serializable]
        public class ItemInstanceData
        {
            public int id;
            public string type; // 用字符串存储类型
            public int item_id;
            public SerializableVector3Int position;
            // ToolInstance
            public int durability;
            // MaterialInstance
            public int amount;
            // CropInstance
            public float growth;
            public float real_lifetime;
            public float growth_per_frame;
            public List<KeyValuePair<int, int>> harvest_list;
            // BuildingInstance
            public List<KeyValuePair<int, int>> material_list;
            public int content_id; // 存储内容物id
            // PrintInstance
            public List<PrintMaterialProgress> print_material_list;
            // ResourceInstance
            public int resource_durability; // 新增
        }

        [System.Serializable]
        public class PrintMaterialProgress
        {
            public int item_id;
            public int current;
            public int need;
        }

        [System.Serializable]
        public class SerializableVector3Int
        {
            public int x, y, z;
            public SerializableVector3Int() { }
            public SerializableVector3Int(Vector3Int v) { x = v.x; y = v.y; z = v.z; }
            public Vector3Int ToVector3Int() => new Vector3Int(x, y, z);
        }
    }

    private string itemInstanceSavePath => Path.Combine(Application.persistentDataPath, "ItemInstanceManagerData.json");

    public void SaveItemInstanceManager()
    {
        if (ItemInstanceManager.Instance == null)
        {
            Debug.LogError("ItemInstanceManager.Instance is null. Cannot save data.");
            return;
        }

        ItemInstanceManagerData data = new ItemInstanceManagerData
        {
            instances = new List<ItemInstanceManagerData.ItemInstanceData>()
        };

        foreach (var kvp in ItemInstanceManager.Instance.itemInstanceLists)
        {
            foreach (var itemInstance in kvp.Value)
            {
                var d = new ItemInstanceManagerData.ItemInstanceData
                {
                    id = itemInstance.id,
                    type = itemInstance.type.ToString(),
                    item_id = itemInstance.item_id,
                    position = new ItemInstanceManagerData.SerializableVector3Int(itemInstance.position)
                };

                switch (itemInstance.type)
                {
                    case ItemInstanceManager.ItemInstanceType.ToolInstance:
                        d.durability = ((ItemInstanceManager.ToolInstance)itemInstance).durability;
                        break;
                    case ItemInstanceManager.ItemInstanceType.MaterialInstance:
                        d.amount = ((ItemInstanceManager.MaterialInstance)itemInstance).amount;
                        break;
                    case ItemInstanceManager.ItemInstanceType.CropInstance:
                        var crop = (ItemInstanceManager.CropInstance)itemInstance;
                        d.growth = crop.growth;
                        d.real_lifetime = crop.real_lifetime;
                        d.growth_per_frame = crop.growth_per_frame;
                        d.harvest_list = crop.harvest_list != null ? new List<KeyValuePair<int, int>>(crop.harvest_list) : null;
                        break;
                    case ItemInstanceManager.ItemInstanceType.BuildingInstance:
                        var build = (ItemInstanceManager.BuildingInstance)itemInstance;
                        d.durability = build.durability;
                        d.material_list = build.material_list != null ? new List<KeyValuePair<int, int>>(build.material_list) : null;
                        d.content_id = build.content != null ? build.content.id : -1;
                        break;
                    case ItemInstanceManager.ItemInstanceType.PrintInstance:
                        var print = (ItemInstanceManager.PrintInstance)itemInstance;
                        d.print_material_list = new List<ItemInstanceManagerData.PrintMaterialProgress>();
                        if (print.material_list != null)
                        {
                            foreach (var p in print.material_list)
                            {
                                d.print_material_list.Add(new ItemInstanceManagerData.PrintMaterialProgress
                                {
                                    item_id = p.Key,
                                    current = p.Value.current,
                                    need = p.Value.need
                                });
                            }
                        }
                        break;
                    case ItemInstanceManager.ItemInstanceType.ResourceInstance:
                        d.resource_durability = ((ItemInstanceManager.ResourceInstance)itemInstance).durability;
                        break;
                }
                data.instances.Add(d);
            }
        }

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(itemInstanceSavePath, json);
        Debug.Log($"ItemInstanceManager data saved to {itemInstanceSavePath}");
    }

    public void LoadItemInstanceManager()
    {
        if (!File.Exists(itemInstanceSavePath))
        {
            Debug.LogWarning("ItemInstanceManager save file not found!");
            return;
        }

        if (ItemInstanceManager.Instance == null)
        {
            Debug.LogError("ItemInstanceManager.Instance is null. Cannot load data.");
            return;
        }

        string json = File.ReadAllText(itemInstanceSavePath);
        ItemInstanceManagerData data = JsonConvert.DeserializeObject<ItemInstanceManagerData>(json);

        // 清空现有数据
        ItemInstanceManager.Instance.itemInstanceLists.Clear();

        foreach (var d in data.instances)
        {
            var type = (ItemInstanceManager.ItemInstanceType)System.Enum.Parse(typeof(ItemInstanceManager.ItemInstanceType), d.type);
            ItemInstanceManager.ItemInstance ins = null;
            switch (type)
            {
                case ItemInstanceManager.ItemInstanceType.ToolInstance:
                    ins = new ItemInstanceManager.ToolInstance
                    {
                        id = d.id,
                        type = type,
                        item_id = d.item_id,
                        position = d.position.ToVector3Int(),
                        durability = d.durability
                    };
                    break;
                case ItemInstanceManager.ItemInstanceType.MaterialInstance:
                    ins = new ItemInstanceManager.MaterialInstance
                    {
                        id = d.id,
                        type = type,
                        item_id = d.item_id,
                        position = d.position.ToVector3Int(),
                        amount = d.amount
                    };
                    break;
                case ItemInstanceManager.ItemInstanceType.CropInstance:
                    ins = new ItemInstanceManager.CropInstance
                    {
                        id = d.id,
                        type = type,
                        item_id = d.item_id,
                        position = d.position.ToVector3Int(),
                        growth = d.growth,
                        real_lifetime = d.real_lifetime,
                        growth_per_frame = d.growth_per_frame,
                        harvest_list = d.harvest_list != null ? new List<KeyValuePair<int, int>>(d.harvest_list) : null
                    };
                    break;
                case ItemInstanceManager.ItemInstanceType.BuildingInstance:
                    ins = new ItemInstanceManager.BuildingInstance
                    {
                        id = d.id,
                        type = type,
                        item_id = d.item_id,
                        position = d.position.ToVector3Int(),
                        durability = d.durability,
                        material_list = d.material_list != null ? new List<KeyValuePair<int, int>>(d.material_list) : null,
                        // content 暂时置空，后面统一设置
                    };
                    break;
                case ItemInstanceManager.ItemInstanceType.PrintInstance:
                    var print = new ItemInstanceManager.PrintInstance
                    {
                        id = d.id,
                        type = type,
                        item_id = d.item_id,
                        position = d.position.ToVector3Int(),
                        material_list = new List<KeyValuePair<int, ItemInstanceManager.PrintInstance.Progress>>()
                    };
                    if (d.print_material_list != null)
                    {
                        foreach (var p in d.print_material_list)
                        {
                            print.material_list.Add(new KeyValuePair<int, ItemInstanceManager.PrintInstance.Progress>(
                                p.item_id,
                                new ItemInstanceManager.PrintInstance.Progress { current = p.current, need = p.need }
                            ));
                        }
                    }
                    ins = print;
                    break;
                case ItemInstanceManager.ItemInstanceType.ResourceInstance:
                    ins = new ItemInstanceManager.ResourceInstance
                    {
                        id = d.id,
                        type = type,
                        item_id = d.item_id,
                        position = d.position.ToVector3Int(),
                        durability = d.resource_durability
                    };
                    break;
            }
            if (ins != null)
            {
                Sprite texture = null;
                // 例如：texture = ItemManager.Instance.GetItem(ins.item_id)?.texture;
                texture = ItemManager.Instance.GetItem(ins.item_id)?.texture;
                ItemInstanceManager.Instance.InitInstance(ins, texture);
                if (!ItemInstanceManager.Instance.itemInstanceLists.ContainsKey(type))
                    ItemInstanceManager.Instance.itemInstanceLists[type] = new List<ItemInstanceManager.ItemInstance>();
                ItemInstanceManager.Instance.itemInstanceLists[type].Add(ins);
                // 如果有idToInstance等索引，也可以在这里同步
            }
        }

        Debug.Log("ItemInstanceManager data loaded successfully.");
    }

    public void TestSaveAndLoadItemInstanceManagerData()
    {
        Debug.Log("TestSaveAndLoadItemInstanceManagerData called.");

        if (ItemInstanceManager.Instance == null)
        {
            Debug.LogError("ItemInstanceManager.Instance is null. Cannot test save and load.");
            return;
        }

        // 创建测试数据
        ItemInstanceManager.Instance.itemInstanceLists = new Dictionary<ItemInstanceManager.ItemInstanceType, List<ItemInstanceManager.ItemInstance>>
        {
            [ItemInstanceManager.ItemInstanceType.ToolInstance] = new List<ItemInstanceManager.ItemInstance>
            {
                new ItemInstanceManager.ToolInstance { id = 1, type = ItemInstanceManager.ItemInstanceType.ToolInstance, item_id = 1001, position = new Vector3Int(0, 0, 0), durability = 75 }
            },
            [ItemInstanceManager.ItemInstanceType.MaterialInstance] = new List<ItemInstanceManager.ItemInstance>
            {
                new ItemInstanceManager.MaterialInstance { id = 2, type = ItemInstanceManager.ItemInstanceType.MaterialInstance, item_id = 2001, position = new Vector3Int(1, 0, 0), amount = 10 }
            },
            [ItemInstanceManager.ItemInstanceType.CropInstance] = new List<ItemInstanceManager.ItemInstance>
            {
                new ItemInstanceManager.CropInstance { id = 3, type = ItemInstanceManager.ItemInstanceType.CropInstance, item_id = 3001, position = new Vector3Int(0, 1, 0), growth = 0.5f, real_lifetime = 5.0f, growth_per_frame = 0.1f }
            },
            [ItemInstanceManager.ItemInstanceType.BuildingInstance] = new List<ItemInstanceManager.ItemInstance>
            {
                new ItemInstanceManager.BuildingInstance { id = 4, type = ItemInstanceManager.ItemInstanceType.BuildingInstance, item_id = 4001, position = new Vector3Int(1, 1, 0), durability = 100, material_list = new List<KeyValuePair<int, int>> { new KeyValuePair<int, int>(2001, 5) } }
            },
            [ItemInstanceManager.ItemInstanceType.PrintInstance] = new List<ItemInstanceManager.ItemInstance>
            {
                new ItemInstanceManager.PrintInstance
                {
                    id = 5,
                    type = ItemInstanceManager.ItemInstanceType.PrintInstance,
                    item_id = 5001,
                    position = new Vector3Int(0, 2, 0),
                    material_list = new List<KeyValuePair<int, ItemInstanceManager.PrintInstance.Progress>>
                    {
                        new KeyValuePair<int, ItemInstanceManager.PrintInstance.Progress>(2001, new ItemInstanceManager.PrintInstance.Progress { current = 2, need = 5 })
                    }
                }
            }
        };

        // 保存数据
        SaveItemInstanceManager();

        // 清空现有数据
        ItemInstanceManager.Instance.itemInstanceLists.Clear();

        // 加载数据
        LoadItemInstanceManager();

        // 验证加载的数据
        foreach (var kvp in ItemInstanceManager.Instance.itemInstanceLists)
        {
            foreach (var itemInstance in kvp.Value)
            {
                Debug.Log($"ItemInstance ID: {itemInstance.id}, Type: {itemInstance.GetType().Name}, Position: {itemInstance.position}");
            }
        }
    }
    #endregion

    #region "CropManagerData"
    [System.Serializable]
    public class CropManagerData
    {
        public List<CropData> crops;
        public List<PestDisasterData> pestDisasterEnvFactors;
        public float globalBuffChangeRate;
        public List<GrowthPerFrameData> growthPerFrames;

        [System.Serializable]
        public class CropData
        {
            public int id;
            public string name;
            public float lifetime;
            public float best_fertility;
            public float best_humidity;
            public float best_light;
            public int seed_id;
            // 可扩展字段
        }

        [System.Serializable]
        public class PestDisasterData
        {
            public int crop_id;
            public float change_rate;
        }

        [System.Serializable]
        public class GrowthPerFrameData
        {
            public int crop_id;
            public float growth_per_frame;
        }
    }

    private string cropManagerSavePath => Path.Combine(Application.persistentDataPath, "CropManagerData.json");

    public void SaveCropManager()
    {
        if (CropManager.Instance == null)
        {
            Debug.LogError("CropManager.Instance is null. Cannot save data.");
            return;
        }

        CropManagerData data = new CropManagerData
        {
            crops = new List<CropManagerData.CropData>(),
            pestDisasterEnvFactors = new List<CropManagerData.PestDisasterData>(),
            growthPerFrames = new List<CropManagerData.GrowthPerFrameData>(),
            globalBuffChangeRate = CropManager.Instance.globalBuffEnvFactor != null ? CropManager.Instance.globalBuffEnvFactor.change_rate : 1.0f
        };

        // 保存作物基础数据
        foreach (var crop in CropManager.Instance.cropList)
        {
            var d = new CropManagerData.CropData
            {
                id = crop.id,
                name = crop.name,
                lifetime = crop.lifetime,
                best_fertility = crop.best_fertility,
                best_humidity = crop.best_humidity,
                best_light = crop.best_light,
                seed_id = crop.seed_id
            };
            data.crops.Add(d);
        }

        // 保存 pestDisasterEnvFactorDict
        foreach (var kvp in CropManager.Instance.pestDisasterEnvFactorDict)
        {
            if (kvp.Value != null)
            {
                data.pestDisasterEnvFactors.Add(new CropManagerData.PestDisasterData
                {
                    crop_id = kvp.Key,
                    change_rate = kvp.Value.change_rate
                });
            }
        }

        // 保存 growthPerFrameDict
        foreach (var kvp in CropManager.Instance.growthPerFrameDict)
        {
            data.growthPerFrames.Add(new CropManagerData.GrowthPerFrameData
            {
                crop_id = kvp.Key,
                growth_per_frame = kvp.Value
            });
        }

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(cropManagerSavePath, json);
        Debug.Log($"CropManager data saved to {cropManagerSavePath}");
    }

    public void LoadCropManager()
    {
        if (!File.Exists(cropManagerSavePath))
        {
            Debug.LogWarning("CropManager save file not found!");
            return;
        }

        if (CropManager.Instance == null)
        {
            Debug.LogError("CropManager.Instance is null. Cannot load data.");
            return;
        }

        string json = File.ReadAllText(cropManagerSavePath);
        CropManagerData data = JsonConvert.DeserializeObject<CropManagerData>(json);

        CropManager.Instance.cropList.Clear();
        CropManager.Instance.cropDict.Clear();
        CropManager.Instance.SeedIdDict.Clear();
        CropManager.Instance.pestDisasterEnvFactorDict.Clear();
        CropManager.Instance.growthPerFrameDict.Clear();

        // 恢复作物基础数据
        foreach (var d in data.crops)
        {
            Crop crop = ScriptableObject.CreateInstance<Crop>();
            crop.id = d.id;
            crop.name = d.name;
            crop.lifetime = d.lifetime;
            crop.best_fertility = d.best_fertility;
            crop.best_humidity = d.best_humidity;
            crop.best_light = d.best_light;
            crop.seed_id = d.seed_id;

            CropManager.Instance.cropList.Add(crop);
            CropManager.Instance.cropDict[crop.id] = crop;
            CropManager.Instance.SeedIdDict[crop.id] = crop.seed_id;
        }

        // 恢复 pestDisasterEnvFactorDict
        foreach (var pd in data.pestDisasterEnvFactors)
        {
            var factor = ScriptableObject.CreateInstance<LinearFactor>();
            factor.change_rate = pd.change_rate;
            CropManager.Instance.pestDisasterEnvFactorDict[pd.crop_id] = factor;
        }

        // 恢复 globalBuffEnvFactor
        if (CropManager.Instance.globalBuffEnvFactor == null)
            CropManager.Instance.globalBuffEnvFactor = ScriptableObject.CreateInstance<LinearFactor>();
        CropManager.Instance.globalBuffEnvFactor.change_rate = data.globalBuffChangeRate;

        // 恢复 growthPerFrameDict
        foreach (var gpf in data.growthPerFrames)
        {
            CropManager.Instance.growthPerFrameDict[gpf.crop_id] = gpf.growth_per_frame;
        }

        Debug.Log("CropManager data loaded successfully.");
    }
    #endregion

    #region "TimeManagerData"
    /*
    可以使用的前提是把TimeManager的私有字段暴露出来。
    public static TimeManager Instance { get; set; }   //单例模式，确保只有一个timemanager
    public float realityTime { get; set; } = 0f; // 现实时间
    public float gameTime { get; set; } = 0f; // 游戏内时间
    这样修改这几行代码就可以了，否则不建议启用
    */
    [System.Serializable]
    public class TimeManagerData
    {
        public float realityTime;
        public float gameTime;
        public float timeScale;
        public int currentDay;
        public int currentSeason;
    }

    private string timeManagerSavePath => Path.Combine(Application.persistentDataPath, "TimeManagerData.json");

    public void SaveTimeManager()
    {
        if (TimeManager.Instance == null)
        {
            Debug.LogError("TimeManager.Instance is null. Cannot save data.");
            return;
        }

        TimeManagerData data = new TimeManagerData
        {
            realityTime = TimeManager.Instance.realityTime,
            gameTime = TimeManager.Instance.gameTime,
            timeScale = TimeManager.Instance.timeScale,
            currentDay = TimeManager.Instance.GetCurrentDay(),
            currentSeason = (int)TimeManager.Instance.GetCurrentSeason()
        };

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(timeManagerSavePath, json);
        Debug.Log($"TimeManager data saved to {timeManagerSavePath}");
    }

    public void LoadTimeManager()
    {
        if (!File.Exists(timeManagerSavePath))
        {
            Debug.LogWarning("TimeManager save file not found!");
            return;
        }

        if (TimeManager.Instance == null)
        {
            Debug.LogError("TimeManager.Instance is null. Cannot load data.");
            return;
        }

        string json = File.ReadAllText(timeManagerSavePath);
        TimeManagerData data = JsonConvert.DeserializeObject<TimeManagerData>(json);

        // 反射或暴露接口设置私有字段
        TimeManager.Instance.realityTime = data.realityTime;
        TimeManager.Instance.gameTime = data.gameTime;
        TimeManager.Instance.timeScale = data.timeScale;
        TimeManager.Instance.currentDay = data.currentDay;
        TimeManager.Instance.currentSeason = (TimeManager.Seasons)data.currentSeason;
        Debug.Log("TimeManager data loaded successfully.");
    }
    #endregion

    #region "TraderManagerData"
    [System.Serializable]
    public class TraderManagerData
    {
        public bool isTraderActive;
        public float currentSpawnProbability;
        public int balance;
        public List<TraderGoodsData> goods;
        public List<ItemHistoryData> itemHistory; // 新增

        [System.Serializable]
        public class TraderGoodsData
        {
            public int item_id;
            public int amount;
        }

        [System.Serializable]
        public class ItemHistoryData
        {
            public int item_id;
            public int count;
        }
    }

    private string traderManagerSavePath => Path.Combine(Application.persistentDataPath, "TraderManagerData.json");

    public void SaveTraderManager()
    {
        if (TraderManager.Instance == null || TraderManager.Instance.trader == null)
        {
            Debug.LogError("TraderManager.Instance or trader is null. Cannot save data.");
            return;
        }

        TraderManagerData data = new TraderManagerData
        {
            isTraderActive = TraderManager.Instance.isTraderActive,
            currentSpawnProbability = TraderManager.Instance.currentSpawnProbability,
            balance = TraderManager.Instance.balance,
            goods = new List<TraderManagerData.TraderGoodsData>(),
            itemHistory = new List<TraderManagerData.ItemHistoryData>() // 新增
        };

        foreach (var pair in TraderManager.Instance.trader.goods)
        {
            data.goods.Add(new TraderManagerData.TraderGoodsData
            {
                item_id = pair.Key.id,
                amount = pair.Value
            });
        }

        foreach (var pair in TraderManager.Instance.itemHistory)
        {
            data.itemHistory.Add(new TraderManagerData.ItemHistoryData
            {
                item_id = pair.Key.id,
                count = pair.Value
            });
        }

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(traderManagerSavePath, json);
        Debug.Log($"TraderManager data saved to {traderManagerSavePath}");
    }

    public void LoadTraderManager()
    {
        if (!File.Exists(traderManagerSavePath))
        {
            Debug.LogWarning("TraderManager save file not found!");
            return;
        }

        if (TraderManager.Instance == null || TraderManager.Instance.trader == null)
        {
            Debug.LogError("TraderManager.Instance or trader is null. Cannot load data.");
            return;
        }

        string json = File.ReadAllText(traderManagerSavePath);
        TraderManagerData data = JsonConvert.DeserializeObject<TraderManagerData>(json);

        TraderManager.Instance.isTraderActive = data.isTraderActive;
        TraderManager.Instance.currentSpawnProbability = data.currentSpawnProbability;
        TraderManager.Instance.balance = data.balance;

        TraderManager.Instance.trader.goods.Clear();
        foreach (var g in data.goods)
        {
            var item = ItemManager.Instance.GetItem(g.item_id); // 修改这里
            if (item != null)
                TraderManager.Instance.trader.goods.Add(new KeyValuePair<ItemManager.Item, int>(item, g.amount));
        }

        TraderManager.Instance.itemHistory.Clear();
        if (data.itemHistory != null)
        {
            foreach (var h in data.itemHistory)
            {
                var item = ItemManager.Instance.GetItem(h.item_id); // 修改这里
                if (item != null)
                    TraderManager.Instance.itemHistory[item] = h.count;
            }
        }

        Debug.Log("TraderManager data loaded successfully.");
    }

    public void TestSaveAndLoadTraderManagerData()
    {
        Debug.Log("TestSaveAndLoadTraderManagerData called.");

        if (TraderManager.Instance == null || TraderManager.Instance.trader == null)
        {
            Debug.LogError("TraderManager.Instance or trader is null. Cannot test save and load.");
            return;
        }

        // 修改一些数据用于测试
        TraderManager.Instance.isTraderActive = true;
        TraderManager.Instance.currentSpawnProbability = 0.5f;
        TraderManager.Instance.balance = 1234;
        TraderManager.Instance.trader.goods = new List<KeyValuePair<ItemManager.Item, int>>
        {
            new KeyValuePair<ItemManager.Item, int>(ItemManager.Instance.itemLists[ItemManager.ItemType.Material][0], 10),
            new KeyValuePair<ItemManager.Item, int>(ItemManager.Instance.itemLists[ItemManager.ItemType.Material][1], 20)
        };
        TraderManager.Instance.itemHistory.Clear();
        TraderManager.Instance.itemHistory[ItemManager.Instance.itemLists[ItemManager.ItemType.Material][0]] = 5;
        TraderManager.Instance.itemHistory[ItemManager.Instance.itemLists[ItemManager.ItemType.Material][1]] = 8;

        // 保存
        SaveTraderManager();

        // 清空
        TraderManager.Instance.isTraderActive = false;
        TraderManager.Instance.currentSpawnProbability = 0f;
        TraderManager.Instance.balance = 0;
        TraderManager.Instance.trader.goods.Clear();
        TraderManager.Instance.itemHistory.Clear();

        // 加载
        LoadTraderManager();

        // 验证
        Debug.Log($"isTraderActive: {TraderManager.Instance.isTraderActive}, currentSpawnProbability: {TraderManager.Instance.currentSpawnProbability}, balance: {TraderManager.Instance.balance}");
        foreach (var g in TraderManager.Instance.trader.goods)
        {
            Debug.Log($"Goods: {g.Key.name}, Amount: {g.Value}");
        }
        foreach (var h in TraderManager.Instance.itemHistory)
        {
            Debug.Log($"ItemHistory: {h.Key.name}, Count: {h.Value}");
        }
    }
    #endregion

    #region "EventManagerData"
    [System.Serializable]
    public class EventManagerData
    {
        public List<EventData> events = new List<EventData>();

        [System.Serializable]
        public class EventData
        {
            public string eventType; // EventType枚举
            public string name;
            public int arrival;
            public int end;
            public int predictability_level;

            // Weather
            public string weather_type;

            // PestDisaster
            public int aim_crop;
            public int damage_rate;
        }
    }

    private string eventManagerSavePath => Path.Combine(Application.persistentDataPath, "EventManagerData.json");

    public void SaveEventManager()
    {
        if (EventManager.Instance == null)
        {
            Debug.LogError("EventManager.Instance is null. Cannot save data.");
            return;
        }

        var data = new EventManagerData();

        // 反射或类型判断，序列化所有事件
        var eventListField = typeof(EventManager).GetField("eventList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var eventList = eventListField.GetValue(EventManager.Instance) as List<EventManager.Event>;
        foreach (var e in eventList)
        {
            var ed = new EventManagerData.EventData
            {
                eventType = e.type.ToString(),
                name = e.name,
                arrival = e.arrival,
                end = e.end,
                predictability_level = e.predictability_level
            };
            if (e is EventManager.Weather w)
            {
                ed.weather_type = w.weather_type.ToString();
            }
            else if (e is EventManager.PestDisaster p)
            {
                ed.aim_crop = p.aim_crop;
                ed.damage_rate = p.damage_rate;
            }
            data.events.Add(ed);
        }

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(eventManagerSavePath, json);
        Debug.Log($"EventManager data saved to {eventManagerSavePath}");
    }

    public void LoadEventManager()
    {
        if (!File.Exists(eventManagerSavePath))
        {
            Debug.LogWarning("EventManager save file not found!");
            return;
        }
        if (EventManager.Instance == null)
        {
            Debug.LogError("EventManager.Instance is null. Cannot load data.");
            return;
        }

        string json = File.ReadAllText(eventManagerSavePath);
        var data = JsonConvert.DeserializeObject<EventManagerData>(json);

        var eventListField = typeof(EventManager).GetField("eventList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var eventList = eventListField.GetValue(EventManager.Instance) as List<EventManager.Event>;
        eventList.Clear();

        foreach (var ed in data.events)
        {
            EventManager.Event e = null;
            var typeEnum = (EventManager.EventType)System.Enum.Parse(typeof(EventManager.EventType), ed.eventType);
            switch (typeEnum)
            {
                case EventManager.EventType.Weather:
                    var w = new EventManager.Weather
                    {
                        type = typeEnum,
                        name = ed.name,
                        arrival = ed.arrival,
                        end = ed.end,
                        predictability_level = ed.predictability_level,
                        weather_type = (EventManager.Weather.WeatherType)System.Enum.Parse(typeof(EventManager.Weather.WeatherType), ed.weather_type)
                    };
                    e = w;
                    break;
                case EventManager.EventType.PestDisaster:
                    var p = new EventManager.PestDisaster
                    {
                        type = typeEnum,
                        name = ed.name,
                        arrival = ed.arrival,
                        end = ed.end,
                        predictability_level = ed.predictability_level,
                        aim_crop = ed.aim_crop,
                        damage_rate = ed.damage_rate
                    };
                    e = p;
                    break;
                default:
                    e = new EventManager.Event
                    {
                        type = typeEnum,
                        name = ed.name,
                        arrival = ed.arrival,
                        end = ed.end,
                        predictability_level = ed.predictability_level
                    };
                    break;
            }
            eventList.Add(e);
        }
        Debug.Log("EventManager data loaded successfully.");
    }

    public void TestSaveAndLoadEventManagerData()
    {
        Debug.Log("TestSaveAndLoadEventManagerData called.");

        if (EventManager.Instance == null)
        {
            Debug.LogError("EventManager.Instance is null. Cannot test save and load.");
            return;
        }

        // 构造测试数据
        var eventListField = typeof(EventManager).GetField("eventList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var eventList = eventListField.GetValue(EventManager.Instance) as List<EventManager.Event>;
        eventList.Clear();

        var weather = new EventManager.Weather
        {
            type = EventManager.EventType.Weather,
            name = "Sunny Day",
            arrival = 1,
            end = 2,
            predictability_level = 1,
            weather_type = EventManager.Weather.WeatherType.Sunny
        };
        var pest = new EventManager.PestDisaster
        {
            type = EventManager.EventType.PestDisaster,
            name = "Locust Attack",
            arrival = 3,
            end = 4,
            predictability_level = 2,
            aim_crop = 101,
            damage_rate = 30
        };
        eventList.Add(weather);
        eventList.Add(pest);

        // 保存
        SaveEventManager();

        // 清空
        eventList.Clear();

        // 加载
        LoadEventManager();

        // 验证
        foreach (var e in eventList)
        {
            Debug.Log($"Event: {e.name}, Type: {e.type}, Arrival: {e.arrival}, End: {e.end}, Predictability: {e.predictability_level}");
            if (e is EventManager.Weather w)
                Debug.Log($"WeatherType: {w.weather_type}");
            if (e is EventManager.PestDisaster p)
                Debug.Log($"AimCrop: {p.aim_crop}, DamageRate: {p.damage_rate}");
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
        //LoadItemManager();
        //StartCoroutine(DelayedLoad());
        
    }
    IEnumerator DelayedLoad()
    {
        // 等待所有依赖的Manager单例初始化
        while (
            MapManager.Instance == null ||
            TaskManager.Instance == null ||
            PawnManager.Instance == null ||
            //ItemManager.Instance == null ||
            CropManager.Instance == null ||
            TimeManager.Instance == null ||
            TraderManager.Instance == null ||
            ItemInstanceManager.Instance == null
        )
        {
            Debug.LogWarning("Waiting for all Managers to initialize...");
            yield return null;
        }
        /*
        // 依次加载数据，顺序建议如下（先基础数据，后依赖数据）
        //LoadItemManager();
        LoadCropManager();
        //LoadMapManager();
        LoadTaskManager();
        LoadPawnManager();
        LoadItemInstanceManager();
        LoadTimeManager();
        LoadTraderManager();
        */
        Debug.Log("All managers loaded successfully.");
    }
    void OnDestroy()
    {
        // 在销毁时保存数据
        //SaveItemManager();
        /*
        SaveMapManager();
        SaveTaskManager();
        SavePawnManager();
        SaveItemInstanceManager();
        SaveCropManager();
        SaveTimeManager();
        SaveTraderManager();
        */
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            //SaveItemManager();            
            SaveMapManager();
            SaveTaskManager();
            SavePawnManager();
            SaveItemInstanceManager();
            SaveCropManager();
            SaveTimeManager();
            SaveTraderManager();
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            //LoadItemManager();            
            LoadMapManager();
            LoadTaskManager();
            LoadPawnManager();
            LoadItemInstanceManager();
            LoadCropManager();
            LoadTimeManager();
            LoadTraderManager();
        }
    }
    
}
