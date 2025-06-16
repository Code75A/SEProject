
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;


public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }

    public Dictionary<BuildingType, List<Building>> buildingLists = new Dictionary<BuildingType, List<Building>>();
    public List<Building> currentBuildingList ;

    //TODO: REMOVE
    public Building currentBuilding = null;
    public GameObject currentBuilding_preview;
    public SpriteRenderer currentBuilding_preview_spriteRenderer;
    //REMOVE/
    

    const int tempBuildingSpritesCount = 7;
    public Sprite[] tempBuildingSprites = new Sprite[tempBuildingSpritesCount];
    public Sprite printSprite;

    //单例模式
    void Awake(){
        if(Instance == null)
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
        InitBuildingListsData();
        if(currentBuilding_preview!=null)
            currentBuilding_preview_spriteRenderer = currentBuilding_preview.GetComponent<SpriteRenderer>();
        else
            Debug.LogError("Error：请把Managers下的currentBuilding_preview拖到Stage-BuildManager脚本上！");
    }

    // void Update()
    // {
    //     if(Input.GetMouseButtonDown(1)){
    //         CancelCurrentBuilding();
    //     }
    // }

    void InitBuildingListsData()
    {
        for (int i = 0; i < (int)BuildingType.Total; i++)
            buildingLists.Add((BuildingType)i, new List<Building>());

        //TODO: 目前的material_list直接参考id，后续应该改为引用
        string[] guids = AssetDatabase.FindAssets("t:Building", new[] { "Assets/Resources/BuildingData" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Building building = AssetDatabase.LoadAssetAtPath<Building>(path);
            if (building != null)
            {
                if (!buildingLists.ContainsKey(building.type)){
                    Debug.LogWarning($"Building type {building.type} not found in buildingLists, adding it now.");
                    buildingLists[building.type] = new List<Building>();
                }
                buildingLists[building.type].Add(building);
            }
        }
    }

    public Building GetBuilding(int id){
        //TODO:CACHE
        if (id == TraderManager.TRADER_ID)
            return TraderManager.Instance.trader;
        
        foreach (var list in buildingLists.Values)
            {
                foreach (var building in list)
                {
                    if (building.id == id)
                        return building;
                }
            }
        return null;
    }

    public BuildingType GetBuildingType(int id){
        foreach(var list in buildingLists.Values){
            foreach(var building in list){
                if(building.id == id)
                    return building.type;
            }
        }
        return BuildingType.Total;
    }
    /// <summary>
    /// 将type类型的建筑列表加载到currentBuildingList，返回值该List传递回uiManager
    /// </summary>
    /// <param name="type">需加载的建筑类型</param>
    public List<Building> LoadBuildingList(BuildingType type){
        currentBuildingList = buildingLists[type];
        return currentBuildingList;
    }

    
}

#region"箱子"
// 货币类型Item，可以扩展为更多货币
public class CurrencyItem : ItemManager.Item
{
    public CurrencyItem()
    {
        this.id = 10000; // 假设货币id为10000，避免与其他物品冲突
        this.name = "金币";
        this.type = ItemManager.ItemType.Material; // 或者新加一个Currency类型
        this.texture = null; // 可指定货币图标
    }
}

public class ChestBuilding : Building
{
    // 箱子格子数量和每格最大堆叠数量
    public const int SlotCount = 27;
    public const int MaxStackPerSlot = 64;

    // 每个格子存储物品ID和数量
    [System.Serializable]
    public class ChestSlot
    {
        public int itemId = -1; // -1 表示空
        public int count = 0;
    }

    private List<ChestSlot> slots;

    public ChestBuilding(int id)
    {
        this.id = id; // 由外部传入
        this.build_name = "箱子";
        this.texture = null;
        this.type = BuildingType.Dev;
        this.width = 2;
        this.height = 2;
        this.durability = 10000;
        this.can_build = true;
        this.can_walk = false;
        this.can_plant = false;
        this.material_list = new List<KeyValuePair<int, int>>();
        // 初始化格子
        slots = new List<ChestSlot>();
        for (int i = 0; i < SlotCount; i++)
        {
            slots.Add(new ChestSlot());
        }
    }

    // 添加物品，返回实际添加数量
    public int AddItem(int itemId, int count)
    {
        int toAdd = count;
        // 先堆叠已有同类物品
        foreach (var slot in slots)
        {
            if (slot.itemId == itemId && slot.count < MaxStackPerSlot)
            {
                int canStack = MaxStackPerSlot - slot.count;
                int add = Mathf.Min(canStack, toAdd);
                slot.count += add;
                toAdd -= add;
                if (toAdd == 0) return count;
            }
        }
        // 再找空格
        foreach (var slot in slots)
        {
            if (slot.itemId == -1)
            {
                int add = Mathf.Min(MaxStackPerSlot, toAdd);
                slot.itemId = itemId;
                slot.count = add;
                toAdd -= add;
                if (toAdd == 0) return count;
            }
        }
        // 返回实际添加数量
        return count - toAdd;
    }

