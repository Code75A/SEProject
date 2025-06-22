
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }

    public Dictionary<BuildingType, List<Building>> buildingLists = new Dictionary<BuildingType, List<Building>>();
    public List<Building> currentBuildingList;

    //TODO: REMOVE
    public Building currentBuilding = null;
    public GameObject currentBuilding_preview;
    public SpriteRenderer currentBuilding_preview_spriteRenderer;
    //REMOVE/


    const int tempBuildingSpritesCount = 7;
    public Sprite[] tempBuildingSprites = new Sprite[tempBuildingSpritesCount];
    public Sprite printSprite;

    //单例模式
    void Awake()
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
    void Start()
    {
        InitBuildingListsData();
        if (currentBuilding_preview != null)
            currentBuilding_preview_spriteRenderer = currentBuilding_preview.GetComponent<SpriteRenderer>();
        else
            Debug.LogError("Error：请把Managers下的currentBuilding_preview拖到Stage-BuildManager脚本上！");
    }

    // void Update()
    // {
    //     if(Input.GetMouseButtonDown(1)){
    //         CancelCurrentBuilding();
    //     }
    // }

    void InitBuildingListsData()
    {
        for (int i = 0; i < (int)BuildingType.Total; i++)
            buildingLists.Add((BuildingType)i, new List<Building>());
#if UNITY_EDITOR
        //TODO: 目前的material_list直接参考id，后续应该改为引用
        string[] guids = AssetDatabase.FindAssets("t:Building", new[] { "Assets/Resources/BuildingData" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Building building = AssetDatabase.LoadAssetAtPath<Building>(path);
            if (building != null)
            {
                if (!buildingLists.ContainsKey(building.type))
                {
                    Debug.LogWarning($"Building type {building.type} not found in buildingLists, adding it now.");
                    buildingLists[building.type] = new List<Building>();
                }
                building.InitMaterialList();
                buildingLists[building.type].Add(building);
            }
        }
#else
        // 非编辑器下用 Resources.LoadAll 加载
        Building[] buildings = Resources.LoadAll<Building>("BuildingData");
        foreach (var building in buildings)
        {
            if (building != null)
            {
                if (!buildingLists.ContainsKey(building.type)){
                    buildingLists[building.type] = new List<Building>();
                }
                building.InitMaterialList();
                buildingLists[building.type].Add(building);
            }
        }
#endif
    }

    public Building GetBuilding(int id)
    {
        //TODO:CACHE
        if (id == TraderManager.TRADER_ID)
            return TraderManager.Instance.trader;

        foreach (var list in buildingLists.Values)
        {
            foreach (var building in list)
            {
                if (building.id == id)
                    return building;
            }
        }
        return null;
    }

    public BuildingType GetBuildingType(int id)
    {
        foreach (var list in buildingLists.Values)
        {
            foreach (var building in list)
            {
                if (building.id == id)
                    return building.type;
            }
        }
        return BuildingType.Total;
    }
    /// <summary>
    /// 将type类型的建筑列表加载到currentBuildingList，返回值该List传递回uiManager
    /// </summary>
    /// <param name="type">需加载的建筑类型</param>
    public List<Building> LoadBuildingList(BuildingType type)
    {
        currentBuildingList = buildingLists[type];
        return currentBuildingList;
    }


}

//暂未实装，需要扩展框架结构才能容纳
#region "奇观建筑"
// 疾风奇观：提升全体小人移动速度
public class GaleWonderBuilding : Building
{
    public static bool IsBuilt = false;
    public const float MoveSpeedBuff = 1.5f;
    public GaleWonderBuilding()
    {
        this.id = 101;
        this.build_name = "疾风奇观";
        this.texture = null; // 可指定专属Sprite
        this.type = BuildingType.Dev;
        this.width = 2;
        this.height = 2;
        this.durability = 99999;
        this.can_build = true;
        this.can_walk = false;
        this.can_plant = false;
        this.material_list = new List<KeyValuePair<int, int>>{
        };
    }
    public static void OnBuilt()
    {
        if (!IsBuilt)
        {
            IsBuilt = true;
            //PawnManager.ApplyGaleWonderBuff();
            Debug.Log("疾风奇观建成，全体小人移动速度提升！");
        }
    }
    public static void OnDestroyed()
    {
        if (IsBuilt)
        {
            IsBuilt = false;
            //PawnManager.RemoveGaleWonderBuff();
            Debug.Log("疾风奇观被摧毁，移动速度恢复正常。");
        }
    }
}

// 勤工奇观：提升全体小人工作速度
public class DiligenceWonderBuilding : Building
{
    public static bool IsBuilt = false;
    public const float WorkSpeedBuff = 1.5f;
    public DiligenceWonderBuilding()
    {
        this.id = 102;
        this.build_name = "勤工奇观";
        this.texture = null;
        this.type = BuildingType.Dev;
        this.width = 2;
        this.height = 2;
        this.durability = 99999;
        this.can_build = true;
        this.can_walk = false;
        this.can_plant = false;
        this.material_list = new List<KeyValuePair<int, int>>{
        };
    }
    public static void OnBuilt()
    {
        if (!IsBuilt)
        {
            IsBuilt = true;
            //PawnManager.ApplyDiligenceWonderBuff();
            Debug.Log("勤工奇观建成，全体小人工作速度提升！");
        }
    }
    public static void OnDestroyed()
    {
        if (IsBuilt)
        {
            IsBuilt = false;
            //PawnManager.RemoveDiligenceWonderBuff();
            Debug.Log("勤工奇观被摧毁，工作速度恢复正常。");
        }
    }
}

// 巨力奇观：提升全体小人运载容量
public class MightWonderBuilding : Building
{
    public static bool IsBuilt = false;
    public const float CapacityBuff = 1.5f;
    public MightWonderBuilding()
    {
        this.id = 103;
        this.build_name = "巨力奇观";
        this.texture = null;
        this.type = BuildingType.Dev;
        this.width = 2;
        this.height = 2;
        this.durability = 99999;
        this.can_build = true;
        this.can_walk = false;
        this.can_plant = false;
        this.material_list = new List<KeyValuePair<int, int>>{
        };
    }
    public static void OnBuilt()
    {
        if (!IsBuilt)
        {
            IsBuilt = true;
            //PawnManager.ApplyMightWonderBuff();
            Debug.Log("巨力奇观建成，全体小人运载容量提升！");
        }
    }
    public static void OnDestroyed()
    {
        if (IsBuilt)
        {
            IsBuilt = false;
            //PawnManager.RemoveMightWonderBuff();
            Debug.Log("巨力奇观被摧毁，运载容量恢复正常。");
        }
    }
}
#endregion
