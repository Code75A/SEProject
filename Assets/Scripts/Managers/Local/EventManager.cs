using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    //======================================Global Reference Part====================================
    public static EventManager Instance { get; private set; } // 单例模式，确保全局唯一
    private void Awake(){
        // 实现单例模式，确保 ItemManager 只有一个实例
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
            UIManager.Instance.DebugTextAdd(
                "<<Error>> Initing the second EventManager instance FAILED, because it's not allowed. ");
        }
    }
    //======================================Event Class Part====================================
    public enum EventType
    {
        WeatherDisaster, PestDisaster, Total
    }
    public class Event
    {
        public EventType type;
        public string name;
        public int arrival;
        public int end;
        public int predictability_level;
        public bool IsPredictable()
        {
            return false;
        }
        public void Arrive()
        {
            ;
        }
        public void End()
        {
            ;
        }
        public void Impact()
        {
            ;
        }
    }
    public class WeatherDisaster : Event { }
    public class PestDisaster : Event
    {
        public int aim_crop;
        public int control_level;
    }

    List<Event> disasterList = new List<Event>();
    public void UpdateDisaster()
    {
        int now_day = 0; //TODO:get time from timeManager
        foreach (var it in disasterList)
        {
            // Disaster not arrive
            if (now_day < it.arrival) continue;
            // Disaster arrive
            if (now_day == it.arrival) it.Arrive();
            // during Disaster
            if (now_day >= it.arrival && now_day < it.end)
            {
                it.Impact();
                // Disaster end (last day)
                if (now_day == it.end - 1) it.End();
            }

        }
        // Remove ended disaster
        disasterList.RemoveAll(s => now_day >= s.end);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
