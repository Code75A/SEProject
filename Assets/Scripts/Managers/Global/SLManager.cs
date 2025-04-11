// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class SLManager : MonoBehaviour
// {
//     public static SLManager Instance;

//     [System.Serializable]
//     class gameData
//     {
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
//         public Vector2 position;
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
//         public List<ItemData> items = new List<ItemData>();

//         [System.Serializable]
//         public class ItemData
//         {
//             public int id;
//             public string name;
//             public string type;
//             public int maxDurability; // 如果是工具
//             public Dictionary<string, int> enhancements; // 工具强化属性
//         }
//     }

//     [System.Serializable]
//     class MapManagerData
//     {
//         public List<MapData> mapDatas = new List<MapData>();

//         [System.Serializable]
//         public class MapData
//         {
//             public Vector3Int position;
//             public string type;
//             public bool hasItem;
//             public int? itemId;
//             public int? itemAmount;
//             public bool hasBuilding;
//             public bool canWalk;
//             public bool canBuild;
//             public bool canPlant;
//             public float fertility;
//             public float humidity;
//             public float light;
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

//     public void Save()
//     {
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

//     public void Load()
//     {
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
//                 position = pawn.position,
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

//     public void SaveManagersData()
//     {
//         // 保存 ItemManager 数据
//         ItemManagerData itemManagerData = new ItemManagerData();
//         foreach (var itemList in ItemManager.Instance.itemLists)
//         {
//             foreach (var item in itemList.Value)
//             {
//                 var itemData = new ItemManagerData.ItemData
//                 {
//                     id = item.id,
//                     name = item.name,
//                     type = item.type.ToString(),
//                 };

//                 if (item is ItemManager.Tool tool)
//                 {
//                     itemData.maxDurability = tool.max_durability;
//                     itemData.enhancements = new Dictionary<string, int>();
//                     foreach (var enhancement in tool.enhancements)
//                     {
//                         itemData.enhancements.Add(enhancement.Key.ToString(), enhancement.Value);
//                     }
//                 }

//                 itemManagerData.items.Add(itemData);
//             }
//         }

//         // 保存 MapManager 数据
//         MapManagerData mapManagerData = new MapManagerData();
//         if (MapManager.Instance.mapDatas != null)
//         {
//             // 假设mapDatas是Dictionary或其他集合类型
//             foreach (var data in MapManager.Instance.mapDatas.Values)
//             {
//                 var mapDataEntry = new MapManagerData.MapData
//                 {
//                     position = data.position,
//                     type = data.type.ToString(),
//                     hasItem = data.has_item,
//                     itemId = data.item?.id,
//                     itemAmount = data.item is ItemInstanceManager.MaterialInstance material ? material.amount : (int?)null,
//                     hasBuilding = data.has_building,
//                     canWalk = data.can_walk,
//                     canBuild = data.can_build,
//                     canPlant = data.can_plant,
//                     fertility = data.fertility,
//                     humidity = data.humidity,
//                     light = data.light,
//                 };
//                 mapManagerData.mapDatas.Add(mapDataEntry);
//             }
//         }

//         // 保存 TaskManager 数据
//         TaskManagerData taskManagerData = new TaskManagerData();
//         foreach (var task in TaskManager.Instance.availableTaskList)
//         {
//             taskManagerData.availableTasks.Add(new TaskManagerData.TaskData
//             {
//                 id = task.id,
//                 position = task.position,
//                 type = task.type.ToString(),
//             });
//         }
//         foreach (var task in TaskManager.Instance.inavailableTaskList)
//         {
//             taskManagerData.inavailableTasks.Add(new TaskManagerData.TaskData
//             {
//                 id = task.id,
//                 position = task.position,
//                 type = task.type.ToString(),
//             });
//         }

//         // 保存 TimeManager 数据
//         TimeManagerData timeManagerData = new TimeManagerData
//         {
//             realityTime = TimeManager.Instance.realityTime,
//             gameTime = TimeManager.Instance.gameTime,
//             timeScale = TimeManager.Instance.timeScale,
//         };

//         // 序列化并存储
//         string itemJson = JsonUtility.ToJson(itemManagerData, true);
//         PlayerPrefs.SetString("ItemManagerData", itemJson);

//         string mapJson = JsonUtility.ToJson(mapManagerData, true);
//         PlayerPrefs.SetString("MapManagerData", mapJson);

//         string taskJson = JsonUtility.ToJson(taskManagerData, true);
//         PlayerPrefs.SetString("TaskManagerData", taskJson);

//         string timeJson = JsonUtility.ToJson(timeManagerData, true);
//         PlayerPrefs.SetString("TimeManagerData", timeJson);

//         Debug.Log("所有管理器数据保存成功！");
//     }
// }
