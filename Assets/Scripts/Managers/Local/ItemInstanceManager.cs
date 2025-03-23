using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Tilemaps;

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
                "<<Error>> Initing the second ItemManager instance FAILED, becauese it's not allowed. ");
        }
    }

    //<代码源：3.24Update --cjh >
    public Tilemap landTilemap;
    public GameObject content;
    //</代码源：3.24Update --cjh >

    public GameObject itemInstance;

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
        public Vector3Int position;
        public GameObject instance;
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

    #region (0)
    /// <summary>
    /// 添加一个ItemInstance到列表中，并Instantiate
    /// </summary>
    /// <param name="new_ins"></param>
    public void initInstance(ItemInstance new_ins, Sprite texture){
        
        //TODO: new_ins.instance.transform

        //<代码源：3.24Update --cjh >
        Vector3 worldPosition = landTilemap.GetCellCenterWorld(new_ins.position);
        Vector3 localPosition = this.transform.InverseTransformPoint(worldPosition);

        new_ins.instance = Instantiate(itemInstance,this.transform); 
        new_ins.instance.transform.position = worldPosition;
        
        // 消除缩放影响
        //Vector3 managerScale = this.transform.lossyScale;
        Vector3 contentLossyScale = content.transform.lossyScale;
        Vector3 contentLocalScale = content.transform.localScale;
        Vector3 totalScale = new Vector3(
            contentLocalScale.x / contentLossyScale.x,
            contentLocalScale.y / contentLossyScale.y,
            contentLocalScale.z / contentLossyScale.z
        );

        new_ins.instance.transform.localScale = totalScale ;

        new_ins.instance.GetComponent<SpriteRenderer>().sprite = texture;
        
        new_ins.id = GetNewId();
        //</代码源：3.24Update --cjh >
    }
    #endregion

    #region (1)主要指定位置和模版
    public ItemInstance makeToolInstance(int item_id, Vector3Int position){
        ItemManager.Item sample = itemManager.GetItem(item_id, ItemManager.ItemType.Tool);
        if(sample == null){
            UIManager.Instance.DebugTextAdd(
               "<<Error>> Spawning ToolInstance FAILED, because the item_id for Tool is not found in ItemManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new ToolInstance{
            id=GetNewId(), type=ItemInstanceType.ToolInstance, item_id=item_id, position=position, 
            durability=((ItemManager.Tool)sample).max_durability
        };
        initInstance(new_ins, sample.texture);
        return new_ins;
    }
    public ItemInstance makeMaterialInstance(int item_id, Vector3Int position, int amount){
        ItemManager.Item sample = itemManager.GetItem(item_id, ItemManager.ItemType.Material);
        if(sample == null){
            UIManager.Instance.DebugTextAdd(
               "<<Error>> Spawning MaterialInstance FAILED: the item_id for Material is not found in ItemManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new MaterialInstance{
            id=GetNewId(), type=ItemInstanceType.MaterialInstance, item_id=item_id, position=position, 
            amount=amount
        };
        initInstance(new_ins, sample.texture);
        return new_ins;
    }
    public ItemInstance makeCropInstance(int crop_id, Vector3Int position){
        CropManager.Crop sample = cropManager.GetCrop(crop_id);
        if(sample == null){
            UIManager.Instance.DebugTextAdd(
               "<<Error>>Spawning an Cropinstance FAILED: the crop_id is not found in CropManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new CropInstance{
            id=GetNewId(), type=ItemInstanceType.CropInstance, item_id=crop_id, position=position, 
            growth_countdown=0  //sample.max_growth
        };
        initInstance(new_ins, sample.texture);
        return new_ins;
    }
    public ItemInstance makeBuildingInstance(int building_id, Vector3Int position){
        BuildManager.Building sample = buildManager.GetBuilding(building_id);
        if(sample == null){
            UIManager.Instance.DebugTextAdd(
               "<<Error>> Spawning an BuildingInstance FAILED: the building_id is not found in BuildManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new BuildingInstance{
            id=GetNewId(), type=ItemInstanceType.BuildingInstance, item_id=building_id, position=position, 
            durability=0        //sample.max_durability
        };
        initInstance(new_ins, sample.texture);
        return new_ins;
    }
    public ItemInstance makePrintInstance(int building_id, Vector3Int position){
        BuildManager.Building sample = buildManager.GetBuilding(building_id);
        if(sample == null){
            UIManager.Instance.DebugTextAdd(
               "<<Error>> Spawning an PrintInstance FAILED: the building_id is not found in BuildManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new PrintInstance{
            id=GetNewId(), type=ItemInstanceType.PrintItemInstance, item_id=building_id, position=position, 
            material_list=new Dictionary<int, PrintInstance.Progress>()
        };
        initInstance(new_ins, BuildManager.Instance.printSprite);
        // TODO: 从BuildManager中获取的Building应当提供蓝图所需材料列表
        // foreach (var it in sample.material_list){
        //    ((PrintInstance)new_ins).material_list.Add(it.Key, new PrintInstance.Progress{current=0, need=it.Value});
        // }
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
        //<代码源：3.24Update --cjh >

        // 同步尺寸
        GetComponent<RectTransform>().sizeDelta = content.GetComponent<RectTransform>().sizeDelta;

        //</代码源：3.24Update --cjh >
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
        ItemInstance new_ins = null;
        switch (type){
            case ItemInstanceType.ToolInstance:
                new_ins = makeToolInstance(sample_id, position);
                break;
            case ItemInstanceType.MaterialInstance:
                new_ins = makeMaterialInstance(sample_id, position, amount);
                break;
            case ItemInstanceType.CropInstance:
                new_ins = makeCropInstance(sample_id, position);
                break;
            case ItemInstanceType.BuildingInstance:
                new_ins = makeBuildingInstance(sample_id, position);
                break;
            case ItemInstanceType.PrintItemInstance:
                new_ins = makePrintInstance(sample_id, position);
                break;
            default:
                UIManager.Instance.DebugTextAdd(
                    "<<Error>>Spawning an ItemInstance FAILED: the type is not found in ItemInstanceManager. "
                );
                break;
        }
        if(new_ins != null){
            itemInstanceList.Add(new_ins);
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
