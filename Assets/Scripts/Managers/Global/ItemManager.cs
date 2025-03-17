using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    //======================================Global Reference Part====================================
    public static ItemManager Instance { get; private set; } // 单例模式，确保全局唯一
    public UIManager uiManager;
    public SLManager slManager;
    public PawnManager pawnManager;
    private void Awake(){
        // 实现单例模式，确保 ItemManager 只有一个实例
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
            UIManager.Instance.DebugTextAdd(
                "Error: Your operation of initing the second ItemManager instance FAILED, becauese it's not allowed. ");
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
        public SpriteRenderer texture;
    }
    public class Tool : Item{
        // TODO: EnhanceType应该归Tool所有吗？还是直接为小人建立属性列表，然后引用到Tool实现内？
        public enum EnhanceType{
            Speed, Power, Total
        }
        /// <summary>
        /// 工具的强化属性列表，key为强化项id，value为强化值
        /// </summary>
        public Dictionary<EnhanceType,int> enhancements;
        public int max_durability;
    }
    public class Material : Item{}

    //=========================================Manager Fuction Part=======================================
    //=========================================Private Fuction Part=======================================
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
            new Tool{id = 0, type = ItemType.Tool, texture = null, max_durability = 100,
                    enhancements = new Dictionary<Tool.EnhanceType, int>{{Tool.EnhanceType.Speed,100},{Tool.EnhanceType.Power,0}}});
        itemLists[ItemType.Tool].Add(
            new Tool{id = 1, type = ItemType.Tool, texture = null, max_durability = 100, 
                    enhancements = new Dictionary<Tool.EnhanceType, int>{{Tool.EnhanceType.Speed,0},{Tool.EnhanceType.Power,200}}});
        itemLists[ItemType.Tool].Add(
            new Tool{id = 2, type = ItemType.Tool, texture = null, max_durability = 100, 
                    enhancements = new Dictionary<Tool.EnhanceType, int>{{Tool.EnhanceType.Speed,10},{Tool.EnhanceType.Power,100}}});
        // Material
        itemLists[ItemType.Material].Add(
            new Material{id = 3, type = ItemType.Material, texture = null});
        itemLists[ItemType.Material].Add(
            new Material{id = 4, type = ItemType.Material, texture = null});
        itemLists[ItemType.Material].Add(
            new Material{id = 5, type = ItemType.Material, texture = null});
        #endregion 
    }
    void Start()
    {
        InitItemListsData();
    }
    //void Update(){}

    //=========================================Public Fuction Part=======================================
    /// <summary>
    /// 获取指定id的Item
    /// </summary>
    public Item GetItem(int item_id, ItemType type = ItemType.Total){
        Item item = null;
        if(type != ItemType.Total){
            item = itemLists[type].Find(c => c.id == item_id);
            
            if(item is null){
                UIManager.Instance.DebugTextAdd(
                    "[Log]Getting Item FAILED: the item_id "+ item_id +" is not found in "+type.ToString()+" List of ItemManager. "
                );
            }
            return item;
        }
        else{
            for(ItemType i = 0; i < ItemType.Total; i++){
                if(itemLists[i] is not null)
                    item = itemLists[i].Find(c => c.id == item_id);
                if(item is not null)
                    break;
            }

            if(item is null){
                UIManager.Instance.DebugTextAdd(
                    "[Log]Getting Item FAILED: the item_id "+ item_id +" is not found in ItemManager. "
                );
            }
            return item;
        }
    }
}
