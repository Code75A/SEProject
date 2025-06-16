using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public List<KeyValuePair<int, int>> harvest_list;
}
