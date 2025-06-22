using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CropManager : MonoBehaviour
{
    public static CropManager Instance { get; private set; }
    
    #region 常数部分
    const int GROWTH_STAGE_COUNT = 4;
    public const float standardGrowthPerFrame = 0.005f;
    const float ENV_DATA_UP_LIMIT= 6.0f;
    const float ENV_DATA_DOWN_LIMIT = 0.0f;
    // const float UP_LIMIT_FERTILITY = 6.0f;
    // const float DOWN_LIMIT_FERTILITY = 0.0f;
    // const float UP_LIMIT_HUMIDITY = 6.0f;
    // const float DOWN_LIMIT_HUMIDITY = 0.0f;
    // const float UP_LIMIT_LIGHT = .0f;
    // const float DOWN_LIMIT_LIGHT = 0.0f;
    #endregion
    
    //=========================================Factor Part=======================================
    #region 影响因素模块-动态growthPerFrame管理
    #region 0.变量部分
    // For growth per frame
    public Dictionary<int, LinearFactor> pestDisasterEnvFactorDict = new Dictionary<int, LinearFactor>();
    public LinearFactor globalBuffEnvFactor = null;
    public Dictionary<int, float> growthPerFrameDict = new Dictionary<int, float>();
    // For real life time，some environment conditions will have different impacts on different crops
    // So every crop has its own fertility, humidity, and light factors
    // These factors should be initialized with the message from CropList at every beginning of game
    public Dictionary<int, EnvFactor> fertilityFactorDict = new Dictionary<int, EnvFactor>();
    public Dictionary<int, EnvFactor> humidityFactorDict = new Dictionary<int, EnvFactor>();
    public Dictionary<int, EnvFactor> lightFactorDict = new Dictionary<int, EnvFactor>();
    #endregion
    #region 1.私有函数部分
    ///<summary>
    /// private: init the growthPerFrameDict with standardGrowthPerFrame
    /// </summary>
    public void InitGrowthPerFrameDict() {
        // the globalBuffEnvFactor with a default value of change_rate = 1.0f, which means no change
        globalBuffEnvFactor = Resources.Load<LinearFactor>("FactorData/GlobalBuffEnvFactor");
        
        for (int i = 0; i < cropList.Count; i++)
        {
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
                if (pestDisasterEnvFactorDict[crop.id] != null) {
                    crop_growth_per_frame = pestDisasterEnvFactorDict[crop.id].GetImpacted(crop_growth_per_frame);
                }
                growthPerFrameDict[crop.id] = crop_growth_per_frame;
                //Debug.Log("Crop: " + crop.id.ToString() + " growthPerFrame: " + crop_growth_per_frame.ToString());
            }
        }
        else{
            float crop_growth_per_frame = growth_per_frame;
            if (pestDisasterEnvFactorDict[crop_id] != null){
                crop_growth_per_frame = pestDisasterEnvFactorDict[crop_id].GetImpacted(crop_growth_per_frame);
            }
            growthPerFrameDict[crop_id] = crop_growth_per_frame;
            //Debug.Log("Crop: " + crop_id.ToString() + " growthPerFrame: " + crop_growth_per_frame.ToString());
        }
        return;
    }
    #endregion
    #region 2.公有函数部分
    #region (1)接受改动
    /// <summary>
    /// 本函数用于设置对某种Crop的虫害灾害影响。
    /// 该函数会创建一个新的EnvFactor，并将其注册到pestDisasterEnvFactorDict中。
    /// 由于影响growthPerFrameDict的EnvFactor是可变的，所以会在设置后触发更新growthPerFrameDict
    /// 然后连锁触发ItemInstanceManager更新相应CropInstance的growth_per_frame缓存
    /// </summary>
    /// <param name="crop_id">目前合法值在[0, Crop种类总数-1]</param>
    /// <param name="decrease_rate">合法值在[0, 1]</param>
    /// <returns>返回是否成功设置虫害灾害影响</returns>
    public bool SetCropPestDisaster(int crop_id, float decrease_rate){
        // Safety check
        // TODO：若后续Crop的id不再按从0顺序不间断排列，则参数crop_id唯一合法标准是“合法CropID”，
        // 需要更新（或直接删除部分）SafetyCheck方法。
        if (crop_id < 0 || crop_id >= cropList.Count){
            UIManager.Instance.DebugTextAdd("<<Error>>SetCropPestDisaster received invalid crop_id: " + crop_id.ToString());
            return false;
        }
        if (decrease_rate < 0.0f || decrease_rate > 1.0f)
        {
            UIManager.Instance.DebugTextAdd("<<Error>>SetCropPestDisaster received invalid decrease_rate: " + decrease_rate.ToString());
            return false;
        }
        // Create a new EnvFactor with the decrease_rate
        LinearFactor env_factor = ScriptableObject.CreateInstance<LinearFactor>();
        env_factor.change_rate = decrease_rate;
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
    /// <summary>
    /// 本函数用于移除某种Crop的虫害灾害影响。
    /// 该函数会从pestDisasterEnvFactorDict中移除对应的EnvFactor。
    /// 由于影响growthPerFrameDict的EnvFactor是可变的，所以在移除后会触发更新growthPerFrameDict
    /// 然后连锁触发ItemInstanceManager更新相应CropInstance的growth_per_frame缓存
    /// </summary>
    /// <param name="crop_id">目前合法值在[0, Crop种类总数-1]</param>
    /// <returns>返回是否成功移除虫害灾害影响；移除不成功包括了当前不存在针对该Crop的虫害灾害影响的情况</returns>
    public bool RemoveCropPestDisaster(int crop_id){
        // Safety check
        // TODO：若后续Crop的id不再按从0顺序不间断排列，则参数crop_id唯一合法标准是“合法CropID”，
        // 需要更新（或直接删除部分）SafetyCheck方法。
        if (crop_id < 0 || crop_id >= cropList.Count){
            UIManager.Instance.DebugTextAdd("<<Error>>RemoveCropPestDisaster received invalid crop_id: " + crop_id.ToString());
            return false;
        }
        // Remove the EnvFactor from the pestDisasterEnvFactorDict
        if (pestDisasterEnvFactorDict.ContainsKey(crop_id)){
            pestDisasterEnvFactorDict.Remove(crop_id);
            // Update the growthPerFrameDict without the EnvFactor
            UpdateGrowthPerFrameDict(crop_id);
            // Notify the ItemInstanceManager to update the growth_per_frame
            ItemInstanceManager.Instance.UpdateGrowthPerFrame(crop_id);
            return true;
        }
        else
        {
            UIManager.Instance.DebugTextAdd("<<Error>>RemoveCropPestDisaster received invalid crop_id: " + crop_id.ToString());
            return false;
        }
    }
    /// <summary>
    /// 本函数用于设置全局Crop生长增益/削弱的倍率
    /// </summary>
    /// <param name="global_rate">合法值为大于0的浮点数</param>
    /// <returns>返回是否成功设置全局Crop生长倍率</returns>
    public bool SetGlobalBuffEnvFactor(float global_rate)
    {
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
            - 各种可以扰动的环境因子（pest disaster, global buff等）修改growthPerFrame的接口。
        同时ItemInstanceManager需要提供一个接口供CropManager更新相应CropInstance的growth_per_frame。
    */
    /// <summary>
    /// 获取属于某种Crop的CropInstance的growth_per_frame缓存
    /// </summary>
    /// <param name="crop_id">目前合法值在[0, Crop种类总数-1]</param>
    /// <returns>返回该Crop的growth_per_frame缓存，查询失败会返回0.0f</returns>
    public float GetGrowthPerFrame(int crop_id)
    {
        // Safety check
        // TODO：若后续Crop的id不再按从0顺序不间断排列，则参数crop_id唯一合法标准是“合法CropID”。
        // 需要更新（或直接删除部分）SafetyCheck方法。
        if (crop_id < 0 || crop_id >= cropList.Count){
            UIManager.Instance.DebugTextAdd("<<Error>>GetGrowthPerFrame received invalid crop_id: " + crop_id.ToString());
            return 0.0f;
        }
        // Return the growth per frame for the crop
        return growthPerFrameDict[crop_id];
    }
    /// <summary>
    /// 携带环境数据获取属于某种Crop的CropInstance的real_lifetime缓存
    /// </summary>
    /// <param name="crop_id">目前合法值在[0, Crop种类总数-1]</param>
    /// <param name="env_data">环境数据</param>
    /// <returns>返回该Crop的real_lifetime缓存，查询失败会返回0.0f</returns>
    public float GetRealLifetime(int crop_id, MapManager.MapData env_data)
    {
        // Safety check
        // TODO：若后续Crop的id不再按从0顺序不间断排列，则参数crop_id唯一合法标准是“合法CropID”。
        // 需要更新（或直接删除部分）SafetyCheck方法。
        if (crop_id < 0 || crop_id >= cropList.Count || env_data == null){
            UIManager.Instance.DebugTextAdd(
                "<<Error>>GetRealLifetime received null: "
                + "crop: " + ((crop_id < 0 || crop_id >= cropList.Count) ? "null" : "not null")
                + ", env_data: " + (env_data == null ? "null" : "not null"));
            return 0.0f;
        }

        // Calculate the real lifetime based on the crop's lifetime and the environment data
        float grow_speed = 1.0f;
        //  Apply fertility factor
        grow_speed *= fertilityFactorDict[crop_id].GetImpacted(1.0f, env_data.fertility);
        //Debug.Log("grow_speed: " + grow_speed.ToString());
        //  Apply humidity factor
        grow_speed *= humidityFactorDict[crop_id].GetImpacted(1.0f, env_data.humidity);
        //Debug.Log("grow_speed: " + grow_speed.ToString());
        //  Apply light factor
        grow_speed *= lightFactorDict[crop_id].GetImpacted(1.0f, env_data.light);
        //Debug.Log("grow_speed: " + grow_speed.ToString());

        //Debug.Log("Crop: " + crop_id.ToString() + " grow speed rate: " + grow_speed.ToString());

        return cropList[crop_id].lifetime / grow_speed;
    }
    #endregion
    #endregion
    #endregion

    // =====================================Crop Part==========================================
    public List<Crop> cropList = new List<Crop>();
    public Dictionary<int, Crop> cropDict = new Dictionary<int, Crop>();
    const int CropSpritesCount = 6;
    public Sprite[] CropSprites = new Sprite[CropSpritesCount*GROWTH_STAGE_COUNT];
    /// <summary>
    /// Dictionary to map crop IDs to their seed(Materials) IDs.
    /// </summary>
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
        // CropListShould be initialized before any other operations
        // the Operation is temporarily without SLM
        InitCropListData();
        // Other initializations: Dicts
        InitCropDict();
        InitSeedIdDict();
        InitGrowthPerFrameDict();
        InitEnvFactorDicts();
    }
    //=========================================Private Function Part=======================================
    /// <summary>
    /// private: temporarily initialize the cropList with some default crops.
    /// </summary>
    void InitCropListData()
    {
        // 区分编辑器和运行时环境
        // AssetsDatabase方法只有Editor开发者模式下可以调用
        // 发行版/build版需要用Resources.Load
#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:Crop", new[] { "Assets/Resources/CropData" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Crop crop = AssetDatabase.LoadAssetAtPath<Crop>(path);
            if (crop != null)
            {
                crop.InitHarvestList();
                cropList.Add(crop);
            }
        }
#else
        Crop[] crops = Resources.LoadAll<Crop>("CropData");
        foreach (var crop in crops)
        {
            if (crop != null){
                crop.InitHarvestList();
                cropList.Add(crop);
            }
        }
#endif
    }
    /// <summary>
    /// private, init cropDict with cropList
    /// Used to quickly access crops by their ID.
    /// </summary>
    void InitCropDict(){
        foreach (var crop in cropList){
            cropDict.Add(crop.id, crop);
        }
    }
    /// <summary>
    /// private, init SeedIdDict with cropList
    /// </summary>
    void InitSeedIdDict(){
        foreach (var crop in cropList){
            SeedIdDict.Add(crop.id, crop.seed_id);
        }
    }
    /// <summary>
    /// private: Make an EnvFactor for a crop based on its factor type and standard environment value.
    /// This function is used to create the fertility, humidity, and light factors for each crop.
    /// </summary>
    public EnvFactor MakeEnvFactorForCrop(FactorType factor_type,
            float standard_env, float up_limit = ENV_DATA_UP_LIMIT, float down_limit = ENV_DATA_DOWN_LIMIT) {
        EnvFactor env_factor = null;
        switch (factor_type) {
            case FactorType.LinearEnv:
                env_factor = ScriptableObject.CreateInstance<LinearEnvFactor>();
                env_factor.standard_env = standard_env; env_factor.up_limit = up_limit; env_factor.down_limit = down_limit;
                break;
            case FactorType.LinearDiffEnv:
                env_factor = ScriptableObject.CreateInstance<LinearDiffEnvFactor>();
                env_factor.standard_env = standard_env; env_factor.up_limit = up_limit; env_factor.down_limit = down_limit;
                break;
            case FactorType.InRangeEnv:
                env_factor = ScriptableObject.CreateInstance<InRangeEnvFactor>();
                env_factor.standard_env = standard_env; env_factor.up_limit = up_limit; env_factor.down_limit = down_limit;
                break;
            default:
                Debug.LogError("CropManager.MakeEnvFactor: Invalid factor_type " + factor_type.ToString());
                break;
        }
        return env_factor;
    }
    /// <summary>
    /// private: Initialize the environment factor dictionaries for each crop.
    /// </summary>
    void InitEnvFactorDicts() {
        foreach (var crop in cropList) {
            fertilityFactorDict.Add(crop.id, MakeEnvFactorForCrop(crop.fertility_factor_type, crop.best_fertility));
            humidityFactorDict.Add(crop.id, MakeEnvFactorForCrop(crop.humidity_factor_type, crop.best_humidity));
            lightFactorDict.Add(crop.id, MakeEnvFactorForCrop(crop.light_factor_type, crop.best_light));
        }
        return;
    }
    //=========================================Public Function Part=======================================
    /// <summary>
    /// 获取指定id的Crop，ID有Dict定位速度快
    /// </summary>
    public Crop GetCrop(int id)
    {
        cropDict.TryGetValue(id, out Crop crop);
        if (crop != null)
        {
            return crop;
        }
        Debug.LogError("GetCrop收到无效id: " + id.ToString());
        return null;
    }
    /// <summary>
    /// 获取指定name的Crop，依靠遍历速度慢，不建议在高重复场景使用
    /// </summary>
    public Crop GetCrop(string name)
    {
        for (int i = 0; i < cropList.Count; i++)
        {
            if (cropList[i].name == name)
            {
                return cropList[i];
            }
        }
        Debug.LogError("GetCrop收到无效name: " + name.ToString());
        return null;
    }
    public Sprite GetSprite(int crop_id, int growth_stage) {
        // Safety check
        // TODO：若后续Crop的id不再按从0顺序不间断排列，则参数crop_id唯一合法标准是“合法CropID”，
        // 需要更新（或直接删除部分）SafetyCheck方法。
        if (crop_id >= CropSpritesCount || crop_id < 0)
        {
            UIManager.Instance.DebugTextAdd("[ERROR] Crop with id: " + crop_id + " beyond the legal range!");
            return null;
        }
        else if (growth_stage >= GROWTH_STAGE_COUNT || growth_stage < 0)
        {
            UIManager.Instance.DebugTextAdd("[ERROR] growth_stage: " + crop_id + " beyond the legal range!");
            return null;
        }

        Sprite tmp = CropSprites[crop_id * GROWTH_STAGE_COUNT + growth_stage];
        return tmp;
    }
}
