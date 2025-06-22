using System.Collections.Generic;
using UnityEngine;

public class StorageManager : MonoBehaviour
{
    public static StorageManager Instance { get; private set; }

    Dictionary<int, int> MaterialManager = new Dictionary<int, int>();
    //TODO: 剔除暂时没有的条目？
    Dictionary<int, StorageMenuBarLoadController> StorageBarFinder = new Dictionary<int, StorageMenuBarLoadController>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 如果需要在场景切换时不销毁，可以加上下面这句
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 获取所有箱子引用（每次调用时动态获取，保证数据最新）
    public List<StorageBuilding> GetAllChests()
    {
        List<StorageBuilding> chests = new List<StorageBuilding>();
        foreach (var building in BuildManager.Instance.currentBuildingList)
        {
            if (building is StorageBuilding chest)
            {
                chests.Add(chest);
            }
        }
        return chests;
    }
    
    public void AddItem(int itemId, int count)
    {
        if (count == 0)
        {
            Debug.LogWarning("Attempted to add item with count 0, operation ignored.");
            return;
        }

        if (MaterialManager.ContainsKey(itemId))//更新数据
        {
            MaterialManager[itemId] += count;
            StorageBarFinder.TryGetValue(itemId, out StorageMenuBarLoadController barController);
            if (barController != null)
            {
                barController.UpdateText(MaterialManager[itemId].ToString());
            }
        }
        else//新建
        {
            MaterialManager[itemId] = count;
            StorageMenuBarLoadController controller = UIManager.Instance.AddOneStorageMenuBar(itemId, count);
            if (controller != null)
                StorageBarFinder[itemId] = controller;
        }

        if (MaterialManager[itemId] <= 0)
        {
            MaterialManager.Remove(itemId);
            UIManager.Instance.ClearStorageMenuBar();
            StorageBarFinder.Clear();

            foreach (var tuple in MaterialManager)
            {
                StorageMenuBarLoadController controller = UIManager.Instance.AddOneStorageMenuBar(tuple.Key, tuple.Value);
                if (controller != null)
                    StorageBarFinder[tuple.Key] = controller;
            }

        }
    }

    // 统计所有箱子中指定物品的总数
    public int GetTotalItemCount(int itemId)
    {
        MaterialManager.TryGetValue(itemId, out int totalCount);
        return totalCount;
    }

    // 统计所有箱子中金币（CurrencyItem）的总数
    // public int GetTotalCurrencyCount()
    // {
    //     int currencyId = 10000; // CurrencyItem的id，确保和CurrencyItem类一致
    //     return GetTotalItemCount(currencyId);
    // }
}
