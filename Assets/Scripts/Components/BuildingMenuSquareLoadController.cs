using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuildingMenuSquareLoadController : MonoBehaviour
{
    public SpriteRenderer texture;
    public TextMeshProUGUI buildingName;

    public void Init(String name, Sprite sprite){
        buildingName.text = name;
        texture.sprite = sprite;
    }
}
