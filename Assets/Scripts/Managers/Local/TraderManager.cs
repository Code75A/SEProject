using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TraderManager : MonoBehaviour
{
    public static TraderManager Instance { get; private set; }
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

    #region 常数
    const int DEFAULT_GOODS_COUNT = 5;
    const int DEFAULT_BALANCE = 4000;
    public static int TRADER_ID = 100; 
    #endregion

    #region 商人刷新设置
    public float spawnProbability = 0.3f;
    public float spawnProbabilityIncrement = 0.1f;
    public float spawnProbabilityMax = 0.9f;
    public float currentSpawnProbability = 1f;

    public int goodsCount = DEFAULT_GOODS_COUNT;
    public int balance = DEFAULT_BALANCE;

    private Dictionary<ItemManager.Item, float> itemWeights = new();
    private Dictionary<ItemManager.Item, int> itemHistory = new();
    public List<ItemManager.Item> availableItems = new();
    #endregion

    public GameObject goodsDetail;
    public Sprite trader_sprite;
    public TraderBuilding trader;
    public bool isTraderActive = true;
    public bool inTraderPanel = false;

    public class TraderBuilding : Building
    {
        public List<KeyValuePair<ItemManager.Item, int>> goods = new List<KeyValuePair<ItemManager.Item, int>>();
        public void Init()
        {
            // 固定父类属性
            this.id = TRADER_ID;
            this.build_name = "商人";
            this.texture = Instance.trader_sprite; // 可以替换为你的商人建筑Sprite
            this.type = BuildingType.Dev; // 或者你定义的专属类型
            this.width = 1;
            this.height = 1;
            this.durability = -1;
            this.can_build = false;
            this.can_walk = true;
            this.can_plant = false;
            this.material_list = new List<KeyValuePair<int, int>>();

            Debug.Log("TraderBuilding 初始化完成");
        }
        //public bool BuyItem(ItemManager.Item item){}
    }

    void Start()
    {
        Init();
        trader = ScriptableObject.CreateInstance<TraderBuilding>();
        trader.Init();

        GenerateTraderGoods();

        foreach (var trader_goods_pair in trader.goods)
        {
            ItemManager.Item trader_goods = trader_goods_pair.Key;
            int trader_goods_number = trader_goods_pair.Value;

            GameObject goods_detail = Instantiate(goodsDetail, UIManager.Instance.traderMenuPanelContent.transform);
            GoodsContentController goodsContent = goods_detail.GetComponent<GoodsContentController>();

            int tempPrice = 100;
            goodsContent.Init(trader_goods, tempPrice, trader_goods_number);
        }
    }

    private void Init()
    {
        // 初始化商品列表
        
        if (availableItems == null || availableItems.Count == 0)
        {
            foreach (var testItem in ItemManager.Instance.itemLists[ItemManager.ItemType.Material])
            {
                if(testItem.name != "金币")
                    availableItems.Add(testItem);
            }
        }

        // 初始化权重
        itemWeights.Clear();
        foreach (var item in availableItems)
            itemWeights[item] = 1f / availableItems.Count;
    }

    private void GenerateTraderGoods()
    {
        trader.goods.Clear();
        List<ItemManager.Item> tempItems = new List<ItemManager.Item>(availableItems);
        Dictionary<ItemManager.Item, float> tempWeights = new Dictionary<ItemManager.Item, float>(itemWeights);

        for (int i = 0; i < goodsCount && tempItems.Count > 0; i++)
        {
            float totalWeight = 0f;
            foreach (var item in tempItems)
                totalWeight += tempWeights[item];

            float rand = UnityEngine.Random.value * totalWeight;
            float sum = 0f;
            ItemManager.Item chosen = null;
            foreach (var item in tempItems)
            {
                sum += tempWeights[item];
                if (rand <= sum)
                {
                    chosen = item;
                    break;
                }
            }
            if (chosen == null) break;

            int goods_num = UnityEngine.Random.Range(0, 999);

            trader.goods.Add(new KeyValuePair<ItemManager.Item, int>(chosen,goods_num));
            tempItems.Remove(chosen);
            tempWeights.Remove(chosen);
        }

        foreach (var item_pair in trader.goods)
        {
            ItemManager.Item item = item_pair.Key;
            if (!itemHistory.ContainsKey(item)) itemHistory[item] = 0;
            itemHistory[item]++;
        }
    }

    public void DailyRefresh()
    {
        float rand = UnityEngine.Random.value;
        if (rand < currentSpawnProbability)
        {
            isTraderActive = true;
            currentSpawnProbability = spawnProbability;
            //GenerateTraderGoods();
        }
        else
        {
            isTraderActive = false;
            //traderGoods.Clear();
            currentSpawnProbability = Mathf.Min(currentSpawnProbability + spawnProbabilityIncrement, spawnProbabilityMax);
        }
    }
    
}
