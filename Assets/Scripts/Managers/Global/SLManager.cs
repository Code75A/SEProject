// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System;
// using System.Linq; // 添加 LINQ 的命名空间

// public class SLManager : MonoBehaviour
// {
//     public static SLManager Instance;

//     [System.Serializable]
//     class gameData
//     {
//         //目前来看这个gameData类是多余的，因为要存储的数据太多了，因此拆分为不同的类进行存储
//         //但是我还是保存这个类为了避免后续有需要存储的东西
//         //如果有需要存储的东西可以直接在这个类里添加
//         public ItemManager itemManager; // 存储 ItemManager 的数据
//         public MapManager mapManager; // 存储 MapManager 的数据
//         public PawnManager pawnManager; // 存储 PawnManager 的数据
//         public TaskManager taskManager; // 存储 TaskManager 的数据
//         public TimeManager timeManager; // 存储 TimeManager 的数据
//     }

//     [System.Serializable]
//     class PawnData
//     {
//         public int id;
//         public bool isOnTask;
//         public int? handlingTaskId; // 当前任务的 ID
//         public float moveSpeed;
//         public float workSpeed;
//         public int capacity; // 运载容量
//         public Vector3Int position;        
//         public int? handlingToolId; // 当前工具的 ID
//         public List<int> taskIds; // 任务 ID 列表
//     }

//     [System.Serializable]
//     class PawnManagerData
//     {
//         public List<PawnData> pawns = new List<PawnData>();
//         public int? selectingPawnId; // 当前选中的 Pawn 的 ID
//     }

//     [System.Serializable]
//     class ItemManagerData
//     {
//         public List<ToolData> tools = new List<ToolData>();
//         public List<MaterialData> materials = new List<MaterialData>();

//         [System.Serializable]
//         public class ToolData
//         {
//             public int id;
//             public string name;
//             public int maxDurability;
//             public List<KeyValuePair<string, int>> enhancements; // 替换 Dictionary 为 List<KeyValuePair>
//         }

//         [System.Serializable]
//         public class MaterialData
//         {
//             public int id;
//             public string name;
//         }
//     }

//     [System.Serializable]
//     class MapManagerData
//     {
//         public List<MapDataEntry> mapDataEntries = new List<MapDataEntry>();

//         [System.Serializable]
//         public class MapDataEntry
//         {
//             public Vector3Int position;
//             public string type;
//             public bool hasItem;
//             public int? itemId;
//             public int? itemAmount;
//             public bool hasBuilding;
//             public bool hasPrint;
//             public bool canWalk;
//             public bool canBuild;
//             public bool canPlant;
//             public float fertility;
//             public float humidity;
//             public float light;
//             public float walkSpeed;
//             public bool hasPawn;
//         }
        
//     }

//     [System.Serializable]
//     class TaskManagerData
//     {
//         public List<TaskData> availableTasks = new List<TaskData>();
//         public List<TaskData> inavailableTasks = new List<TaskData>();

//         [System.Serializable]
//         public class TaskData
//         {
//             public int id;
//             public Vector3Int position;
//             public string type;
//             public int materialId;
//             public int materialAmount;
//         }
//     }

//     [System.Serializable]
//     class TimeManagerData
//     {
//         public float realityTime;
//         public float gameTime;
//         public float timeScale;
//     }

//     void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     public void Save_useless()
//     {
//         // 这个函数是多余的，保留以防后续需要
//         //目前的存储函数是Save()
//         gameData data = new gameData();
//         data.itemManager = ItemManager.Instance; // 将 ItemManager 的数据存入 gameData
//         data.mapManager = MapManager.Instance; // 将 MapManager 的数据存入 gameData
//         data.pawnManager = PawnManager.Instance; // 将 PawnManager 的数据存入 gameData
//         data.taskManager = TaskManager.Instance; // 将 TaskManager 的数据存入 gameData
//         data.timeManager = TimeManager.Instance; // 将 TimeManager 的数据存入 gameData

//         string json = JsonUtility.ToJson(data, true); // 序列化为 JSON
//         PlayerPrefs.SetString("SaveData", json); // 存储到 PlayerPrefs
//         Debug.Log("保存成功: " + json);
//     }

