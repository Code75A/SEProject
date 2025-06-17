using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 影响因素模块-class Factor的基本内容
public enum FactorType
{
    Linear, LinearEnv, LinearDiffEnv, InRangeEnv, Zero, Total
}

public abstract class Factor : ScriptableObject
{
    //public int id;
    public FactorType type;
    public abstract float GetImpacted(float origin);
    public abstract float GetImpacted(float origin, float env_data);
}
#endregion