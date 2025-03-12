using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BuildingMenuSquareLoadController : MonoBehaviour
{
    public SpriteRenderer texture;
    public TextMeshProUGUI buildingName;
    public BuildManager.Building building;

    public void Init(BuildManager.Building building,Sprite sprite){
        buildingName.text = building.name;
        texture.sprite = sprite;
        this.building = building;
    }

    void OnMouseDown(){
        if(building != null) 
        {
            BuildManager.Instance.SetCurrentBuilding(building);
            UIManager.Instance.DebugTextAdd("选择建筑: " + building.name);
        }
        
    }
}