//     public void Load_useless()
//     {
//         // 这个函数是多余的，保留以防后续需要
//         //目前的加载函数是Load()
//         string json = PlayerPrefs.GetString("SaveData");
//         if (string.IsNullOrEmpty(json))
//         {
//             Debug.LogWarning("没有找到保存的数据！");
//             return;
//         }

//         gameData data = JsonUtility.FromJson<gameData>(json); // 反序列化 JSON
//         if (data == null)
//         {
//             Debug.LogWarning("加载失败，数据为空！");
//             return;
//         }

//         JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data.itemManager), ItemManager.Instance); // 将数据覆盖到 ItemManager
//         JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data.mapManager), MapManager.Instance); // 将数据覆盖到 MapManager
//         JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data.pawnManager), PawnManager.Instance); // 将数据覆盖到 PawnManager
//         JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data.taskManager), TaskManager.Instance); // 将数据覆盖到 TaskManager
//         JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data.timeManager), TimeManager.Instance); // 将数据覆盖到 TimeManager

//         Debug.Log("加载成功: " + json);
//     }

//     public void SavePawnManagerData()
//     {
//         PawnManagerData pawnManagerData = new PawnManagerData();

//         // 保存所有 Pawn 的数据
//         foreach (var pawn in PawnManager.Instance.pawns)
//         {
//             PawnData pawnData = new PawnData
//             {
//                 id = pawn.id,
//                 isOnTask = pawn.isOnTask,
//                 handlingTaskId = pawn.handlingTask?.id,
//                 moveSpeed = pawn.moveSpeed,
//                 workSpeed = pawn.workSpeed,
//                 capacity = pawn.capacity,
//                 //position = pawn.position,
//                 handlingToolId = pawn.handlingTool?.id,
//                 taskIds = pawn.PawntaskList.ConvertAll(task => task.id)
//             };
//             pawnManagerData.pawns.Add(pawnData);
//         }

//         // 保存当前选中的 Pawn 的 ID
//         if (PawnManager.Instance.SelectingPawn != null)
//         {
//             pawnManagerData.selectingPawnId = PawnManager.Instance.SelectingPawn.id;
//         }

//         // 序列化为 JSON 并存储
//         string json = JsonUtility.ToJson(pawnManagerData, true);
//         PlayerPrefs.SetString("PawnManagerData", json);
//         Debug.Log("PawnManager 数据保存成功: " + json);
//     }

//     public void SaveItemManagerData()
//     {
//         ItemManagerData itemManagerData = new ItemManagerData();

//         // 保存 Tool 数据
//         foreach (var tool in ItemManager.Instance.itemLists[ItemManager.ItemType.Tool])
//         {
//             if (tool is ItemManager.Tool toolItem)
//             {
//                 var toolData = new ItemManagerData.ToolData
//                 {
//                     id = toolItem.id,
//                     name = toolItem.name,
//                     maxDurability = toolItem.max_durability,
//                     enhancements = new List<KeyValuePair<string, int>>()
//                 };

//                 // 将 Dictionary 转换为 List<KeyValuePair>
//                 foreach (var enhancement in toolItem.enhancements)
//                 {
//                     toolData.enhancements.Add(new KeyValuePair<string, int>(enhancement.Key.ToString(), enhancement.Value));
//                 }

//                 itemManagerData.tools.Add(toolData);
//             }
//         }

//         // 保存 Material 数据
//         foreach (var material in ItemManager.Instance.itemLists[ItemManager.ItemType.Material])
//         {
//             if (material is ItemManager.Material materialItem)
//             {
//                 var materialData = new ItemManagerData.MaterialData
//                 {
//                     id = materialItem.id,
//                     name = materialItem.name
//                 };

//                 itemManagerData.materials.Add(materialData);
//             }
//         }

//         // 序列化为 JSON 并存储
//         string json = JsonUtility.ToJson(itemManagerData, true);
//         PlayerPrefs.SetString("ItemManagerData", json);
//         Debug.Log("ItemManager 数据保存成功: " + json);
//     }

//     public void SaveMapManagerData()
//     {
//         MapManagerData mapManagerData = new MapManagerData();

