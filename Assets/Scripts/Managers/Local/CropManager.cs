using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;

public class CropManager : MonoBehaviour
{
    public static CropManager Instance { get; private set; }
    const int GROWTH_STAGE_COUNT = 4;
    public const float standardGrowthPerFrame = 0.005f;
    //=========================================Factor Part=======================================
    #region 影响因素模块-class Factor的基本内容
    public enum FactorType
    {
        Linear, LinearDiffEnv, InRangeEnv, Zero, Total
    }
    public abstract class Factor
    {
        //public int id;
        public FactorType type;
        public abstract float GetImpacted(float origin);
        public abstract float GetImpacted(float origin, float env_data);
    }
    public class LinearFactor : Factor
    {
        public float change_rate;
        public override float GetImpacted(float origin) { return origin * change_rate; }
        public override float GetImpacted(float origin, float env_data)
        {
            UIManager.Instance.DebugTextAdd("<<Error>>LinearFactor.GetImpacted should not be called with expect_env.");
            return origin;
        }
    }
    public class ZeroFactor : Factor
    {
        public override float GetImpacted(float origin) { return 0.0f; }
        public override float GetImpacted(float origin, float env_data)
        {
            UIManager.Instance.DebugTextAdd("<<Error>>ZeroFactor.GetImpacted should not be called with expect_env.");
            return origin;
        }
    }
    public abstract class EnvFactor: Factor
    {
        public float standard_env;
        public float up_limit;
        public float down_limit;
        public sealed override float GetImpacted(float origin)
        {
            UIManager.Instance.DebugTextAdd("<<Warning>>EnvFactor.GetImpacted should be called with expect_env.");
            return origin;
        }
    }
    public class LinearDiffEnvFactor : EnvFactor
    {
        public override float GetImpacted(float origin, float env_data)
        {
            float use_env_data = Math.Clamp(env_data, down_limit, up_limit);
            return origin * (1.0f - (Math.Abs(standard_env - use_env_data) / (up_limit - down_limit)));
        }
    }
    public class InRangeEnvFactor : EnvFactor
    {
        public override float GetImpacted(float origin, float env_data)
        {
            if (env_data < down_limit || env_data > up_limit) return 0.0f; // Outside the range, no impact
            else return origin;
        }
    }
    #endregion
    #region 影响因素模块-动态growthPerFrame管理
    #region 0.变量部分
    public Dictionary<int, LinearFactor> pestDisasterEnvFactorDict = new Dictionary<int, LinearFactor>();
    public LinearFactor globalBuffEnvFactor = null;
    public Dictionary<int, float> growthPerFrameDict = new Dictionary<int, float>();
    #endregion
    #region 1.私有函数部分
    ///<summary>
    /// private: init the growthPerFrameDict with standardGrowthPerFrame
    /// </summary>
    public void InitGrowthPerFrameDict()
    {
        // the globalBuffEnvFactor with a default value of change_rate = 1.0f, which means no change
        globalBuffEnvFactor = new LinearFactor
        {
            //id = -1, // -1 means global buff
            type = FactorType.Linear,
            change_rate = 1.0f // default no change
        };
        for(int i=0;i< cropList.Count; i++){
            // Initialize each crop's growthPerFrameDict with the standardGrowthPerFrame
            growthPerFrameDict.Add(cropList[i].id, standardGrowthPerFrame);
            // Initialize the pestDisasterEnvFactorDict with null
            pestDisasterEnvFactorDict.Add(cropList[i].id, null);
        }
        UpdateGrowthPerFrameDict();
    }
    ///<summary>
    /// private: update the growthPerFrameDict with EnvFactor changed
    /// </summary>
    public void UpdateGrowthPerFrameDict(int crop_id = -1){
        // globalBuffEnvFactor is the global EnvFactor, it will impact all crops
        float growth_per_frame = globalBuffEnvFactor.GetImpacted(standardGrowthPerFrame);

        // pestDisasterEnvFactorDict is the local EnvFactor, it will impact each crop
        if (crop_id == -1){
            // If crop_id is -1, we update all crops' growthPerFrameDict
            foreach (var crop in cropList){
                float crop_growth_per_frame = growth_per_frame;
                if (pestDisasterEnvFactorDict[crop.id] != null)
                {
                    crop_growth_per_frame = pestDisasterEnvFactorDict[crop.id].GetImpacted(crop_growth_per_frame);
                }
                growthPerFrameDict[crop.id] = crop_growth_per_frame;
                Debug.Log("Crop: " + crop.id + " growthPerFrame: " + crop_growth_per_frame);
            }
        }
        else{
            float crop_growth_per_frame = growth_per_frame;
            if (pestDisasterEnvFactorDict[crop_id] != null){
                crop_growth_per_frame = pestDisasterEnvFactorDict[crop_id].GetImpacted(crop_growth_per_frame);
            }
            growthPerFrameDict[crop_id] = crop_growth_per_frame;
            Debug.Log("Crop: " + crop_id + " growthPerFrame: " + crop_growth_per_frame);
        }
        return;
    }
    #endregion
    #region 2.公有函数部分
    #region (1)接受改动
    public bool SetCropPestDisaster(int crop_id, float decrease_rate){
        // Safety check
        if (crop_id < 0 || crop_id >= cropList.Count){
            UIManager.Instance.DebugTextAdd("<<Error>>SetCropPestDisaster received invalid crop_id: " + crop_id.ToString());
            return false;
        }
        if (decrease_rate < 0.0f || decrease_rate > 1.0f){
            UIManager.Instance.DebugTextAdd("<<Error>>SetCropPestDisaster received invalid decrease_rate: " + decrease_rate.ToString());
            return false;
        }
        // Create a new EnvFactor with the decrease_rate
        LinearFactor env_factor = new LinearFactor { change_rate = decrease_rate };
        // Register the EnvFactor to the pestDisasterEnvFactorDict
        if (pestDisasterEnvFactorDict.ContainsKey(crop_id))
            pestDisasterEnvFactorDict[crop_id] = env_factor;
        else
            pestDisasterEnvFactorDict.Add(crop_id, env_factor);
        // Update the growthPerFrameDict with the new EnvFactor
        UpdateGrowthPerFrameDict(crop_id);
        // Notify the ItemInstanceManager to update the growth_per_frame
        ItemInstanceManager.Instance.UpdateGrowthPerFrame(crop_id);
        return true;
    }
    public bool RemoveCropPestDisaster(int crop_id){
        // Safety check
        if(crop_id < 0 || crop_id >= cropList.Count){
            UIManager.Instance.DebugTextAdd("<<Error>>RemoveCropPestDisaster received invalid crop_id: " + crop_id.ToString());
            return false;
        }
        // Remove the EnvFactor from the pestDisasterEnvFactorDict
        if(pestDisasterEnvFactorDict.ContainsKey(crop_id)){
            pestDisasterEnvFactorDict.Remove(crop_id);
            // Update the growthPerFrameDict without the EnvFactor
            UpdateGrowthPerFrameDict(crop_id);
            // Notify the ItemInstanceManager to update the growth_per_frame
            ItemInstanceManager.Instance.UpdateGrowthPerFrame(crop_id);
            return true;
        }
        else{
            UIManager.Instance.DebugTextAdd("<<Error>>RemoveCropPestDisaster received invalid crop_id: " + crop_id.ToString());
            return false;
        }
    }
    public bool SetGlobalBuffEnvFactor(float global_rate){
        // Safety check
        if (global_rate < 0.0f)
        {
            UIManager.Instance.DebugTextAdd("<<Error>>SetGlobalBuffEnvFactor received invalid global_rate: " + global_rate.ToString());
            return false;
        }
        // Set the globalBuffFactor with the change_rate
        globalBuffEnvFactor.change_rate = global_rate;
        // Update the growthPerFrameDict with the new globalBuffEnvFactor
        UpdateGrowthPerFrameDict();
        // Notify the ItemInstanceManager to update the growth_per_frame
        ItemInstanceManager.Instance.UpdateGrowthPerFrame();
        return true;
    }
    #endregion
    #region (2)供CropInstance获取信息
    /*
        将生长总耗时的扰动因素换成两部分：
            - （1）种植下去时的农田的环境因子（fertility, humidity, light）与植物需求的适配度
            - （2）生长过程中可能会受到的全局灾害和增益（pest disaster（对crop种类分类）, 其他全局buff，etc.）
        那么我们将（1）绑定到RealLifeTime上，（2）绑定到growthPerFrame上。
        CropInstance相应地设置私有成员real_lifetime和growth_per_frame作为缓存
        CropManager将负责使用相应的公式计算这两个值；前者Factor写死（植物特性），后者Factor可变（环境变化、全局buff等）
        由于后者可变，CropManager需要提供：
            - 一个接口供CropInstance获取当前的growthPerFrame。
            - 标志位提醒某种Crop的CropInstance需要更新growthPerFrame。
            - 各种可以扰动的环境因子（pest disaster, global buff等）修改growthPerFrame的接口。
    */
    public float GetGrowthPerFrame(int crop_id)
    {
        // Safety check
        if (crop_id < 0 || crop_id >= cropList.Count)
        {
            UIManager.Instance.DebugTextAdd("<<Error>>GetGrowthPerFrame received invalid crop_id: " + crop_id.ToString());
            return 0.0f;
        }
        // Return the growth per frame for the crop
        return growthPerFrameDict[crop_id];
    }
    public float GetRealLifetime(Crop crop, MapManager.MapData env_data){
        float real_lifetime =  crop.lifetime;
        real_lifetime = real_lifetime * env_data.fertility * env_data.light * Math.Abs(1.0f-env_data.humidity);
        // TODO: UseFactor to enable different require for different crop
        return real_lifetime;
    }
    #endregion
    #endregion
    #endregion

