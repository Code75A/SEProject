using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.VersionControl;
using UnityEngine;

public class ItemInstanceManager : MonoBehaviour
{
    //======================================Global Reference Part====================================
    public static ItemInstanceManager Instance { get; private set; } // 单例
    public UIManager uiManager;
    public SLManager slManager;
    public PawnManager pawnManager;
    public ItemManager itemManager;
    public BuildManager buildManager;
    public CropManager cropManager;
    private void Awake(){
        // 实现单例模式，确保 ItemManager 只有一个实例
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
            UIManager.Instance.DebugTextAdd(
                "Error: Your operation of initing the second ItemManager instance FAILED, becauese it's not allowed. ");
        }
    }

    //====================================ItemInstance Class Part====================================
    public enum ItemInstanceType{
        ToolInstance, MaterialInstance, CropInstance, BuildingInstance, PrintItemInstance, Total
    }
    
    public class ItemInstance{
        public int id;
        public ItemInstanceType type;
        /// <summary>
        /// 本项为ItemInstance的模版id, 用于标记在ItemManager/CropManager/BuildManager中的对应的Item/Crop/Building
        /// </summary>
        public int item_id;
        public Vector2 position;
    }
    public class ToolInstance : ItemInstance{
        public int durability;
    }
    public class MaterialInstance : ItemInstance{
        public int amount;
    }
    public class CropInstance : ItemInstance{
        public int growth_countdown;
    }
    public class BuildingInstance : ItemInstance{
        public int durability;
    }
    public class PrintInstance : ItemInstance{
        public struct Progress{
            public int current;
            public int need;
        }
        /// <summary>
        /// 蓝图所需材料列表，key为材料的item_id，value为所需数量
        /// </summary>
        public Dictionary<int,Progress> material_list;
    }
    
    //======================================Manager Fuction Part=====================================
    //======================================Private Fuction Part=====================================
    #region ID生成和分发
    private static int ID_COUNTER = 0;
    private int GetNewId(){
        // TODO: Make sure the ID is unique and safe
        int new_id = ID_COUNTER;
        ID_COUNTER++;
        return new_id;
    }
    #endregion 
    // TODO: 在ItemInstance死亡时回收ID
    
    #region 用于创建各种ItemInstance的子函数

    #region (1)主要指定位置和模版
    private ItemInstance makeToolInstance(int item_id, Vector2 position){
        ItemManager.Item sample = itemManager.GetItem(item_id, ItemManager.ItemType.Tool);
        if(sample == null){
            UIManager.Instance.DebugTextAdd(
               "【Error】Spawning ToolInstance FAILED, because the item_id for Tool is not found in ItemManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new ToolInstance{
            id=GetNewId(), type=ItemInstanceType.ToolInstance, item_id=item_id, position=position, 
            durability=((ItemManager.Tool)sample).max_durability
        };
        if(new_ins != null){
            itemInstanceList.Add(new_ins);
        }
        return new_ins;
    }
    private ItemInstance makeMaterialInstance(int item_id, Vector2 position, int amount){
        ItemManager.Item sample = itemManager.GetItem(item_id, ItemManager.ItemType.Material);
        if(sample == null){
            UIManager.Instance.DebugTextAdd(
               "【Error】Spawning MaterialInstance FAILED: the item_id for Material is not found in ItemManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new MaterialInstance{
            id=GetNewId(), type=ItemInstanceType.MaterialInstance, item_id=item_id, position=position, 
            amount=amount
        };
        if(new_ins != null){
            itemInstanceList.Add(new_ins);
        }
        return new_ins;
    }
    private ItemInstance makeCropInstance(int crop_id, Vector2 position){
        // TODO3: CropManager应该有获得信息的接口
        CropManager.Crop sample = null; // (cropManager.Crop)cropManager.GetCrop(sample_id);
        if(sample == null){
            UIManager.Instance.DebugTextAdd(
               "【Error】Spawning an Cropinstance FAILED: the crop_id is not found in CropManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new CropInstance{
            id=GetNewId(), type=ItemInstanceType.CropInstance, item_id=crop_id, position=position, 
            growth_countdown=0  //sample.max_growth
        };
        if(new_ins != null){
            itemInstanceList.Add(new_ins);
        }
        return new_ins;
    }
    private ItemInstance makeBuildingInstance(int building_id, Vector2 position){
        // TODO1: BuildManager应该有获得信息的接口
        BuildManager.Building sample = null; // (buildManager.Building)buildManager.GetBuilding(sample_id);
        if(sample == null){
            UIManager.Instance.DebugTextAdd(
               "【Error】Spawning an BuildingInstance FAILED: the building_id is not found in BuildManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new BuildingInstance{
            id=GetNewId(), type=ItemInstanceType.BuildingInstance, item_id=building_id, position=position, 
            durability=0        //sample.max_durability
        };
        if(new_ins != null){
            itemInstanceList.Add(new_ins);
        }
        return new_ins;
    }
    private ItemInstance makePrintInstance(int building_id, Vector2 position){
        // TODO1: BuildManager应该有获得信息的接口
        BuildManager.Building sample = null; // (buildManager.Building)buildManager.GetBuilding(sample_id);
        if(sample == null){
            UIManager.Instance.DebugTextAdd(
               "【Error】Spawning an PrintInstance FAILED: the building_id is not found in BuildManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new PrintInstance{
            id=GetNewId(), type=ItemInstanceType.PrintItemInstance, item_id=building_id, position=position, 
            material_list=new Dictionary<int, PrintInstance.Progress>()
        };
        // TODO2: 从BuildManager中获取的Building应当提供蓝图所需材料列表
        // foreach (var it in sample.material_list){
        //    ((PrintInstance)new_ins).material_list.Add(it.Key, new PrintInstance.Progress{current=0, need=it.Value});
        // }
        if(new_ins != null){
            itemInstanceList.Add(new_ins);
        }
        return new_ins;
    }
    #endregion
    #region (2)PrintInstance转BuildInstance
    // TODO
    #endregion

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //======================================Public Fuction Part=======================================
    
    /// <summary>
    /// 在指定位置`position`生成一个指定`type`的ItemInstance
    /// </summary>
    /// <param name="sample_id">`id` for class `Item`,`Crop` or `Building` which you're going to make a instance </param>
    /// <param name="type"></param>
    /// <param name="amount">default 1, used for type like `Material`</param>
    /// <returns></returns>
    public ItemInstance SpawnItem(Vector3Int position, int sample_id, ItemInstanceType type, int amount=1){
        Vector2 pos = new Vector2(position.x, position.y);
        ItemInstance new_ins = null;
        switch (type){
            case ItemInstanceType.ToolInstance:
                new_ins = makeToolInstance(sample_id, pos);
                break;
            case ItemInstanceType.MaterialInstance:
                new_ins = makeMaterialInstance(sample_id, pos, amount);
                break;
            case ItemInstanceType.CropInstance:
                new_ins = makeCropInstance(sample_id, pos);
                break;
            case ItemInstanceType.BuildingInstance:
                new_ins = makeBuildingInstance(sample_id, pos);
                break;
            case ItemInstanceType.PrintItemInstance:
                new_ins = makePrintInstance(sample_id, pos);
                break;
            default:
                UIManager.Instance.DebugTextAdd(
                    "【Error】Spawning an ItemInstance FAILED: the type is not found in ItemInstanceManager. "
                );
                break;
        }
        return new_ins;
    }

    public List<ItemInstance> itemInstanceList = new List<ItemInstance>();
    public ItemInstance GetInstance(int iteminstance_id){
        ItemInstance aim_ins = itemInstanceList.Find(c => c.id == iteminstance_id);
        if(aim_ins is null){
            UIManager.Instance.DebugTextAdd(
                "[Log]Getting Item FAILED: the iteminstance_id "+ iteminstance_id +" is not found in ItemInstanceManager. "
            );
        }
        return aim_ins;
    }

}