//         // 遍历 MapManager 的 mapDatas 并保存每个格子的信息
//         for (int x = 0; x < MapManager.MAP_SIZE; x++)
//         {
//             for (int y = 0; y < MapManager.MAP_SIZE; y++)
//             {
//                 var mapData = MapManager.Instance.mapDatas[x, y];
//                 if (mapData != null)
//                 {
//                     var mapDataEntry = new MapManagerData.MapDataEntry
//                     {
//                         position = mapData.position,
//                         type = mapData.type.ToString(),
//                         hasItem = mapData.has_item,
//                         itemId = mapData.item?.id,
//                         itemAmount = mapData.item is ItemInstanceManager.MaterialInstance material ? material.amount : (int?)null,
//                         hasBuilding = mapData.has_building,
//                         hasPrint = mapData.has_print,
//                         canWalk = mapData.can_walk,
//                         canBuild = mapData.can_build,
//                         canPlant = mapData.can_plant,
//                         fertility = mapData.fertility,
//                         humidity = mapData.humidity,
//                         light = mapData.light,
//                         walkSpeed = mapData.walk_speed,
//                         hasPawn = mapData.has_pawn
//                     };
//                     mapManagerData.mapDataEntries.Add(mapDataEntry);
//                 }
//             }
//         }

//         string json = JsonUtility.ToJson(mapManagerData, true);
//         PlayerPrefs.SetString("MapManagerData", json);
//         Debug.Log("MapManager 数据保存成功: " + json);
//     }

//     public void SaveTaskManagerData()
//     {
//         TaskManagerData taskManagerData = new TaskManagerData();

//         foreach (var task in TaskManager.Instance.availableTaskList)
//         {
//             taskManagerData.availableTasks.Add(new TaskManagerData.TaskData
//             {
//                 id = task.id,
//                 position = task.position,
//                 type = task.type.ToString(),
//                 materialId = task.MaterialId,
//                 materialAmount = task.MaterialAmount
//             });
//         }

//         foreach (var task in TaskManager.Instance.inavailableTaskList)
//         {
//             taskManagerData.inavailableTasks.Add(new TaskManagerData.TaskData
//             {
//                 id = task.id,
//                 position = task.position,
//                 type = task.type.ToString(),
//                 materialId = task.MaterialId,
//                 materialAmount = task.MaterialAmount
//             });
//         }

//         string json = JsonUtility.ToJson(taskManagerData, true);
//         PlayerPrefs.SetString("TaskManagerData", json);
//         Debug.Log("TaskManager 数据保存成功: " + json);
//     }

//     public void SaveTimeManagerData()
//     {
//         TimeManagerData timeManagerData = new TimeManagerData
//         {
//             realityTime = TimeManager.Instance.realityTime,
//             gameTime = TimeManager.Instance.gameTime,
//             timeScale = TimeManager.Instance.timeScale,
//         };

//         string json = JsonUtility.ToJson(timeManagerData, true);
//         PlayerPrefs.SetString("TimeManagerData", json);
//         Debug.Log("TimeManager 数据保存成功: " + json);
//     }

//     public void SaveManagersData()
//     {
//         // 保存 PawnManager 数据
//         SavePawnManagerData();

//         // 保存 ItemManager 数据
//         SaveItemManagerData();

//         // 保存 MapManager 数据
//         SaveMapManagerData();

//         // 保存 TaskManager 数据
//         SaveTaskManagerData();

//         // 保存 TimeManager 数据
//         SaveTimeManagerData();

//         Debug.Log("所有管理器数据保存成功！");
//     }

//     public void LoadItemManagerData()
//     {
//         string json = PlayerPrefs.GetString("ItemManagerData");
//         if (string.IsNullOrEmpty(json))
//         {
//             Debug.LogWarning("没有找到 ItemManager 的保存数据！");
//             return;
//         }

//         ItemManagerData data = JsonUtility.FromJson<ItemManagerData>(json);

//         // 清空当前 ItemManager 的数据
//         ItemManager.Instance.itemLists[ItemManager.ItemType.Tool].Clear();
//         ItemManager.Instance.itemLists[ItemManager.ItemType.Material].Clear();

//         // 加载 Tool 数据
//         foreach (var toolData in data.tools)
//         {
//             var tool = new ItemManager.Tool
//             {
//                 id = toolData.id,
//                 name = toolData.name,
//                 max_durability = toolData.maxDurability,
//                 enhancements = new Dictionary<ItemManager.Tool.EnhanceType, int>()
//             };

