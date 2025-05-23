using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageManager : MonoBehaviour
{
    public static StorageManager Instance { get; private set; }

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
    public List<ChestBuilding> GetAllChests()
    {
        List<ChestBuilding> chests = new List<ChestBuilding>();
        foreach (var building in BuildManager.Instance.currentBuildingList)
        {
            if (building is ChestBuilding chest)
            {
                chests.Add(chest);
            }
        }
        return chests;
    }

    // 统计所有箱子中指定物品的总数
    public int GetTotalItemCount(int itemId)
    {
        int total = 0;
        foreach (var chest in GetAllChests())
        {
            total += chest.GetItemCount(itemId);
        }
        return total;
    }

    // 统计所有箱子中金币（CurrencyItem）的总数
    public int GetTotalCurrencyCount()
    {
        int currencyId = 10000; // CurrencyItem的id，确保和CurrencyItem类一致
        return GetTotalItemCount(currencyId);
    }
}
