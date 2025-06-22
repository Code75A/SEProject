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
    public void DailyEventUpdate(){
        int now_day = TimeManager.Instance.GetCurrentDay();
        // Deal all end events
        foreach (var it in eventList){
            // Event end (last day)
            if (now_day <= it.end){
                // Events need to be Removed
                if (now_day == it.end){
                    it.End(); 
                    // TODO: Send end message to UI
                } 
            }
        }
        // Remove ended event
        eventList.RemoveAll(s => now_day >= s.end);
        // Deal other events
        foreach (var it in eventList){
            // Events not arrive
            if (now_day < it.arrival) continue;
            // Events arrive
            if (now_day == it.arrival){
                it.Arrive();
                // TODO: Send arrival message to UI
            } 
            // during Event
            if (now_day >= it.arrival && now_day < it.end){
                it.DailyImpact();
            }
        }
    }
    /// <summary>
    /// 新增一个对指定作物的虫害事件
    /// </summary>
    public bool AddPestDisasterEvent(int aim_crop, int arrival, int predictability_level, int end, int damage_rate){
        // Safety Check
        if (arrival <= TimeManager.Instance.currentDay){
            UIManager.Instance.DebugTextAdd("<<Warning>> arrival day HAS ARRIVED.");
            return false;
        }
        if (arrival >= end) {
            UIManager.Instance.DebugTextAdd("<<Warning>> arrival day is equal with or later than end day.");
            return false;
        }
        if (damage_rate < 0 || damage_rate > 1){
            UIManager.Instance.DebugTextAdd("<<Warning>> Illegal damage rate.");
            return false;
        }
        // create
        PestDisaster pest_disaster = new PestDisaster{
            type = EventType.PestDisaster,
            name = "Pest Disaster on Crop " + aim_crop.ToString(),
            arrival = arrival,
            end = end,
            predictability_level = predictability_level,
            aim_crop = aim_crop,
            damage_rate = damage_rate
        };
        eventList.Add(pest_disaster);
        UIManager.Instance.DebugTextAdd("<<Log>> " + pest_disaster.name + " will arrive at day " + arrival.ToString() +
            " and end at day " + end.ToString() + ". It will damage crop " + aim_crop.ToString() +
            " with a rate of " + damage_rate.ToString() + "."
        );
        // if it can be predictable for now village
        if (IsEventPredictable(EventType.PestDisaster, predictability_level))
        {
            // TODO: Send a Message to UIManager, and other work?
            UIManager.Instance.DebugTextAdd("<<Log>> Predict the event: " + pest_disaster.name);
        }
        return true;
    }
    public bool IsEventPredictable(EventType type, int predictability_level){
        // TODO: 预测机制待完成
        return true;
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
