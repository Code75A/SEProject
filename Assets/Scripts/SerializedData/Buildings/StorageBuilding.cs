using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Concrete/Buildings/StorageBuilding")]
public class StorageBuilding : Building
{
    // 箱子格子数量和每格最大堆叠数量
    public int storage_id;
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

    public void Init(int id)
    {
        this.storage_id = id; // 由外部传入
        //TODO: 如何静态初始化？可能得改变数据结构
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
