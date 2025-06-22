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
        Weather, PestDisaster, Total
    }
    public class Event
    {
        public EventType type;
        public string name;
        public int arrival;
        public int end;
        public int predictability_level;
        //public virtual bool IsPredictable(){ return false; }
        public virtual void Arrive(){ ; }
        public virtual void End(){ ; }
        public virtual void DailyImpact(){ ; }
    }
    public class Weather : Event
    {
        public enum WeatherType
        {
            Sunny, Rainy, Snowy, Stormy, Total
        }
        public WeatherType weather_type;
    }
    public class PestDisaster : Event
    {
        public int aim_crop;
        public int damage_rate;
        public override void Arrive()
        {
            UIManager.Instance.DebugTextAdd("<<Event>> " + name + " has arrived.");
            CropManager.Instance.SetCropPestDisaster(aim_crop, 1.0f - damage_rate);
        }
        public override void End()
        {
            UIManager.Instance.DebugTextAdd("<<Event>> " + name + " has ended.");
            CropManager.Instance.RemoveCropPestDisaster(aim_crop);
        }
    }

    List<Event> eventList = new List<Event>();
    public void DailyEventUpdate()
    {
        int now_day = TimeManager.Instance.GetCurrentDay();
        foreach (var it in eventList)
        {
            // Event end (last day)
            if (now_day <= it.end){
                // Events need to be Removed
                if (now_day == it.end) it.End();
                // Remove ended event
                eventList.RemoveAll(s => now_day >= s.end);
            }
            // Events not arrive
            if (now_day < it.arrival) continue;
            // Events arrive
            if (now_day == it.arrival) it.Arrive();
            // during Event
            if (now_day >= it.arrival && now_day < it.end){
                it.DailyImpact();
            }
        }
    }
    void Start()
    {
        //TODO: SLManager应当初始化载入Event列表
    }
    // void FixedUpdate()
    // {
    //     return;
    // }
}
