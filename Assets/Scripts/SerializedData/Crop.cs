using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Crop受fertility、humidity、light等环境因素影响
//具体影响的变动公式见Factor及其子类定义

[CreateAssetMenu(menuName = "Concrete/Crop")]
public class Crop : ScriptableObject
{
    public int id;
    public int seed_id; // Seed ID, used for ItemInstanceManager
    public string crop_name;
    public float lifetime;
    public float best_fertility;
    public FactorType fertility_factor_type = FactorType.LinearEnv;
    public float best_humidity;
    public FactorType humidity_factor_type = FactorType.LinearDiffEnv;
    public float best_light;
    public FactorType light_factor_type = FactorType.LinearDiffEnv;

    public List<IntPair> harvest_statistics;
    public List<KeyValuePair<int, int>> harvest_list = new List<KeyValuePair<int, int>>();

    public void InitHarvestList()
    {
        foreach (var pair in harvest_statistics)
            harvest_list.Add(new KeyValuePair<int, int>(pair.Key, pair.Value));
    }
}
