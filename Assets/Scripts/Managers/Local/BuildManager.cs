
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }
    public enum BuildingType{
        Dev,Wall,Farm,Total
    }

    public class Building{
        //数据属性
        public int id;
        public string name;
        public Sprite texture;
        public BuildingType type;
        //游戏属性
        public int width, height;
        public int durability;

        public bool can_build;
        public bool can_walk;
        public bool can_plant;

        public List<KeyValuePair<int, int>> material_list = new List<KeyValuePair<int, int>>();

        //TODO: 拓展为List<bool> cans + enum canTypes{walk,build,plant}
    }

    public Dictionary<BuildingType, List<Building>> buildingLists = new Dictionary<BuildingType, List<Building>>();
    public List<Building> currentBuildingList ;

    public Building currentBuilding = null;
    public GameObject currentBuilding_preview;
    public SpriteRenderer currentBuilding_preview_spriteRenderer;
    

    const int tempBuildingSpritesCount = 6;
    public Sprite[] tempBuildingSprites = new Sprite[tempBuildingSpritesCount];
    public Sprite printSprite;

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
        InitBuildingListsData();
        if(currentBuilding_preview!=null)
            currentBuilding_preview_spriteRenderer = currentBuilding_preview.GetComponent<SpriteRenderer>();
        else
            Debug.LogError("Error：请把Managers下的currentBuilding_preview拖到Stage-BuildManager脚本上！");
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(1)){
            CancelCurrentBuilding();
        }
    }

    void InitBuildingListsData(){
        for(int i = 0; i < (int)BuildingType.Total; i++)
            buildingLists.Add((BuildingType)i, new List<Building>());
        
        //TODO: 目前的material_list直接参考id，后续应该改为引用

        buildingLists[BuildingType.Dev].Add(new Building{
            id = 0, name="草地", texture = tempBuildingSprites[0], type = BuildingType.Dev, width = 1, height = 1, durability = -1,
            can_build = true, can_walk = true, can_plant = true, material_list = new List<KeyValuePair<int, int>>()});
        buildingLists[BuildingType.Dev].Add(new Building{
            id = 1, name="路径", texture = tempBuildingSprites[1], type = BuildingType.Dev, width = 1, height = 1, durability = -1,
            can_build = false, can_walk = true, can_plant = false, material_list = new List<KeyValuePair<int, int>>()});
        buildingLists[BuildingType.Dev].Add(new Building{
            id = 2, name="水地", texture = tempBuildingSprites[2], type = BuildingType.Dev, width = 1, height = 1, durability = -1,
            can_build = false, can_walk = false, can_plant = false, material_list = new List<KeyValuePair<int, int>>()});
        buildingLists[BuildingType.Dev].Add(new Building{
            id = 3, name="树木", texture = tempBuildingSprites[3], type = BuildingType.Dev, width = 1, height = 1, durability = -1,
            can_build = false, can_walk = false, can_plant = false, material_list = new List<KeyValuePair<int, int>>()});

        buildingLists[BuildingType.Wall].Add(new Building{
            id = 4, name="墙", texture = tempBuildingSprites[4], type = BuildingType.Wall, width = 1, height = 1, durability = 100,
            can_build = false, can_walk = false, can_plant = false, material_list = new List<KeyValuePair<int, int>>{
                new KeyValuePair<int, int>(5, 10)
            }});

        buildingLists[BuildingType.Farm].Add(new Building{
            id = 5, name="农田", texture = tempBuildingSprites[5], type = BuildingType.Farm, width = 1, height = 1, durability = -1,
            can_build = false, can_walk = false, can_plant = true, material_list = new List<KeyValuePair<int, int>>()});
    }

    public Building GetBuilding(int id){
        //TODO:CACHE
        
        foreach(var list in buildingLists.Values){
            foreach(var building in list){
                if(building.id == id)
                    return building;
            }
        }
        return null;
    }

    /// <summary>
    /// 将type类型的建筑列表加载到currentBuildingList，返回值该List传递回uiManager
    /// </summary>
    /// <param name="type">需加载的建筑类型</param>
    public List<Building> LoadBuildingList(BuildingType type){
        currentBuildingList = buildingLists[type];
        return currentBuildingList;
    }

    public void CancelCurrentBuilding(){
        currentBuilding_preview_spriteRenderer.sprite = null;
        currentBuilding_preview.SetActive(false);
        currentBuilding = null;
    }
    public void SetCurrentBuilding(Building building){ 
        currentBuilding_preview_spriteRenderer.sprite = building.texture;
        currentBuilding_preview.SetActive(true);
        currentBuilding = building;
    }


}
