using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    //======================================Global Reference Part====================================
    public static ItemManager Instance { get; private set; } // 单例模式，确保全局唯一
    private void Awake(){
        // 实现单例模式，确保 ItemManager 只有一个实例
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
            UIManager.Instance.DebugTextAdd(
                "<<Error>> Initing the second ItemManager instance FAILED, because it's not allowed. ");
        }
    }

    //=========================================Item Class Part=======================================
    public enum ItemType{
        //ItemType详解
        /*
        单个Item相当于一个“图鉴项”，是一类物品的抽象存在
        目前Item子类类型的细化初步设定为：
        1. Tool: 工具类，其实例拥有独立耐久值，并且因Pawn实例的使用或修理等操作变化；耐久归零时消灭：如斧头、锄头等；
        2. Material：材料类，以单个单元为计数单位，其实例通常被批量使用后消灭：如木材、石材等；
            -- 关于Consumable：某些Item可以属于消耗品类，如食物、药品等，可以作为一个接口来实现；
        3. Building(交给BuildingManager)：建筑类, 在地块上消耗Material实例建造，可以被拆除（转化为其他Material实例）或被卸载（Building实例放入背包）；
        4. Crop(交给CropManager)：作物类，其实例一般不会被放在背包里，在地块上生长，成熟后可以被收获，收获后消灭并产生Material实例；
        此外，Total用于指示ItemType的总数。
        */
        Tool, Material, Building, Crop, Total
    }
    public class Item{
        public int id;
        public string name;
        public ItemType type;
        public Sprite texture;
    }
    public class Tool : Item{
        /// <summary>
        /// 工具的强化属性列表，key为强化项id，value为强化值
        /// </summary>
        public Dictionary<PawnManager.Pawn.EnhanceType,int> enhancements;
        public int max_durability;
    }
    public class Material : Item
    {
        public int can_plant_crop;
        public bool IsSeed()
        {
            if (can_plant_crop >= 0 && CropManager.Instance.GetCrop(can_plant_crop) != null) return true;
            return false;
        }
        public int GetPlantCropId()
        {
            if (IsSeed()) return can_plant_crop;
            else return -1;
        }
    }

    const int tempItemSpritesCount = 7;
    public Sprite[] tempItemSprites = new Sprite[tempItemSpritesCount];

    //=========================================Manager Function Part=======================================
    //=========================================Private Function Part=======================================
    public Dictionary<ItemType, List<Item>> itemLists = new Dictionary<ItemType, List<Item>>();
    void InitItemListsData(){
        #region itemLists初始化
        itemLists.Add(ItemType.Tool, new List<Item>());
        itemLists.Add(ItemType.Material, new List<Item>());
        itemLists.Add(ItemType.Building, null);
        itemLists.Add(ItemType.Crop, null);
        #endregion

        #region 动态载入初始Item, 仅供测试
        // Tool
        itemLists[ItemType.Tool].Add(
            new Tool{id=0, name="采矿镐", type=ItemType.Tool, texture=tempItemSprites[0], max_durability=100,
                    enhancements=new Dictionary<PawnManager.Pawn.EnhanceType, int>{
                        {PawnManager.Pawn.EnhanceType.capacity,0}, 
                        {PawnManager.Pawn.EnhanceType.Speed,100},
                        {PawnManager.Pawn.EnhanceType.Power,0}}});
        itemLists[ItemType.Tool].Add(
            new Tool{id=1, name="镰刀", type=ItemType.Tool, texture=tempItemSprites[1], max_durability=100, 
                    enhancements=new Dictionary<PawnManager.Pawn.EnhanceType, int>{
                        {PawnManager.Pawn.EnhanceType.capacity,0}, 
                        {PawnManager.Pawn.EnhanceType.Speed,0},
                        {PawnManager.Pawn.EnhanceType.Power,200}}});
        itemLists[ItemType.Tool].Add(
            new Tool{id=2, name="斧头", type=ItemType.Tool, texture=tempItemSprites[2], max_durability=100, 
                    enhancements=new Dictionary<PawnManager.Pawn.EnhanceType, int>{
                        {PawnManager.Pawn.EnhanceType.capacity,0}, 
                        {PawnManager.Pawn.EnhanceType.Speed,10},
                        {PawnManager.Pawn.EnhanceType.Power,100}}});
        itemLists[ItemType.Tool].Add(
            new Tool{id=6, name="手推车", type=ItemType.Tool, texture=tempItemSprites[6], max_durability=100, 
                    enhancements=new Dictionary<PawnManager.Pawn.EnhanceType, int>{
                        {PawnManager.Pawn.EnhanceType.capacity,100}, 
                        {PawnManager.Pawn.EnhanceType.Speed,0},
                        {PawnManager.Pawn.EnhanceType.Power,0}}});
        // Material
        itemLists[ItemType.Material].Add(
            new Material{id=3, name="蓝莓", type=ItemType.Material, texture=tempItemSprites[3], can_plant_crop=-1});
        itemLists[ItemType.Material].Add(
            new Material{id=4, name="草莓", type=ItemType.Material, texture=tempItemSprites[4], can_plant_crop=-1});
        itemLists[ItemType.Material].Add(
            new Material{id=5, name="木材", type=ItemType.Material, texture=tempItemSprites[5], can_plant_crop=-1});
        
        itemLists[ItemType.Material].Add(
            new Material{id=7, name="稻种", type=ItemType.Material, texture=tempItemSprites[5], can_plant_crop=0});
        itemLists[ItemType.Material].Add(
            new Material{id=8, name="土豆种子", type=ItemType.Material, texture=tempItemSprites[5], can_plant_crop=1});
        itemLists[ItemType.Material].Add(
            new Material{id=9, name="麦种", type=ItemType.Material, texture=tempItemSprites[5], can_plant_crop=2});
        itemLists[ItemType.Material].Add(
            new Material{id=10, name="棉花种子", type=ItemType.Material, texture=tempItemSprites[5], can_plant_crop=3});
        itemLists[ItemType.Material].Add(
            new Material{id=11, name="葡萄藤根", type=ItemType.Material, texture=tempItemSprites[5], can_plant_crop=4});
        itemLists[ItemType.Material].Add(
            new Material{id=12, name="树苗", type=ItemType.Material, texture=tempItemSprites[5], can_plant_crop=5});
        #endregion 
    }
    void Start()
    {
        InitItemListsData();
    }
    //void Update(){}

    //=========================================Public Function Part=======================================
    /// <summary>
    /// 获取指定id的Item
    /// </summary>
    public Item GetItem(int item_id, ItemType type = ItemType.Total){
        Item item = null;
        if(type != ItemType.Total)
            item = itemLists[type].Find(c => c.id == item_id);
        else{
            for(ItemType i = 0; i < ItemType.Total; i++){
                if(itemLists[i] is not null)
                    item = itemLists[i].Find(c => c.id == item_id);
                if(item is not null)
                    break;
            }
        }
        if(item is null){
            UIManager.Instance.DebugTextAdd(
                "[Log]Getting Item FAILED: the item_id "+ item_id +" is not found in "+type.ToString()+" List of ItemManager. "
            );
        }
        return item;
    }
    /// <summary>
    /// 获取指定name的Item
    /// </summary>
    public Item GetItem(string item_name, ItemType type = ItemType.Total){
        Item item = null;
        if(type != ItemType.Total)
            item = itemLists[type].Find(c => c.name == item_name);
        else{
            for(ItemType i = 0; i < ItemType.Total; i++){
                if(itemLists[i] is not null)
                    item = itemLists[i].Find(c => c.name == item_name);
                if(item is not null)
                    break;
            }
        }
        if(item is null){
            UIManager.Instance.DebugTextAdd(
                "[Log]Getting Item FAILED: the item_name "+ item_name +" is not found in "+type.ToString()+" List of ItemManager. "
            );
        }
        return item;
    }
}
