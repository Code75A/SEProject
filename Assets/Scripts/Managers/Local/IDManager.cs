using System.Collections.Generic;
using System.IO;
using UnityEngine; // 添加这一行

public class IDManager
{
    private HashSet<int> usedIds; // 已使用的 ID 集合
    private Queue<int> availableIds; // 可用的 ID 队列
    private int nextId; // 下一个生成的 ID

    public IDManager()
    {
        usedIds = new HashSet<int>();
        availableIds = new Queue<int>();
        nextId = 1; // 从 1 开始分配 ID
    }

    // 获取一个新的唯一 ID
    public int GenerateId()
    {
        int id;
        if (availableIds.Count > 0)
        {
            id = availableIds.Dequeue(); // 从回收的 ID 中取一个
        }
        else
        {
            id = nextId++;
        }
        usedIds.Add(id);
        return id;
    }

    // 回收一个 ID
    public void RecycleId(int id)
    {
        if (usedIds.Contains(id))
        {
            usedIds.Remove(id);
            availableIds.Enqueue(id); // 将回收的 ID 放入队列
        }
    }

    // 初始化已使用的 ID 集合
    public void Initialize(IEnumerable<int> existingIds)
    {
        Reset();
        foreach (var id in existingIds)
        {
            usedIds.Add(id);
            nextId = System.Math.Max(nextId, id + 1);
        }
    }

    // 重置 ID 管理器
    public void Reset()
    {
        usedIds.Clear();
        availableIds.Clear();
        nextId = 1;
    }

    // 检查 ID 是否已被使用
    public bool IsIdUsed(int id)
    {
        return usedIds.Contains(id);
    }

    // 保存到文件
    public void SaveToFile(string path)
    {
        var data = new SerializableData
        {
            UsedIds = new List<int>(usedIds),
            AvailableIds = new List<int>(availableIds),
            NextId = nextId
        };
        var json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);
    }

    // 从文件加载
    public void LoadFromFile(string path)
    {
        if (!File.Exists(path)) return;
        try
        {
            var json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<SerializableData>(json);
            if (data == null || data.UsedIds == null || data.AvailableIds == null)
                return;
            usedIds = new HashSet<int>(data.UsedIds);
            availableIds = new Queue<int>(data.AvailableIds);
            nextId = data.NextId;
        }
        catch
        {
            // 可加日志
        }
    }

    [System.Serializable] // 必须加这个
    private class SerializableData
    {
        public List<int> UsedIds;
        public List<int> AvailableIds;
        public int NextId;
    }
}
