using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropManager : MonoBehaviour
{
    public static CropManager Instance { get; private set; }
    public class Crop{
        public int id;
        public string name;
        public Sprite texture;
    }
    public List<Crop> cropList = new List<Crop>();
    const int tempCropSpritesCount = 6;
    public Sprite[] tempCropSprites = new Sprite[tempCropSpritesCount];

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

    void InitCropListData(){
        cropList.Add(new Crop{id = 0, name="水稻"});
        cropList.Add(new Crop{id = 1, name="土豆"});
        cropList.Add(new Crop{id = 2, name="小麦"});
        cropList.Add(new Crop{id = 3, name="棉花"});
        cropList.Add(new Crop{id = 4, name="葡萄"});
        cropList.Add(new Crop{id = 5, name="树"});
    }
    
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
}
