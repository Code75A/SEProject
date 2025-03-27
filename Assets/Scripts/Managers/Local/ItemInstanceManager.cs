using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemInstanceManager : MonoBehaviour
{
    public bool ISDEBUGMODE = true;
    //======================================Global Reference Part====================================
    public static ItemInstanceManager Instance { get; private set; } // 单例
    public UIManager uiManager;
    public SLManager slManager;
    public PawnManager pawnManager;
    public ItemManager itemManager;
    public BuildManager buildManager;
    public CropManager cropManager;
    public MapManager mapManager;
    private void Awake(){
        // 实现单例模式，确保 ItemManager 只有一个实例
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
            UIManager.Instance.DebugTextAdd(
                "<<Error>> Initing the second ItemManager instance FAILED, because it's not allowed. ");
        }
        ISDEBUGMODE = true;
    }

    public Tilemap landTilemap;
    public GameObject content;

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
        public List<KeyValuePair<int,Progress> > material_list;
    }
    
    //======================================Manager Function Part=====================================
    //======================================Private Function Part=====================================
    #region 1.唯一ID生成管理
    private static int ID_COUNTER = 0;
    /// <summary>
    /// 唯一ID生成器，用于为ItemInstance分配唯一ID。
    /// 我们约定它只被‘InitInstance’调用，以确保ID的唯一性。
    /// </summary>
    private int GetNewId(){
        // TODO: Make sure the ID is unique and safe
        int new_id = ID_COUNTER;
        ID_COUNTER++;
        
        UIManager.Instance.DebugTextAdd(
            "[Log] You are getting new id: "+ new_id +". "
        );
        return new_id;
    }
    private static void RecycleId(int id){
        // TODO: 在ItemInstance死亡时回收ID
        UIManager.Instance.DebugTextAdd(
            "[Log] You are recycling id: "+ id + ". " +
            "<<Warning>> But `RecycleId` is not implemented yet. "
        );
        return;
    }
    #endregion 
    
    #region 2.创建各种ItemInstance个体的子函数

    #region (0)
    /// <summary>
    /// 为填充了基本后端信息的ItemInstance分配唯一ID；
    /// 为其instance成员变量装载预制体，设置transform组件（包括position和scale），加载材质；
    /// 是创建合法ItemInstance的唯一工具。
    /// </summary>
    /// <param name="new_ins">待初始化的ItemInstance，应当被填充基本后端信息</param>I
    public void InitInstance(ItemInstance new_ins, Sprite texture){
        // 设置transform组件
        //  装载预制体
        new_ins.instance = Instantiate(itemInstance,this.transform); 
        //  设置位置
        Vector3 worldPosition = landTilemap.GetCellCenterWorld(new_ins.position);
        Vector3 localPosition = this.transform.InverseTransformPoint(worldPosition);
        new_ins.instance.transform.position = worldPosition;
        //  消除缩放影响
        //Vector3 managerScale = this.transform.lossyScale;
        Vector3 contentLossyScale = content.transform.lossyScale;
        Vector3 contentLocalScale = content.transform.localScale;
        Vector3 totalScale = new Vector3(
            contentLocalScale.x / contentLossyScale.x,
            contentLocalScale.y / contentLossyScale.y,
            contentLocalScale.z / contentLossyScale.z
        );
        new_ins.instance.transform.localScale = totalScale ;

        // 加载材质
        new_ins.instance.GetComponent<SpriteRenderer>().sprite = texture;
        
        // 分配唯一ID
        new_ins.id = GetNewId();
    }

    //
    #endregion

    #region (1)主要指定位置和模版
    public ItemInstance MakeToolInstance(int item_id, Vector3Int position){
        ItemManager.Item sample = itemManager.GetItem(item_id, ItemManager.ItemType.Tool);
        if(sample == null){
            UIManager.Instance.DebugTextAdd(
               "<<Error>> Spawning ToolInstance FAILED, because the item_id for Tool is not found in ItemManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new ToolInstance{
            id=-1, type=ItemInstanceType.ToolInstance, item_id=item_id, position=position, 
            durability=((ItemManager.Tool)sample).max_durability
        };
        InitInstance(new_ins, sample.texture);
        return new_ins;
    }
    public ItemInstance MakeMaterialInstance(int item_id, Vector3Int position, int amount){
        ItemManager.Item sample = itemManager.GetItem(item_id, ItemManager.ItemType.Material);
        if(sample == null){
            UIManager.Instance.DebugTextAdd(
               "<<Error>> Spawning MaterialInstance FAILED: the item_id for Material is not found in ItemManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new MaterialInstance{
            id=-1, type=ItemInstanceType.MaterialInstance, item_id=item_id, position=position, 
            amount=amount
        };
        InitInstance(new_ins, sample.texture);
        return new_ins;
    }
    public ItemInstance MakeCropInstance(int crop_id, Vector3Int position){
        CropManager.Crop sample = cropManager.GetCrop(crop_id);
        if(sample == null){
            UIManager.Instance.DebugTextAdd(
               "<<Error>>Spawning an Cropinstance FAILED: the crop_id is not found in CropManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new CropInstance{
            id=-1, type=ItemInstanceType.CropInstance, item_id=crop_id, position=position, 
            growth_countdown=0  //sample.max_growth
        };
        InitInstance(new_ins, sample.texture);
        return new_ins;
    }
    public ItemInstance MakeBuildingInstance(int building_id, Vector3Int position){
        BuildManager.Building sample = buildManager.GetBuilding(building_id);
        if(sample == null){
            UIManager.Instance.DebugTextAdd(
               "<<Error>> Spawning an BuildingInstance FAILED: the building_id is not found in BuildManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new BuildingInstance{
            id=-1, type=ItemInstanceType.BuildingInstance, item_id=building_id, position=position, 
            durability=0        //sample.max_durability
        };
        InitInstance(new_ins, sample.texture);
        return new_ins;
    }
    public ItemInstance MakePrintInstance(int building_id, Vector3Int position){
        BuildManager.Building sample = buildManager.GetBuilding(building_id);
        if(sample == null){
            UIManager.Instance.DebugTextAdd(
               "<<Error>> Spawning an PrintInstance FAILED: the building_id is not found in BuildManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        List<KeyValuePair<int,PrintInstance.Progress> > temp =new List<KeyValuePair<int, PrintInstance.Progress> >();
        foreach (KeyValuePair<int,int> it in sample.material_list){
            // <DEBUG>
            //temp.Add(new KeyValuePair<int, PrintInstance.Progress>(it.Key, new PrintInstance.Progress{current=10,need=20}));
            temp.Add(new KeyValuePair<int, PrintInstance.Progress>(it.Key, new PrintInstance.Progress{current=0,need=it.Value}));
            // </DEBUG>
        }
        new_ins = new PrintInstance{
            id=-1, type=ItemInstanceType.PrintItemInstance, item_id=building_id, position=position, 
            material_list=new List<KeyValuePair<int, PrintInstance.Progress> >()
        };
        InitInstance(new_ins, BuildManager.Instance.printSprite);
        // TODO: 从BuildManager中获取的Building应当提供蓝图所需材料列表
        return new_ins;
    }
    #endregion
    
    #region (2)PrintInstance转BuildInstance
    // TODO
    #endregion

    #endregion

    #region 3.销毁各种ItemInstance个体的子函数
    /// <summary>
    /// 用于确认DestroyItem函数的销毁模式。
    /// Hard表示摧毁并产生部分遗留物；Soft表示摧毁但不产生任何遗留物；Middle表示有部分遗留物，配合remain_rate使用。
    /// </summary>
    public enum DestroyMode{
        Hard, Soft, Middle, Total
    }
    public void DestroyInstance(ItemInstance aim_ins, DestroyMode mode, float remain_rate){
        GameObject.Destroy(aim_ins.instance);
        aim_ins.instance = null;
        switch(aim_ins.type){
            case ItemInstanceType.ToolInstance:
            case ItemInstanceType.MaterialInstance:
            case ItemInstanceType.CropInstance:     
            case ItemInstanceType.BuildingInstance:
                break;
            case ItemInstanceType.PrintItemInstance:
                ClearPrintInstance(aim_ins, mode, remain_rate);
                break;
            default:
                UIManager.Instance.DebugTextAdd(
                    "<<Error>>Destroying Item FAILED: the type is not found in ItemInstanceManager. "
                );
                break;
        }
        return;
    }
    public void ClearPrintInstance(ItemInstance aim_ins, DestroyMode mode, float remain_rate){
        if(aim_ins.type != ItemInstanceType.PrintItemInstance){
            UIManager.Instance.DebugTextAdd(
                "<<Error>>Clearing PrintInstance FAILED: the iteminstance_id "+ aim_ins.id +" is not a PrintInstance. "
            );
            return;
        }
        if(mode == DestroyMode.Soft){
            ;
        }
        else if (mode == DestroyMode.Hard || mode == DestroyMode.Middle){
            List<KeyValuePair<int,int> > temp = new List<KeyValuePair<int,int> >();
            int item_id, amount;
            foreach (KeyValuePair<int,PrintInstance.Progress> it in ((PrintInstance)aim_ins).material_list){
                item_id = it.Key;
                amount = it.Value.current;
                if(mode == DestroyMode.Middle) amount = (int)System.Math.Truncate((double)(amount*remain_rate));
                temp.Add(new KeyValuePair<int, int>(item_id, amount));
            }
            int set_res = mapManager.SetMaterial(aim_ins.position, temp);
            UIManager.Instance.DebugTextAdd("[Log]From mapManager get set-material-result: "+ set_res + ". ");
        }
        else{
            UIManager.Instance.DebugTextAdd("<<Error>>Clearing PrintInstance FAILED: UnDefined DestroyMode. ");
        }
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // 同步尺寸
        GetComponent<RectTransform>().sizeDelta = content.GetComponent<RectTransform>().sizeDelta;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //======================================Public Function Part=======================================
    
    /// <summary>
    /// 在指定位置`position`生成一个指定`type`的ItemInstance
    /// </summary>
    /// <param name="sample_id">`id` for class `Item`,`Crop` or `Building` which you're going to make a instance </param>
    /// <param name="amount">default 1, used for type like `Material`</param>
    public ItemInstance SpawnItem(Vector3Int position, int sample_id, ItemInstanceType type, int amount=1){
        ItemInstance new_ins = null;
        switch (type){
            case ItemInstanceType.ToolInstance:
                new_ins = MakeToolInstance(sample_id, position);
                break;
            case ItemInstanceType.MaterialInstance:
                new_ins = MakeMaterialInstance(sample_id, position, amount);
                break;
            case ItemInstanceType.CropInstance:
                new_ins = MakeCropInstance(sample_id, position);
                break;
            case ItemInstanceType.BuildingInstance:
                new_ins = MakeBuildingInstance(sample_id, position);
                break;
            case ItemInstanceType.PrintItemInstance:
                new_ins = MakePrintInstance(sample_id, position);
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
    
    /// <summary>
    /// 销毁指定的ItemInstance
    /// </summary>
    /// <param name="remain_rate">遗留物的生成率，只在destroy_mode==DestroyMode.Middle时有效</param>
    public void DestroyItem(ItemInstance aim_ins, DestroyMode destroy_mode=DestroyMode.Hard, float remain_rate=0.5f){
        // 检查该Instance是否存在
        if(GetInstance(aim_ins.id) == null){
            UIManager.Instance.DebugTextAdd(
                "<<Error>>Destroying Item FAILED: the iteminstance_id "+ aim_ins.id +" is not found in ItemInstanceManager. "
            );
            return;
        }
        // 进行销毁
        //  从ItemInstanceList中删除
        itemInstanceList.Remove(aim_ins);
        //  与InitInstance对应 销毁GameObject
        DestroyInstance(aim_ins, destroy_mode, remain_rate);
        //  回收ID
        RecycleId(aim_ins.id);
        return;
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