//             // 将 List<KeyValuePair> 转换回 Dictionary
//             foreach (var enhancement in toolData.enhancements)
//             {
//                 if (System.Enum.TryParse(enhancement.Key, out ItemManager.Tool.EnhanceType enhanceType))
//                 {
//                     tool.enhancements.Add(enhanceType, enhancement.Value);
//                 }
//             }

//             ItemManager.Instance.itemLists[ItemManager.ItemType.Tool].Add(tool);
//         }

//         // 加载 Material 数据
//         foreach (var materialData in data.materials)
//         {
//             var material = new ItemManager.Material
//             {
//                 id = materialData.id,
//                 name = materialData.name
//             };

//             ItemManager.Instance.itemLists[ItemManager.ItemType.Material].Add(material);
//         }

//         Debug.Log("ItemManager 数据加载成功: " + json);
//     }

//     public void LoadMapManagerData()
//     {
//         string json = PlayerPrefs.GetString("MapManagerData");
//         if (string.IsNullOrEmpty(json))
//         {
//             Debug.LogWarning("没有找到 MapManager 的保存数据！");
//             return;
//         }

//         MapManagerData data = JsonUtility.FromJson<MapManagerData>(json);
//         if (data == null || data.mapDataEntries == null)
//         {
//             Debug.LogWarning("加载失败，MapManager 数据为空！");
//             return;
//         }

//         // 清空当前 MapManager 的数据
//         MapManager.Instance.GenerateMapData();

//         // 加载保存的 MapData
//         foreach (var entry in data.mapDataEntries)
//         {
//             var mapData = MapManager.Instance.mapDatas[entry.position.x, entry.position.y];
//             if (mapData != null)
//             {
//                 mapData.type = Enum.TryParse(entry.type, out MapManager.tileTypes type) ? type : MapManager.tileTypes.grass;
//                 mapData.has_item = entry.hasItem;
//                 mapData.item = entry.itemId.HasValue
//                     ? ItemInstanceManager.Instance.SpawnItem(entry.position, entry.itemId.Value, ItemInstanceManager.ItemInstanceType.MaterialInstance, entry.itemAmount ?? 0)
//                     : null;
//                 mapData.has_building = entry.hasBuilding;
//                 mapData.has_print = entry.hasPrint;
//                 mapData.can_walk = entry.canWalk;
//                 mapData.can_build = entry.canBuild;
//                 mapData.can_plant = entry.canPlant;
//                 mapData.fertility = entry.fertility;
//                 mapData.humidity = entry.humidity;
//                 mapData.light = entry.light;
//                 mapData.walk_speed = entry.walkSpeed;
//                 mapData.has_pawn = entry.hasPawn;
//             }
//         }

//         // 更新地图瓦片
//         MapManager.Instance.GenerateMapTiles();

//         Debug.Log("MapManager 数据加载成功: " + json);
//     }

//     public void LoadTaskManagerData()
//     {
//         string json = PlayerPrefs.GetString("TaskManagerData");
//         if (string.IsNullOrEmpty(json))
//         {
//             Debug.LogWarning("没有找到 TaskManager 的保存数据！");
//             return;
//         }

//         TaskManagerData data = JsonUtility.FromJson<TaskManagerData>(json);
//         if (data == null)
//         {
//             Debug.LogWarning("加载失败，TaskManager 数据为空！");
//             return;
//         }

//         TaskManager.Instance.availableTaskList.Clear();
//         TaskManager.Instance.inavailableTaskList.Clear();

//         foreach (var taskData in data.availableTasks)
//         {
//             TaskManager.Instance.availableTaskList.Add(new TaskManager.Task(
//                 position: taskData.position,
//                 type: Enum.TryParse(taskData.type, out TaskManager.TaskTypes type) ? type : TaskManager.TaskTypes.Total,
//                 id: taskData.id,
//                 materialId: taskData.materialId,
//                 materialAmount: taskData.materialAmount,
//                 materialType: -1 // 默认为 -1
//             ));
//         }

//         foreach (var taskData in data.inavailableTasks)
//         {
//             TaskManager.Instance.inavailableTaskList.Add(new TaskManager.Task(
//                 position: taskData.position,
//                 type: Enum.TryParse(taskData.type, out TaskManager.TaskTypes type) ? type : TaskManager.TaskTypes.Total,
//                 id: taskData.id,
//                 materialId: taskData.materialId,
//                 materialAmount: taskData.materialAmount,
//                 materialType: -1 // 默认为 -1
//             ));
//         }

