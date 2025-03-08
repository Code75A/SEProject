
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuildManager : MonoBehaviour
{
    public UIManager uiManager;
    public enum BuildingType{
        Dev,Wall,Farm,Total
    }

    public class Building{
        //数据属性
        public int id;
        public string name;
        public BuildingType type;
        //游戏属性
        public int width,height;
        public int durability;
        
    }

    public Dictionary<BuildingType, List<Building>> buildingLists = new Dictionary<BuildingType, List<Building>>();
    
    //当前建筑
    public Building currentBuilding = null;
    public List<Building> currentBuildingList ;

    void Start()
    {
        InitBuildingListsData();
        
        //uiManager.InitBuildMenu();
    }

    void InitBuildingListsData(){
        for(int i = 0; i < (int)BuildingType.Total; i++)
            buildingLists.Add((BuildingType)i, new List<Building>());
        
        buildingLists[BuildingType.Dev].Add(new Building{id = 0, name="草地", type = BuildingType.Dev, width = 1, height = 1, durability = -1});
        buildingLists[BuildingType.Dev].Add(new Building{id = 1, name="路径", type = BuildingType.Dev, width = 1, height = 1, durability = -1});
        buildingLists[BuildingType.Dev].Add(new Building{id = 2, name="水地", type = BuildingType.Dev, width = 1, height = 1, durability = -1});
        buildingLists[BuildingType.Dev].Add(new Building{id = 3, name="树木", type = BuildingType.Dev, width = 1, height = 1, durability = -1});

        buildingLists[BuildingType.Wall].Add(new Building{id = 4, name="墙", type = BuildingType.Wall, width = 1, height = 1, durability = 100});

        buildingLists[BuildingType.Farm].Add(new Building{id = 5, name="农田", type = BuildingType.Farm, width = 1, height = 1, durability = -1});
    }

    /// <summary>
    /// 将type类型的建筑列表加载到currentBuildingList，返回值该List传递回uiManager
    /// </summary>
    /// <param name="type">需加载的建筑类型</param>
    public List<Building> LoadBuildingList(BuildingType type){
        currentBuildingList = buildingLists[type];
        return currentBuildingList;
    }
    
}
