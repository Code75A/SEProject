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
    //=========================================Env Factor Part=======================================
    public enum EnvFactorType{
        Linear, DiffLinear, Total
    }
    public class EnvFactor{
        public virtual float GetImpacted(float origin, float env_data){
            return origin;
        }
    }
    public class LinearFactor:EnvFactor{
        public override float GetImpacted(float origin, float env_data){
            return origin*env_data;
        }
    }
    public class DiffLinearFactor:EnvFactor{
        public float expect_env;
        public override float GetImpacted(float origin, float env_data){
            //TODO: ensure the calculation logic!!!
            return origin*(1.0f - (Math.Abs(expect_env-env_data)/expect_env));
        }
    }
    // =====================================Crop Part==========================================
    public class Crop{
        public int id;
        public string name;
        public float lifetime;
        // public EnvFactor fertility_factor;
        // public EnvFactor humidity_factor;
        // public EnvFactor light_factor;
        public List<KeyValuePair<int, int> > harvest_list;
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
    public float GetRealLifetime(Crop crop, MapManager.MapData env_data){
        float real_lifetime =  crop.lifetime;
        real_lifetime = real_lifetime * env_data.fertility * env_data.light * Math.Abs(1.0f-env_data.humidity);
        // TODO: UseFactor to enable different require for different crop
        return real_lifetime;
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
