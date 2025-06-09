
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }
    public enum BuildingType{
        Dev,Wall,Farm,Total
    }

    public class Building{
        //数据属性
        public int id;
        public string name;
        public Sprite texture;
        public BuildingType type;
        //游戏属性
        public int width, height;
        public int durability;

        public bool can_build;
        public bool can_walk;
        public bool can_plant;

        public List<KeyValuePair<int, int>> material_list = new List<KeyValuePair<int, int>>();

        //TODO: 拓展为List<bool> cans + enum canTypes{walk,build,plant}
    }

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

    void InitBuildingListsData(){
        for(int i = 0; i < (int)BuildingType.Total; i++)
            buildingLists.Add((BuildingType)i, new List<Building>());
        
        //TODO: 目前的material_list直接参考id，后续应该改为引用

        buildingLists[BuildingType.Dev].Add(new Building{
            id = 0, name="草地", texture = tempBuildingSprites[0], type = BuildingType.Dev, width = 1, height = 1, durability = -1,
            can_build = true, can_walk = true, can_plant = true, material_list = new List<KeyValuePair<int, int>>()});
        buildingLists[BuildingType.Dev].Add(new Building{
            id = 1, name="路径", texture = tempBuildingSprites[1], type = BuildingType.Dev, width = 1, height = 1, durability = -1,
            can_build = false, can_walk = true, can_plant = false, material_list = new List<KeyValuePair<int, int>>()});
        buildingLists[BuildingType.Dev].Add(new Building{
            id = 2, name="水地", texture = tempBuildingSprites[2], type = BuildingType.Dev, width = 1, height = 1, durability = -1,
            can_build = false, can_walk = false, can_plant = false, material_list = new List<KeyValuePair<int, int>>()});
        buildingLists[BuildingType.Dev].Add(new Building{
            id = 3, name="树木", texture = tempBuildingSprites[3], type = BuildingType.Dev, width = 1, height = 1, durability = -1,
            can_build = false, can_walk = false, can_plant = false, material_list = new List<KeyValuePair<int, int>>()});
        buildingLists[BuildingType.Dev].Add(new Building{
            id = 4, name="石地", texture = tempBuildingSprites[6], type = BuildingType.Dev, width = 1, height =1, durability = -1,
            can_build =true, can_walk = true, can_plant = false, material_list = new List<KeyValuePair<int, int>>()});

        buildingLists[BuildingType.Wall].Add(new Building{
            id = 6, name="墙", texture = tempBuildingSprites[4], type = BuildingType.Wall, width = 1, height = 1, durability = 100,
            can_build = false, can_walk = false, can_plant = false, material_list = new List<KeyValuePair<int, int>>{
                new KeyValuePair<int, int>(ItemManager.Instance.GetItem("木材").id, 10)
            }});

        buildingLists[BuildingType.Farm].Add(new Building{
            id = 5, name="农田", texture = tempBuildingSprites[5], type = BuildingType.Farm, width = 1, height = 1, durability = -1,
            can_build = false, can_walk = true, can_plant = true, material_list = new List<KeyValuePair<int, int>>()});
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

public class ChestBuilding : BuildManager.Building
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
        this.name = "箱子";
        this.texture = null;
        this.type = BuildManager.BuildingType.Dev;
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
