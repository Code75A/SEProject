
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemInstanceManager : MonoBehaviour
{
    public bool ISDEBUGMODE = true;
    //======================================Global Reference Part====================================
    public static ItemInstanceManager Instance { get; private set; } // 单例
    private void Awake()
    {
        // 实现单例模式，确保 ItemManager 只有一个实例
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
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
    public enum ItemInstanceType
    {
        ToolInstance, MaterialInstance, CropInstance, BuildingInstance, PrintInstance, ResourceInstance, Total
    }

    #region InsLabel类定义
    //====================================InsLabel Class Part====================================
    public class InsLabel
    {
        public ItemInstanceType type;
        public int item_id;
        public bool can_merge;
    }
    public class ToolLabel : InsLabel
    {
        public int durability;
        public int GetDurability() { return durability; }
        public void SetDurability(int new_d) { durability = new_d; }
        /// <summary>
        /// 工具使用时发生磨损,注意！并非指装备上该Tool！
        /// </summary>
        public bool Wear()
        {
            if (durability <= 0) return false;
            else
            {
                durability--;
                return true;
            }
        }
        public bool IsBroken()
        {
            return durability <= 0;
        }
    }
    public class MaterialLabel : InsLabel
    {
        public int amount;
        public int GetAmount() { return amount; }
        public void SetAmount(int new_amount) { amount = new_amount; }
    }
    //public class CropLabel:InsLabel{}
    //public class BuildingLabel:InsLabel{}
    //public class PrintLabel:InsLabel{}
    /// <summary>
    /// 确认两个InsLabel是否可以合并。
    /// </summary>
    public bool CanMerge(InsLabel label1, InsLabel label2)
    {
        if (label1.type == label2.type
            && label1.item_id == label2.item_id
            && label1.can_merge
            && label2.can_merge)
            return true;
        return false;
    }
    /// <summary>
    /// 获取可以代表一个实体特性的InsLabel，不会对实体本身进行修改。
    /// </summary>
    public InsLabel GetLabel(ItemInstance ins)
    {
        InsLabel new_label = null;
        switch (ins.type)
        {
            case ItemInstanceType.ToolInstance:
                new_label = new ToolLabel
                {
                    type = ins.type,
                    item_id = ins.item_id,
                    can_merge = false,
                    durability = ((ToolInstance)ins).durability
                };
                break;
            case ItemInstanceType.MaterialInstance:
                new_label = new MaterialLabel
                {
                    type = ins.type,
                    item_id = ins.item_id,
                    can_merge = true,
                    amount = ((MaterialInstance)ins).amount
                };
                break;
            default:
                UIManager.Instance.DebugTextAdd(
                    "<<Error>> Getting Label FAILED: ins with type " + ins.type.ToString() + " can not be transformed to a InsLabel. "
                );
                break;
        }
        return new_label;
    }
    #endregion
    #region ItemInstance类定义
    public class ItemInstance
    {
        public int id;
        public ItemInstanceType type;
        /// <summary>
        /// 本项为ItemInstance的模版id, 用于标记在ItemManager/CropManager/BuildManager中的对应的Item/Crop/Building
        /// </summary>
        public int item_id;
        public Vector3Int position;
        public GameObject instance;

        //--------------------------------private------------------------------------

        /// <summary>
        /// private:设置Instance贴图
        /// </summary>
        public void SetSprite(Sprite sprite)
        {
            instance.GetComponent<SpriteRenderer>().sprite = sprite;
            return;
        }
        /// <summary>
        /// private:设置显示文本
        /// </summary>
        public void SetText(string text)
        {
            instance.GetComponentInChildren<TextMeshPro>().text = text;
            return;
        }
        /// <summary>
        /// private:获取显示文本
        /// </summary>
        public string GetText()
        {
            return instance.GetComponentInChildren<TextMeshPro>().text;
        }
        //--------------------------------public------------------------------------

        /// <summary>
        /// 获取自身作为ItemInstance（实体）的id
        /// </summary>
        public int GetId() { return id; }
        /// <summary>
        /// 获取自身所属的Item模板（物品种类）的id
        /// </summary>
        public int GetModelId() { return item_id; }
        public Vector3Int GetPosition() { return position; }
    }
    public class ToolInstance : ItemInstance
    {
        public int durability;

        //--------------------------------private------------------------------------
        
        //--------------------------------public------------------------------------
        public int GetDurability() { return durability; }
    }
    public class MaterialInstance : ItemInstance
    {
        public int amount;

        //--------------------------------private------------------------------------
        //--------------------------------public------------------------------------
        /// <summary>
        /// 获取物品实体的堆叠数量。
        /// </summary>
        public int GetAmount()
        {
            return amount;
        }
        /// <summary>
        /// 设置物品实体的堆叠数量。
        /// 注意：所有对可堆叠物品实体的堆叠数量的修改请一定要通过此修改函数！
        /// 否则会出现贴图错误以及其他统计数据错误。
        /// </summary>
        public void SetAmount(int new_amount)
        {
            StorageManager.Instance.AddItem(item_id, new_amount-amount);

            amount = new_amount;
            string old_text = GetText();
            string[] strArray = old_text.Split('|');
            string new_text = strArray[0] + "|" + new_amount.ToString();
            SetText(new_text);

            return;
        }
    }
    public class CropInstance : ItemInstance
    {
        public float growth;
        public float real_lifetime;
        public float growth_per_frame;
        public List<KeyValuePair<int, int>> harvest_list; // 指向模板的收获列表
        //TODO: 增加Dictionary
        //--------------------------------public------------------------------------
        public bool IsMature() { return growth >= real_lifetime; }
    }
    public class BuildingInstance : ItemInstance
    {
        public int durability;
        public ItemInstance content; // 用于存放建筑内部的物品实例
        public List<KeyValuePair<int, int>> material_list;   // 指向模板的建筑材料列表
    }
    public class PrintInstance : ItemInstance
    {
        public struct Progress
        {
            public int current;
            public int need;
        }
        /// <summary>
        /// 蓝图所需材料列表，key为材料的item_id，value为所需数量
        /// </summary>
        public List<KeyValuePair<int, Progress>> material_list;

        //--------------------------------public------------------------------------
        public bool IsFinished()
        {
            for (int i = 0; i < material_list.Count; i++)
            {
                if (material_list[i].Value.current < material_list[i].Value.need)
                {
                    return false;
                }
            }
            return true;
        }
        public bool PushProgress(int item_id, int amount)
        {
            // Copilot: 这段代码的作用是更新材料列表中的进度
            for (int i = 0; i < material_list.Count; i++)
            {
                if (material_list[i].Key == item_id)
                {
                    material_list[i] = new KeyValuePair<int, Progress>(item_id,
                        new Progress
                        {
                            current = material_list[i].Value.current + amount,
                            need = material_list[i].Value.need
                        });
                    return true;
                }
            }
            return false;
        }
        public int GetRequirement(int item_id)
        {
            // Copilot: 这段代码的作用是获取材料列表中指定材料的仍待满足的需求量
            for (int i = 0; i < material_list.Count; i++)
            {
                if (material_list[i].Key == item_id)
                {
                    return material_list[i].Value.need - material_list[i].Value.current;
                }
            }
            return 0;
        }
        public KeyValuePair<int,int> GetOneRequirement()
        {
            // Copilot: 这段代码的作用是获取材料列表中某一项材料的仍待满足的需求量
            for (int i = 0; i < material_list.Count; i++)
            {
                if (material_list[i].Value.current < material_list[i].Value.need)
                {
                    return new KeyValuePair<int, int>(material_list[i].Key,
                        material_list[i].Value.need - material_list[i].Value.current);
                }
            }
            return new KeyValuePair<int, int>(-1, -1);
        }
        public Dictionary<int, int> GetAllRequirements()
        {
            // Copilot: 这段代码的作用是获取所有材料的仍待满足的需求量
            Dictionary<int, int> requirements = new Dictionary<int, int>();
            for (int i = 0; i < material_list.Count; i++)
            {
                if (material_list[i].Value.current >= material_list[i].Value.need) continue;
                requirements[material_list[i].Key] = material_list[i].Value.need - material_list[i].Value.current;
            }
            return requirements;
        }
    }
    public class ResourceInstance : ItemInstance
    {
        public int durability;
        public List<KeyValuePair<int, int>> resource_list = new List<KeyValuePair<int, int>>()
        {new KeyValuePair<int, int>(6,50) }; // 指向模板的资源列表
        //TODO: 增加Dictionary
    }
    #endregion

    //======================================Manager Function Part=====================================
    //======================================Private Function Part=====================================
    #region 1.唯一ID生成管理
    private static int ID_COUNTER = 0;
    /// <summary>
    /// private：唯一ID生成器，用于为ItemInstance分配唯一ID。
    /// 我们约定它只被‘SpawnItem’调用，以确保ID的唯一性。
    /// TODO：目前仍然是最简单的增量分配，需要根据后续决定的ID管理机制实现正式的ID分配方法
    /// </summary>
    private int GetNewId()
    {
        // TODO: Make sure the ID is unique and safe
        int new_id = ID_COUNTER;
        ID_COUNTER++;

        UIManager.Instance.DebugTextAdd(
            "[Log] You are getting new id: " + new_id + ". "
        );
        return new_id;
    }
    /// <summary>
    /// private：唯一ID回收器，用于回收ItemInstance的ID。
    /// 我们约定它只被‘DestroyItem’调用，以确保回收操作的唯一性。
    /// TODO：目前仍然是象征性的回收，需要根据后续决定的ID管理机制实现正式的回收工作
    /// </summary>
    private static void RecycleId(int id)
    {
        // TODO: 在ItemInstance死亡时回收ID
        UIManager.Instance.DebugTextAdd(
            "[Log] You are recycling id: " + id + ". " +
            "<<Warning>> But `RecycleId` is not implemented yet. "
        );
        return;
    }
    #endregion

    #region 2.创建各种ItemInstance个体的子函数

    #region (0) 共用的InitInstance函数, 用来为ItemInstance.instance(GameObject)做初始化
    /// <summary>
    /// private：为其instance成员变量装载预制体，设置transform组件（包括position和scale），加载材质；
    /// </summary>
    /// <param name="new_ins">待初始化的ItemInstance，应当被填充基本后端信息</param>I
    public void InitInstance(ItemInstance new_ins, Sprite texture)
    {
        // 设置transform组件
        //  装载预制体
        new_ins.instance = Instantiate(itemInstance, this.transform);
        //  设置位置
        Vector3 worldPosition = landTilemap.GetCellCenterWorld(new_ins.position);
        new_ins.instance.transform.position = worldPosition;
        //  消除缩放影响
        Vector3 contentLossyScale = content.transform.lossyScale;
        Vector3 contentLocalScale = content.transform.localScale;
        Vector3 totalScale = new Vector3(
            contentLocalScale.x / contentLossyScale.x,
            contentLocalScale.y / contentLossyScale.y,
            contentLocalScale.z / contentLossyScale.z
        );
        new_ins.instance.transform.localScale = totalScale;

        // 加载材质
        new_ins.SetSprite(texture);
    }

    #endregion

    #region (1)主要指定位置和模版
    /// <summary>
    /// private
    /// </summary>
    public ItemInstance MakeToolInstance(int item_id, Vector3Int position)
    {
        ItemManager.Item sample = ItemManager.Instance.GetItem(item_id, ItemManager.ItemType.Tool);
        if (sample == null)
        {
            UIManager.Instance.DebugTextAdd(
               "<<Error>> Spawning ToolInstance FAILED, because the item_id for Tool is not found in ItemManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new ToolInstance
        {
            id = -1,
            type = ItemInstanceType.ToolInstance,
            item_id = item_id,
            position = position,
            durability = ((ItemManager.Tool)sample).max_durability
        };
        InitInstance(new_ins, sample.texture);
        new_ins.SetText(sample.name);
        return new_ins;
    }
    /// <summary>
    /// private
    /// </summary>
    public ItemInstance MakeMaterialInstance(int item_id, Vector3Int position, int amount)
    {
        ItemManager.Item sample = ItemManager.Instance.GetItem(item_id, ItemManager.ItemType.Material);
        if (sample == null)
        {
            UIManager.Instance.DebugTextAdd(
               "<<Error>> Spawning MaterialInstance FAILED: the item_id for Material is not found in ItemManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new MaterialInstance
        {
            id = -1,
            type = ItemInstanceType.MaterialInstance,
            item_id = item_id,
            position = position,
            amount = amount
        };
        InitInstance(new_ins, sample.texture);
        new_ins.SetText(sample.name + "|" + amount.ToString());
        return new_ins;
    }
    /// <summary>
    /// private
    /// </summary>
    public ItemInstance MakeCropInstance(int crop_id, Vector3Int position)
    {
        Crop sample = CropManager.Instance.GetCrop(crop_id);
        MapManager.MapData env_data = MapManager.Instance.GetMapData(position);

        if (sample == null)
        {
            UIManager.Instance.DebugTextAdd(
               "<<Error>>Spawning an CropInstance FAILED: the crop_id is not found in CropManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new CropInstance
        {
            id = -1,
            type = ItemInstanceType.CropInstance,
            item_id = crop_id,
            position = position,
            growth = 0,
            real_lifetime = CropManager.Instance.GetRealLifetime(sample.id, env_data),
            growth_per_frame = CropManager.Instance.GetGrowthPerFrame(crop_id),
            harvest_list = sample.harvest_list // 指向模板的收获列表
        };
        InitInstance(new_ins, CropManager.Instance.GetSprite(crop_id, 0));
        new_ins.SetText(sample.name);
        return new_ins;
    }
    /// <summary>
    /// private
    /// </summary>
    public ItemInstance MakeBuildingInstance(int building_id, Vector3Int position)
    {
        Building sample = BuildManager.Instance.GetBuilding(building_id);
        if (sample == null)
        {
            UIManager.Instance.DebugTextAdd(
               "<<Error>> Spawning an BuildingInstance FAILED: the building_id is not found in BuildManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        new_ins = new BuildingInstance
        {
            id = -1,
            type = ItemInstanceType.BuildingInstance,
            item_id = building_id,
            position = position,
            durability = sample.durability,
            content = null, // 初始化内容为空
            material_list = sample.material_list // 指向建筑所需材料列表
        };
        InitInstance(new_ins, sample.texture);
        new_ins.SetText(sample.build_name);
        return new_ins;
    }
    /// <summary>
    /// private
    /// </summary>
    public ItemInstance MakePrintInstance(int building_id, Vector3Int position)
    {
        Building sample = BuildManager.Instance.GetBuilding(building_id);
        if (sample == null)
        {
            UIManager.Instance.DebugTextAdd(
               "<<Error>> Spawning an PrintInstance FAILED: the building_id is not found in BuildManager. "
            );
            return null;
        }
        ItemInstance new_ins;
        List<KeyValuePair<int, PrintInstance.Progress>> temp = new List<KeyValuePair<int, PrintInstance.Progress>>();
        foreach (KeyValuePair<int, int> it in sample.material_list)
        {
            // <DEBUG>
            //temp.Add(new KeyValuePair<int, PrintInstance.Progress>(it.Key, new PrintInstance.Progress{current=10,need=20}));
            temp.Add(new KeyValuePair<int, PrintInstance.Progress>(it.Key, new PrintInstance.Progress { current = 0, need = it.Value }));
            // </DEBUG>
        }
        new_ins = new PrintInstance
        {
            id = -1,
            type = ItemInstanceType.PrintInstance,
            item_id = building_id,
            position = position,
            material_list = temp
        };
        InitInstance(new_ins, BuildManager.Instance.printSprite);
        new_ins.SetText(sample.build_name);
        return new_ins;
    }
    public ItemInstance MakeResourceInstance(int resource_id, Vector3Int position){
        ItemManager.Item sample = ItemManager.Instance.GetItem(resource_id, ItemManager.ItemType.Resource);
        if (sample == null){
            UIManager.Instance.DebugTextAdd(
               "<<Error>> Spawning ResourceInstance FAILED: the resource_id for Resource is not found in ItemManager. "
            );

            return null;
        }
        ItemInstance new_ins;
        new_ins = new ResourceInstance{
            id = -1,
            type = ItemInstanceType.ResourceInstance,
            item_id = resource_id,
            position = position,
            durability = (sample as ItemManager.Resource).max_durability
        };
        InitInstance(new_ins, sample.texture);
        new_ins.SetText(sample.name);
        return new_ins;
    }
    #endregion

    #region (2)PrintInstance转BuildInstance

    // public void PrintToBuild(ItemInstance aim_ins)
    // {
    //     if (aim_ins.type != ItemInstanceType.PrintInstance)
    //     {
    //         UIManager.Instance.DebugTextAdd(
    //             "<<Error>>PrintToBuild FAILED: the iteminstance_id " + aim_ins.id + " is not a PrintInstance. "
    //         );
    //         return;
    //     }

    //     //
    // }

    #endregion

    #endregion

    #region 3.销毁各种ItemInstance个体的子函数, 以及用于销毁模式控制的控制量定义
    /// <summary>
    /// 用于确认DestroyItem函数的销毁模式。
    /// RemainNone表示不产生任何遗留物
    /// RemainAll表示产生最大数量的遗留物
    /// RemainWithRate表示产生部分遗留物，配合remain_rate使用。
    /// </summary>
    public enum DestroyMode
    {
        RemainAll, RemainNone, RemainWithRate, Total
    }
    #region (0) 共用的DestroyInstance函数, 用来销毁ItemInstance.instance(这是一个GameObject)
    /// <summary>
    /// private
    /// </summary>
    public void DestroyInstance(ItemInstance aim_ins)
    {
        GameObject.Destroy(aim_ins.instance);
        aim_ins.instance = null;
        return;
    }
    #endregion

    #region (1) 用于清除各种Instance的内容物
    /// <summary>
    /// private
    /// </summary>
    public void ClearPrintInstance(ItemInstance aim_ins, DestroyMode mode, float remain_rate){
        // Safety check
        if (aim_ins.type != ItemInstanceType.PrintInstance){
            UIManager.Instance.DebugTextAdd(
                "<<Error>>Clearing PrintInstance FAILED: the iteminstance_id " + aim_ins.id + " is not a PrintInstance. "
            );
            return;
        }

        if (mode == DestroyMode.RemainNone) {
            // 不产生任何遗留物
        }
        else if (mode == DestroyMode.RemainAll || mode == DestroyMode.RemainWithRate){
            List<KeyValuePair<int, int>> temp = new List<KeyValuePair<int, int>>();
            int item_id, amount;
            foreach (KeyValuePair<int, PrintInstance.Progress> it in ((PrintInstance)aim_ins).material_list){
                item_id = it.Key;
                amount = it.Value.current;

                if (mode == DestroyMode.RemainWithRate) amount = (int)System.Math.Truncate((double)(amount * remain_rate));

                if (amount > 0)
                    temp.Add(new KeyValuePair<int, int>(item_id, amount));
            }
            int set_res = MapManager.Instance.SetMaterial(aim_ins.position, temp);
            UIManager.Instance.DebugTextAdd("[Log]From mapManager get set-material-result: " + set_res + ". ");
        }
        else{
            UIManager.Instance.DebugTextAdd(
                "<<Warning>>ClearPrintInstance has not implement with DestroyMode: " + mode.ToString() + ". "
            );
        }
        return;
    }
    public void ClearCropInstance(ItemInstance aim_ins, DestroyMode mode, float remain_rate){
        // Safety check
        if (aim_ins.type != ItemInstanceType.CropInstance){
            UIManager.Instance.DebugTextAdd(
                "<<Error>>Clearing CropInstance FAILED: the iteminstance_id " + aim_ins.id + " is not a CropInstance. "
            );
            return;
        }

        if (mode == DestroyMode.RemainNone) {
            // 不产生任何遗留物
        }
        else if (mode == DestroyMode.RemainAll) {
            List<KeyValuePair<int, int>> sample = CropManager.Instance.GetCrop(aim_ins.GetModelId()).harvest_list.ToList();
            int set_res = MapManager.Instance.SetMaterial(aim_ins.position, sample);
            UIManager.Instance.DebugTextAdd("[Log]From mapManager get set-material-result: " + set_res + ". ");
        }
        else if (mode == DestroyMode.RemainWithRate){
            List<KeyValuePair<int, int>> sample = CropManager.Instance.GetCrop(aim_ins.GetModelId()).harvest_list;
            List<KeyValuePair<int, int>> temp = new List<KeyValuePair<int, int>>();
            int crop_id, amount;
            for (int i = 0; i < sample.Count; i++){
                crop_id = sample[i].Key;
                amount = sample[i].Value;
                //Debug.Log(crop_id.ToString()+ "|" + amount.ToString());

                amount = (int)System.Math.Truncate((double)(amount * remain_rate));
                if (amount > 0)
                    temp.Add(new KeyValuePair<int, int>(crop_id, amount));
            }
            int set_res = MapManager.Instance.SetMaterial(aim_ins.position, temp);
            UIManager.Instance.DebugTextAdd("[Log]From mapManager get set-material-result: " + set_res + ". ");
        }
        else{
            UIManager.Instance.DebugTextAdd(
                "<<Warning>>ClearCropInstance has not implement with DestroyMode: " + mode.ToString() + ". "
            );
        }
        return;
    }
    public void ClearBuildingInstance(ItemInstance aim_ins, DestroyMode mode, float remain_rate)
    {
        // Safety check
        if (aim_ins.type != ItemInstanceType.BuildingInstance){
            UIManager.Instance.DebugTextAdd(
                "<<Error>>Clearing BuildingInstance FAILED: the iteminstance_id " + aim_ins.id + " is not a BuildingInstance. "
            );
            return;
        }
        // 1.if content is not null, content will replace the aim_ins
        if( ((BuildingInstance)aim_ins).content != null){
            // 将内容物放置到地图上
            MapManager.Instance.SetMapDataItem(((BuildingInstance)aim_ins).content, aim_ins.position);
            // 将内容物的position设置为aim_ins的position
            ((BuildingInstance)aim_ins).content.position = aim_ins.position;
        }
        // 2.material drop
        if (mode == DestroyMode.RemainNone) {
            // 不产生任何遗留物
        }
        else if(mode == DestroyMode.RemainAll) {
            // 直接将建筑所需材料全部掉落
            int set_res = MapManager.Instance.SetMaterial(aim_ins.position, ((BuildingInstance)aim_ins).material_list.ToList());
            UIManager.Instance.DebugTextAdd("[Log]From mapManager get set-material-result: " + set_res + ". ");
        }
        else if (mode == DestroyMode.RemainWithRate) {
            // 按照remain_rate比例掉落建筑所需材料
            List<KeyValuePair<int, int>> temp = new List<KeyValuePair<int, int>>();
            foreach (KeyValuePair<int, int> it in ((BuildingInstance)aim_ins).material_list) {
                int item_id = it.Key;
                int amount = (int)System.Math.Truncate((double)(it.Value * remain_rate));
                if (amount > 0)
                    temp.Add(new KeyValuePair<int, int>(item_id, amount));
            }
            int set_res = MapManager.Instance.SetMaterial(aim_ins.position, temp);
            UIManager.Instance.DebugTextAdd("[Log]From mapManager get set-material-result: " + set_res + ". ");
        }
        else {
            UIManager.Instance.DebugTextAdd(
                "<<Warning>>ClearBuildingInstance has not implement with DestroyMode: " + mode.ToString() + ". "
            );
        }
        return;
    }
    public void ClearResourceInstance(ItemInstance aim_ins, DestroyMode mode, float remain_rate){
        // Safety check
        if (aim_ins.type != ItemInstanceType.ResourceInstance){
            UIManager.Instance.DebugTextAdd(
                "<<Error>>Clearing ResourceInstance FAILED: the iteminstance_id " + aim_ins.id + " is not a ResourceInstance. "
            );
            return;
        }
        // 1.如果是RemainNone模式，则不产生任何遗留物
        if (mode == DestroyMode.RemainNone) {
            // 不产生任何遗留物
        }
        else if (mode == DestroyMode.RemainAll) {
            // 直接将资源掉落
            int set_res = MapManager.Instance.SetMaterial(aim_ins.position, ((ResourceInstance)aim_ins).resource_list.ToList());
            UIManager.Instance.DebugTextAdd("[Log]From mapManager get set-resource-result: " + set_res + ". ");
        }
        else if (mode == DestroyMode.RemainWithRate) {
            // 按照remain_rate比例掉落资源
            List<KeyValuePair<int, int>> temp = new List<KeyValuePair<int, int>>();
            foreach (KeyValuePair<int, int> it in ((ResourceInstance)aim_ins).resource_list) {
                int item_id = it.Key;
                int amount = (int)System.Math.Truncate((double)(it.Value * remain_rate));
                if (amount > 0)
                    temp.Add(new KeyValuePair<int, int>(item_id, amount));
            }
            int set_res = MapManager.Instance.SetMaterial(aim_ins.position, temp);
            UIManager.Instance.DebugTextAdd("[Log]From mapManager get set-resource-result: " + set_res + ". ");
        }
        else {
            UIManager.Instance.DebugTextAdd(
                "<<Warning>>ClearResourceInstance has not implement with DestroyMode: " + mode.ToString() + ". "
            );
        }
    }
    #endregion

    #endregion

    #region 4.更新各种ItemInstance个体的子函数
    #region (0) 所有ItemInstance个体可用

    #endregion

    #region (1) 分类按时间更新
    /// <summary>
    /// private:CropInstance的生长更新
    /// </summary>
    public void UpdateAllCropInstance()
    {
        float grow, life, grow_per_frame;
        int stage, new_stage;
        foreach (ItemInstance it in itemInstanceLists[ItemInstanceType.CropInstance])
        {
            // Get the (old)growth , lifetime and growth_per_frame of the crop
            grow = ((CropInstance)it).growth;
            life = ((CropInstance)it).real_lifetime;
            grow_per_frame = ((CropInstance)it).growth_per_frame;
            if (grow >= life) continue;
            // growth is affected by timeScale
            ((CropInstance)it).growth += grow_per_frame * TimeManager.Instance.timeScale;

            // Ensure the stage
            stage = (int)(3 * (grow / life));
            new_stage = (int)(3 * ((grow + grow_per_frame * TimeManager.Instance.timeScale) / life));
            if (new_stage > stage)
            {
                if (new_stage < 0 || new_stage > 3)
                {
                    UIManager.Instance.DebugTextAdd("<<Error>> illegal growth stage: " + new_stage);
                }
                else
                {
                    // Reset the sprite to the one of the new stage
                    it.SetSprite(CropManager.Instance.GetSprite(it.item_id, new_stage));
                }
            }

        }
    }
    #endregion
    #region (2) 供外部调用的批量更新
    /// <summary>
    /// 供CropManager的影响因素模块调用的批量更新函数，会触发相应种类的作物实体更新自己的每固定帧生长值缓存。
    /// </summary>
    /// <param name="crop_id">指定作物的ID，默认为-1表示更新所有作物</param>
    public void UpdateGrowthPerFrame(int crop_id = -1)
    {
        if (crop_id == -1)
        {
            // 更新所有CropInstance的生长速率
            Dictionary<int, float> growth_per_frame_dict = CropManager.Instance.growthPerFrameDict;
            foreach (ItemInstance it in itemInstanceLists[ItemInstanceType.CropInstance])
                ((CropInstance)it).growth_per_frame = growth_per_frame_dict[it.item_id];
        }
        else
        {
            // 更新指定crop_id的CropInstance的生长速率
            float growth_per_frame = CropManager.Instance.GetGrowthPerFrame(crop_id);
            foreach (ItemInstance it in itemInstanceLists[ItemInstanceType.CropInstance])
                if (it.item_id == crop_id)
                    ((CropInstance)it).growth_per_frame = growth_per_frame;
        }
        return;
    }
    #endregion
    #endregion
    // Start is called before the first frame update
    /// <summary>
    /// private
    /// </summary>
    void InitInstanceListsData()
    {
        #region itemInstanceLists初始化
        for (ItemInstanceType i = ItemInstanceType.ToolInstance; i < ItemInstanceType.Total; i++){
            itemInstanceLists.Add(i, new List<ItemInstance>());
        }
        // itemInstanceLists.Add(ItemInstanceType.ToolInstance, new List<ItemInstance>());
        // itemInstanceLists.Add(ItemInstanceType.MaterialInstance, new List<ItemInstance>());
        // itemInstanceLists.Add(ItemInstanceType.BuildingInstance, new List<ItemInstance>());
        // itemInstanceLists.Add(ItemInstanceType.CropInstance, new List<ItemInstance>());
        // itemInstanceLists.Add(ItemInstanceType.PrintInstance, new List<ItemInstance>());
        // itemInstanceLists.Add(ItemInstanceType.ResourceInstance, new List<ItemInstance>());
        
        //Debug.Log("init itemInstanceLists finished");
        #endregion

    }
    
    bool isTested = false;
    /// <summary>
    /// 临时测试函数，靠isTested标志位控制是否执行。
    /// TODO：该方法使得FixedUpdate函数持续检查是否执行测试。
    ///     我们期望一个更好的测试机制，既能完成测试又能不产生此影响（或许我们期望一个TestManager？）
    /// </summary>
    public void Test()
    {
        //动态载入部分ItemInstance供测试
        #region (1)CropInstance收割和生长接口自测试 【由于会导致脚本调用顺序成环,转移到MapManager 原因：用到MapManager】
        // CropInstance tmp1 = (CropInstance)SpawnItem(new Vector3Int(30, 30, 0), 0, ItemInstanceType.CropInstance);
        // CropInstance tmp2 = (CropInstance)SpawnItem(new Vector3Int(30, 31, 0), 1, ItemInstanceType.CropInstance);
        // CropInstance tmp3 = (CropInstance)SpawnItem(new Vector3Int(30, 32, 0), 2, ItemInstanceType.CropInstance);

        // HarvestCrop(tmp1);
        // HarvestCrop(tmp2);
        // HarvestCrop(tmp3);
        #endregion

        #region (2)ToolInstance获取强化项接口自测试  【由于会导致脚本调用顺序成环暂且停用 原因：用到MapManager】
        //ToolInstance tmp4 = (ToolInstance)SpawnItem(new Vector3Int(30, 34, 0), 2, ItemInstanceType.ToolInstance);
        // Dictionary<PawnManager.Pawn.EnhanceType, int> tmp4_enh = GetEnhance(tmp4);
        // foreach (var it in tmp4_enh)
        // {
        //     Debug.Log("key:" + it.Key.ToString() + "|val:" + it.Value.ToString());
        // }
        #endregion

        isTested = true;
        Debug.Log("ItemInstanceManager Test Finished.");
    }
    void Start()
    {
        // 同步尺寸
        GetComponent<RectTransform>().sizeDelta = content.GetComponent<RectTransform>().sizeDelta;
        InitInstanceListsData();
        isTested = false;
    }

    void FixedUpdate()
    {
        if(!isTested){
            Test();
        }
        UpdateAllCropInstance();
    }
    //======================================Public Function Part=======================================
    //===================================ItemInstance Function Part====================================

    /// <summary>
    /// 在指定位置`position`生成一个指定`type`的ItemInstance
    /// </summary>
    /// <param name="sample_id">`id` for class `Item`,`Crop` or `Building` which you're going to make a instance </param>
    /// <param name="amount">default 1, used for type like `Material`</param>
    public ItemInstance SpawnItem(Vector3Int position, int sample_id, ItemInstanceType type, int amount = 1)
    {
        ItemInstance new_ins = null;
        // 按type要求进行不同类型ItemInstance的生成：
        switch (type)
        {
            case ItemInstanceType.ToolInstance:
                new_ins = MakeToolInstance(sample_id, position);
                break;
            case ItemInstanceType.MaterialInstance:
                new_ins = MakeMaterialInstance(sample_id, position, amount);
                if(new_ins != null)
                    StorageManager.Instance.AddItem(new_ins.item_id, amount);
                break;
            case ItemInstanceType.CropInstance:
                new_ins = MakeCropInstance(sample_id, position);
                break;
            case ItemInstanceType.BuildingInstance:
                new_ins = MakeBuildingInstance(sample_id, position);
                break;
            case ItemInstanceType.PrintInstance:
                new_ins = MakePrintInstance(sample_id, position);
                break;
            case ItemInstanceType.ResourceInstance:
                new_ins = MakeResourceInstance(sample_id, position);
                break;
            default:
                UIManager.Instance.DebugTextAdd(
                    "<<Error>>Spawning an ItemInstance FAILED: the type is not found in ItemInstanceManager. "
                );
                break;
        }
        // 若创建成功：
        if (new_ins != null)
        {
            // 分配唯一ID
            new_ins.id = GetNewId();
            // 将其GameObject名字设置为id
            new_ins.instance.name = new_ins.id.ToString();
            // 加入列表
            itemInstanceLists[type].Add(new_ins);
            // 设置到地图数据中
            MapManager.Instance.SetMapDataItem(new_ins, position);
        }
        else Debug.LogError("SpawnItem failed: new_ins is null.");
        return new_ins;
    }

    /// <summary>
    /// 销毁指定的ItemInstance
    /// note: PawnManager不应使用此函数进行Crop的收获或毁坏,请注意HarvestCrop和RuinCrop函数
    /// </summary>
    /// <param name="remain_rate">遗留物的生成率，只在destroy_mode==DestroyMode.RemainWithRate时有效</param>
    public void DestroyItem(ItemInstance aim_ins, DestroyMode destroy_mode = DestroyMode.RemainNone, float remain_rate = 0.5f){
        // 检查该Instance是否存在
        if (GetInstance(aim_ins.id) == null) {
            UIManager.Instance.DebugTextAdd(
                "<<Error>>Destroying Item FAILED: the iteminstance_id " + aim_ins.id + " is not found in ItemInstanceManager. "
            );
            return;
        }
        // 检查销毁模式
        if (destroy_mode >= DestroyMode.Total || destroy_mode < 0) {
            UIManager.Instance.DebugTextAdd(
                "<<Error>>Destroying Item FAILED: UnDefined DestroyMode. "
            );
            return;
        }
        // 进行销毁
        //  从ItemInstanceList中删除
        itemInstanceLists[aim_ins.type].Remove(aim_ins);
        //  与InitInstance对应 销毁GameObject
        DestroyInstance(aim_ins);
        //  清除不同种类的ItemInstance的内含物
        switch (aim_ins.type) {
            case ItemInstanceType.ToolInstance:
                break;
            case ItemInstanceType.MaterialInstance:
                break;
            case ItemInstanceType.CropInstance:
                ClearCropInstance(aim_ins, destroy_mode, remain_rate);
                break;
            case ItemInstanceType.BuildingInstance:
                ClearBuildingInstance(aim_ins, destroy_mode, remain_rate);
                break;
            case ItemInstanceType.PrintInstance:
                ClearPrintInstance(aim_ins, destroy_mode, remain_rate);
                break;
            case ItemInstanceType.ResourceInstance:
                ClearResourceInstance(aim_ins, destroy_mode, remain_rate);
                break;
            default:
                UIManager.Instance.DebugTextAdd(
                    "<<Error>>Destroying Item FAILED: the type is not found in ItemInstanceManager. "
                );
                break;
        }
        //  回收ID
        RecycleId(aim_ins.id);
        
        return;
    }

    public Dictionary<ItemInstanceType, List<ItemInstance>> itemInstanceLists = new Dictionary<ItemInstanceType, List<ItemInstance>>();
    /// <summary>
    /// 获取指定ID和类型的ItemInstance
    /// 若该ID的实体不存在或实体类型不匹配，均返回null
    /// </summary>
    /// <param name="type">默认Total，查找所有类型</param>
    public ItemInstance GetInstance(int iteminstance_id, ItemInstanceType type = ItemInstanceType.Total)
    {
        ItemInstance aim_ins = null;
        if (type != ItemInstanceType.Total)
        {
            aim_ins = itemInstanceLists[type].Find(c => c.id == iteminstance_id);
        }
        else
        {
            for (ItemInstanceType i = 0; i < ItemInstanceType.Total; i++)
            {
                if (itemInstanceLists[i] is not null)
                    aim_ins = itemInstanceLists[i].Find(c => c.id == iteminstance_id);
                if (aim_ins is not null)
                    break;
            }
        }

        if (aim_ins is null)
        {
            UIManager.Instance.DebugTextAdd(
                "[Log]Getting Item FAILED: the iteminstance_id " + iteminstance_id + " is not found in " + type.ToString() + " List. "
            );
        }
        return aim_ins;
    }
    /// 查找给定地址附近最近的指定物品地址
    /// <param name="item_id">目标物品的id</param>
    /// <param name="currentPosition">当前地址</param>
    /// <param name="searchRadius">搜索半径（可选，默认为无限大）</param>
    /// <returns>最近物品的地址（Vector3Int），如果未找到则返回null</returns>
    /// 此函数lyq负责，用于运输任务时查询
    /// todo:目前仅实现最近距离查找，后续需要加入物品数量，地块是否可通行等方面的考虑
    public Vector3Int FindNearestItemPosition(int item_id, Vector3Int currentPosition, float searchRadius = float.MaxValue)
{
        Vector3Int NearestPosition = Vector3Int.zero;
        float minDistance = float.MaxValue;
        // 遍历所有ItemInstance列表查找，可以后续优化
        foreach (var itemList in itemInstanceLists.Values)
        {
            foreach (var itemInstance in itemList)
            {
                if (itemInstance.item_id == item_id && itemInstance.type == ItemInstanceType.MaterialInstance)
                {
                    float distance = Vector3Int.Distance(currentPosition, itemInstance.position);
                    if (distance < minDistance && distance <= searchRadius)
                    {
                        minDistance = distance;
                        NearestPosition = itemInstance.position;
                    }
                }
            }
        }

        if (NearestPosition == Vector3Int.zero)
        {
            UIManager.Instance.DebugTextAdd(
                "[Log]Finding Nearest Item FAILED: No item with id " + item_id + " found near position " + currentPosition + "."
            );
        }

        return NearestPosition;
    }
    #region ======================ToolInstance Function Part=========================
    /// <summary>
    /// 获取工具的强化信息
    /// </summary>
    public Dictionary<PawnManager.Pawn.EnhanceType, int> GetEnhance(ToolInstance tool_ins)
    {
        Dictionary<PawnManager.Pawn.EnhanceType, int> res;
        ItemManager.Tool sample = (ItemManager.Tool)ItemManager.Instance.GetItem(tool_ins.item_id, ItemManager.ItemType.Tool);
        res = sample.enhancements;
        return res;
    }
    #endregion

    #region ===================CropInstance Function Part=====================
    /// <summary>
    /// 收获作物实体。若作物成熟则收获并掉落相应材料实体，否则返回false。
    /// 注意：当前实现处于调试状态，此函数不会检查作物是否成熟直接进行收获。
    /// </summary>
    /// <param name="crop_ins"></param>
    /// <returns></returns>
    public bool HarvestCrop(CropInstance crop_ins)
    {
        // if (crop_ins.IsMature())
        // {
        Debug.Log("Harvesting Crop: " + crop_ins.id);
        DestroyItem(crop_ins, DestroyMode.RemainAll);
        return true;
        // }
        // else
        // {
        //     Debug.Log("Crop is not mature, cannot harvest.");
        // }
        //return false;
    }
    /// <summary>
    /// 开采资源实体，使之掉落相应材料
    /// </summary>
    public bool HarvestResource(ResourceInstance resourceInstance) {
        Debug.Log("Harvesting Resource: " + resourceInstance.id);
        DestroyItem(resourceInstance, DestroyMode.RemainAll);
        return true;
    }
    /// <summary>
    /// 毁坏作物实体，不会掉落任何材料
    /// </summary>
    public bool RuinCrop(CropInstance crop_ins)
    {
        DestroyItem(crop_ins, DestroyMode.RemainNone);
        return true;
    }
    /// <summary>
    /// 在要求的位置种植一个该种子ID对应的作物种类的作物实体。
    /// 注意：传入的int是种子种类ID，显然不会完成种子实体的消耗工作，需要配合种子实体的GetAmount和SetAmount函数。
    /// </summary>
    /// <returns>返回是否成功种植</returns>
    public bool PlantSeed(int seedId, Vector3Int position){
        // Check
        ItemManager.Material seed = (ItemManager.Material)ItemManager.Instance.GetItem(seedId, ItemManager.ItemType.Material);
        int cropid = seed.can_plant_crop;
        if (!(cropid == -1))
        {
            SpawnItem(position, cropid, ItemInstanceType.CropInstance);
            Debug.Log("Planting seed: " + seedId + " at position: " + position);
        }
        return true;
    }
    #endregion
}
