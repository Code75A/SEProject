using UnityEngine;
using TMPro;
public class TimeManager : MonoBehaviour{

    const float SECONDS_PER_GAMEDAY = 300f;
    public static TimeManager Instance { get;  set; }   //单例模式，确保只有一个timemanager
    public float realityTime { get;  set; } = 0f; // 现实时间
    public float gameTime { get;  set; } = 0f; // 游戏内时间
    public float timeScale = 1f; // 时间倍率

    public int currentDay = 0;
    public enum Seasons { Spring, Summer, Fall, Winter }

    //暂定春季开始，后续可能可以让玩家自行设定初始季节
    public Seasons currentSeason = Seasons.Spring;
    
    public int GetCurrentDay() {
        return currentDay; // 获取当前天数
    }

    public Seasons GetCurrentSeason() {
        return currentSeason; // 获取当前季节
    }

    // 每次天数变化时需要执行的操作，留作扩展
    private void OnDayChanged()
    {
        // TODO: 在此处进行当天数变化时对应的处理，比如刷新事件、更新界面等
        //EventManager.Instance.DailyEventUpdate(); // 更新事件
    }

    // 每次季节变化时需要执行的操作，留作扩展
    private void OnSeasonChanged(){
        // TODO: 在此处进行当季节变化时对应的处理，比如切换季节贴图、刷新环境效果等
    }

    // 绑定 UI 组件，显示游戏时间
    public TMP_Text gameTimeText;
    private void Awake()
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

    private void Update(){
        float deltaTime = Time.deltaTime;//利用帧间隔时间计算现实时间
        // 现实时间累加
        realityTime += deltaTime;
        // 游戏时间受 timeScale 影响
        gameTime += deltaTime * timeScale;

        if (gameTime >= (currentDay + 1) * SECONDS_PER_GAMEDAY){
            currentDay++;
            OnDayChanged();

            // 每 30 天改变季节
            int seasonIndex = (currentDay / 30) % 4;
            var newSeason = (Seasons)seasonIndex;
            if (newSeason != currentSeason)
            {
                currentSeason = newSeason;
                OnSeasonChanged();
            }
        }
        // 更新 UI
        UpdateGameTimeUI();
    }

    private void UpdateGameTimeUI(){
        if (gameTimeText != null){
            gameTimeText.text = $"游戏时间: {gameTime:F2}s (x{timeScale})\n游戏天数: {currentDay}";//显示游戏时间，保留两位小数点，并显示倍率
            //todo:注意这里计时是多少多少秒，到实际显示需要转换成游戏中的"日期“等数值，比如10秒后是几点几分等。
        }
        else{
            Debug.LogError("gameTimeText 未绑定！");
        }
    }

    // 修改时间倍率
    public void SetTimeScale(float newScale)
    {
        timeScale = Mathf.Clamp(newScale, 0.1f, 10f); // 限制倍率范围在（0.1-10）之间
        //具体倍率输入即可，这里做了范围限制
    }
}
