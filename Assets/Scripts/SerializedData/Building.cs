using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType{
        Dev,Wall,Farm,Total
}

[CreateAssetMenu(menuName = "Concrete/Building")]
public class Building : ScriptableObject
{
    //数据属性
    public int id;
    public string build_name;
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