    // =====================================Crop Part==========================================
    public class Crop
    {
        public int id;
        public string name;
        public float lifetime;
        // public EnvFactor fertility_factor;
        // public EnvFactor humidity_factor;
        // public EnvFactor light_factor;
        public List<KeyValuePair<int, int>> harvest_list;
    }
    public List<Crop> cropList = new List<Crop>();
    const int CropSpritesCount = 6;
    public Sprite[] CropSprites = new Sprite[CropSpritesCount*GROWTH_STAGE_COUNT];
    public Dictionary<int, int> SeedIdDict = new Dictionary<int, int>();

    //单例模式
    void Awake(){
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitCropListData();
        InitSeedIdDict();
        InitGrowthPerFrameDict();
    }
    //=========================================Private Function Part=======================================
    /// <summary>
    /// private, tempUse without SLM
    /// </summary>
    void InitSeedIdDict()
    {
        SeedIdDict.Add(0, 7);
        SeedIdDict.Add(1, 8);
        SeedIdDict.Add(2, 9);
        SeedIdDict.Add(3, 10);
        SeedIdDict.Add(4, 11);
        SeedIdDict.Add(5, 12);
    }
    /// <summary>
    /// private
    /// </summary>
    void InitCropListData(){
        cropList.Add(new Crop{id = 0, name="水稻", lifetime=10.0f, 
            harvest_list = new List<KeyValuePair<int,int> >{
                new KeyValuePair<int, int>(ItemManager.Instance.GetItem("蓝莓",ItemManager.ItemType.Material).id ,10)
            }});
        cropList.Add(new Crop{id = 1, name="土豆", lifetime=5.0f, 
            harvest_list = new List<KeyValuePair<int,int> >{
                new KeyValuePair<int, int>(ItemManager.Instance.GetItem("草莓",ItemManager.ItemType.Material).id ,10)
            }});
        cropList.Add(new Crop{id = 2, name="小麦", lifetime=10.0f, 
            harvest_list = new List<KeyValuePair<int,int> >{
                new KeyValuePair<int, int>(ItemManager.Instance.GetItem("蓝莓",ItemManager.ItemType.Material).id ,10)
            }});
        cropList.Add(new Crop{id = 3, name="棉花", lifetime=10.0f, 
            harvest_list = new List<KeyValuePair<int,int> >{
                new KeyValuePair<int, int>(ItemManager.Instance.GetItem("蓝莓",ItemManager.ItemType.Material).id ,10)
            }});
        cropList.Add(new Crop{id = 4, name="葡萄", lifetime=15.0f, 
            harvest_list = new List<KeyValuePair<int,int> >{
                new KeyValuePair<int, int>(ItemManager.Instance.GetItem("蓝莓",ItemManager.ItemType.Material).id ,10)
            }});
        cropList.Add(new Crop{id = 5, name="树", lifetime=20.0f, 
            harvest_list = new List<KeyValuePair<int,int> >{
                new KeyValuePair<int, int>(ItemManager.Instance.GetItem("木材",ItemManager.ItemType.Material).id ,10)
            }});
    }
    //=========================================Public Function Part=======================================
    public Crop GetCrop(int id){
        //未来：Cache
        foreach (var crop in cropList){
            if (crop.id == id){
                return crop;
            }
        }

        Debug.LogError("GetCrop收到无效id: " + id);
        return null;
    }
    
    public Sprite GetSprite(int crop_id, int growth_stage){ 
        #region 安全检查
        if(crop_id >= CropSpritesCount || crop_id < 0){
            UIManager.Instance.DebugTextAdd("[ERROR] Crop with id: "+crop_id+" beyond the legal range!");
            return null;
        }
        else if(growth_stage >= GROWTH_STAGE_COUNT || growth_stage < 0){
            UIManager.Instance.DebugTextAdd("[ERROR] growth_stage: "+crop_id+" beyond the legal range!");
            return null;
        }
        #endregion

        Sprite tmp = CropSprites[crop_id*GROWTH_STAGE_COUNT+growth_stage];
        return tmp;
    }
}