//         Debug.Log("TaskManager 数据加载成功: " + json);
//     }

//     public void LoadTimeManagerData()
//     {
//         string json = PlayerPrefs.GetString("TimeManagerData");
//         if (string.IsNullOrEmpty(json))
//         {
//             Debug.LogWarning("没有找到 TimeManager 的保存数据！");
//             return;
//         }

//         TimeManagerData data = JsonUtility.FromJson<TimeManagerData>(json);
//         //JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data), TimeManager.Instance);
//         if (data != null)
//         {
//             TimeManager.Instance.GetType().GetProperty("realityTime")?.SetValue(TimeManager.Instance, data.realityTime);
//             TimeManager.Instance.GetType().GetProperty("gameTime")?.SetValue(TimeManager.Instance, data.gameTime);
//             TimeManager.Instance.timeScale = data.timeScale;
//             Debug.Log("TimeManager 数据加载成功: " + json);
//         }
//         else
//         {
//             Debug.LogWarning("加载失败，TimeManager 数据为空！");
//         }
//         Debug.Log("TimeManager 数据加载成功: " + json);
//     }

//     public void LoadPawnManagerData()
//     {
//         string json = PlayerPrefs.GetString("PawnManagerData");
//         if (string.IsNullOrEmpty(json))
//         {
//             Debug.LogWarning("没有找到 PawnManager 的保存数据！");
//             return;
//         }

//         PawnManagerData data = JsonUtility.FromJson<PawnManagerData>(json);
//         if (data == null)
//         {
//             Debug.LogWarning("加载失败，PawnManager 数据为空！");
//             return;
//         }

//         // 清空当前 PawnManager 的数据
//         PawnManager.Instance.pawns.Clear();

//         // 加载 Pawn 数据
//         foreach (var pawnData in data.pawns)
//         {
//             // 使用 CreatePawn 方法创建 Pawn
//             PawnManager.Instance.CreatePawn(pawnData.position);
//             var pawn = PawnManager.Instance.pawns[^1]; // 获取刚刚创建的 Pawn

//             // 设置 Pawn 的属性
//             pawn.id = pawnData.id;
//             pawn.isOnTask = pawnData.isOnTask;

//             // 查找任务
//             pawn.handlingTask = TaskManager.Instance.availableTaskList
//                 .Concat(TaskManager.Instance.inavailableTaskList)
//                 .FirstOrDefault(task => task.id == pawnData.handlingTaskId);

//             pawn.moveSpeed = pawnData.moveSpeed;
//             pawn.workSpeed = pawnData.workSpeed;
//             pawn.capacity = pawnData.capacity;
//             pawn.handlingTool = pawnData.handlingToolId.HasValue 
//                 ? ItemManager.Instance.GetItem(pawnData.handlingToolId.Value, ItemManager.ItemType.Tool) as ItemManager.Tool 
//                 : null;
//             pawn.PawntaskList = pawnData.taskIds
//                 .Select(id => TaskManager.Instance.availableTaskList
//                     .Concat(TaskManager.Instance.inavailableTaskList)
//                     .FirstOrDefault(task => task.id == id))
//                 .Where(task => task != null)
//                 .ToList();
//         }

//         // 设置当前选中的 Pawn
//         if (data.selectingPawnId.HasValue)
//         {
//             PawnManager.Instance.SelectingPawn = PawnManager.Instance.pawns.FirstOrDefault(p => p.id == data.selectingPawnId.Value);
//         }
//         else
//         {
//             PawnManager.Instance.SelectingPawn = null;
//         }

//         Debug.Log("PawnManager 数据加载成功: " + json);
//     }

//     public void Save()
//     {
//         SaveItemManagerData();
//         SaveMapManagerData();
//         SaveTaskManagerData();
//         SaveTimeManagerData();
//         SavePawnManagerData();
//         Debug.Log("所有管理器数据保存完成！");
//     }

//     public void Load()
//     {
//         LoadItemManagerData();
//         LoadMapManagerData();
//         LoadTaskManagerData();
//         LoadTimeManagerData();
//         LoadPawnManagerData();
//         Debug.Log("所有管理器数据加载完成！");
//     }
// }