    // 移除物品，返回实际移除数量
    public int RemoveItem(int itemId, int count)
    {
        int toRemove = count;
        foreach (var slot in slots)
        {
            if (slot.itemId == itemId && slot.count > 0)
            {
                int remove = Mathf.Min(slot.count, toRemove);
                slot.count -= remove;
                toRemove -= remove;
                if (slot.count == 0) slot.itemId = -1;
                if (toRemove == 0) return count;
            }
        }
        // 返回实际移除数量
        return count - toRemove;
    }

    // 获取箱子所有物品（每格信息）
    public List<ChestSlot> GetAllSlots()
    {
        return slots;
    }

    // 获取某物品总数
    public int GetItemCount(int itemId)
    {
        int total = 0;
        foreach (var slot in slots)
        {
            if (slot.itemId == itemId)
                total += slot.count;
        }
        return total;
    }
}

#endregion

#region "奇观建筑"
// 疾风奇观：提升全体小人移动速度
public class GaleWonderBuilding : Building
{
    public static bool IsBuilt = false;
    public const float MoveSpeedBuff = 1.5f;
    public GaleWonderBuilding()
    {
        this.id = 101;
        this.build_name = "疾风奇观";
        this.texture = null; // 可指定专属Sprite
        this.type = BuildingType.Dev;
        this.width = 2;
        this.height = 2;
        this.durability = 99999;
        this.can_build = true;
        this.can_walk = false;
        this.can_plant = false;
        this.material_list = new List<KeyValuePair<int, int>>{
        };
    }
    public static void OnBuilt()
    {
        if (!IsBuilt)
        {
            IsBuilt = true;
            //PawnManager.ApplyGaleWonderBuff();
            Debug.Log("疾风奇观建成，全体小人移动速度提升！");
        }
    }
    public static void OnDestroyed()
    {
        if (IsBuilt)
        {
            IsBuilt = false;
            //PawnManager.RemoveGaleWonderBuff();
            Debug.Log("疾风奇观被摧毁，移动速度恢复正常。");
        }
    }
}

// 勤工奇观：提升全体小人工作速度
public class DiligenceWonderBuilding : Building
{
    public static bool IsBuilt = false;
    public const float WorkSpeedBuff = 1.5f;
    public DiligenceWonderBuilding()
    {
        this.id = 102;
        this.build_name = "勤工奇观";
        this.texture = null;
        this.type = BuildingType.Dev;
        this.width = 2;
        this.height = 2;
        this.durability = 99999;
        this.can_build = true;
        this.can_walk = false;
        this.can_plant = false;
        this.material_list = new List<KeyValuePair<int, int>>{
        };
    }
    public static void OnBuilt()
    {
        if (!IsBuilt)
        {
            IsBuilt = true;
            //PawnManager.ApplyDiligenceWonderBuff();
            Debug.Log("勤工奇观建成，全体小人工作速度提升！");
        }
    }
    public static void OnDestroyed()
    {
        if (IsBuilt)
        {
            IsBuilt = false;
            //PawnManager.RemoveDiligenceWonderBuff();
            Debug.Log("勤工奇观被摧毁，工作速度恢复正常。");
        }
    }
}

// 巨力奇观：提升全体小人运载容量
public class MightWonderBuilding : Building
{
    public static bool IsBuilt = false;
    public const float CapacityBuff = 1.5f;
    public MightWonderBuilding()
    {
        this.id = 103;
        this.build_name = "巨力奇观";
        this.texture = null;
        this.type = BuildingType.Dev;
        this.width = 2;
        this.height = 2;
        this.durability = 99999;
        this.can_build = true;
        this.can_walk = false;
        this.can_plant = false;
        this.material_list = new List<KeyValuePair<int, int>>{
        };
    }
    public static void OnBuilt()
    {
        if (!IsBuilt)
        {
            IsBuilt = true;
            //PawnManager.ApplyMightWonderBuff();
            Debug.Log("巨力奇观建成，全体小人运载容量提升！");
        }
    }
    public static void OnDestroyed()
    {
        if (IsBuilt)
        {
            IsBuilt = false;
            //PawnManager.RemoveMightWonderBuff();
            Debug.Log("巨力奇观被摧毁，运载容量恢复正常。");
        }
    }
}
#endregion
