using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public enum ItemType{
        Tool,Total
    }
    public class Item{
        public int id;
        public ItemType type;
        public SpriteRenderer texture;
        public int max_durability;
        public int durability;
    }
    public class Tool : Item{
        public Dictionary<int,int> enhancements;
    }
    public Dictionary<ItemType, List<Item>> itemLists = new Dictionary<ItemType, List<Item>>();
    
    void InitItemListsData(){
        for(int i = 0; i < (int)ItemType.Total; i++)
            itemLists.Add((ItemType)i, new List<Item>());

        #region 动态载入初始Item, 仅供测试
        itemLists[ItemType.Tool].Add(
            new Tool{id = 0, type = ItemType.Tool, texture = null, max_durability = 100, durability = 80, 
                    enhancements = new Dictionary<int, int>{{0,100},{1,200},{2,300},{3,400}}});
        itemLists[ItemType.Tool].Add(
            new Tool{id = 1, type = ItemType.Tool, texture = null, max_durability = 100, durability = 90, 
                    enhancements = new Dictionary<int, int>{{0,400},{1,300},{2,200},{3,100}}});
        itemLists[ItemType.Tool].Add(
            new Tool{id = 2, type = ItemType.Tool, texture = null, max_durability = 100, durability = 100, 
                    enhancements = new Dictionary<int, int>{{0,10},{1,100},{2,1000},{3,10000}}});
        #endregion 
    }

    // Start is called before the first frame update
    void Start()
    {
        InitItemListsData();
    }

    // Update is called once per frame
    //void Update(){}
}
