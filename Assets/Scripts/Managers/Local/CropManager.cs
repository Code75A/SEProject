using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;

public class CropManager : MonoBehaviour
{
    public static CropManager Instance { get; private set; }
    const int GROWTH_STAGE_COUNT = 4;
    public class Crop{
        public int id;
        public string name;
        //public Sprite[] texture;
        public float lifetime;
    }
    public List<Crop> cropList = new List<Crop>();
    const int CropSpritesCount = 6;
    public Sprite[,] CropSprites = new Sprite[CropSpritesCount,GROWTH_STAGE_COUNT];

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

    void Start(){
        InitCropListData();
    }
    //=========================================Private Function Part=======================================
    void InitCropListData(){
        cropList.Add(new Crop{id = 0, name="水稻", lifetime=10.0f});
        cropList.Add(new Crop{id = 1, name="土豆", lifetime=5.0f});
        cropList.Add(new Crop{id = 2, name="小麦", lifetime=10.0f});
        cropList.Add(new Crop{id = 3, name="棉花", lifetime=10.0f});
        cropList.Add(new Crop{id = 4, name="葡萄", lifetime=15.0f});
        cropList.Add(new Crop{id = 5, name="树", lifetime=20.0f});
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
    public float GetRealLifetime(Crop crop, float temp_rate=1.0f){
        // TODO: float GetRealLifetime(Crop crop, ... (Message from Map)){
        return crop.lifetime * temp_rate;
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

        Sprite tmp = CropSprites[crop_id,growth_stage];
        return tmp;
    }
}
