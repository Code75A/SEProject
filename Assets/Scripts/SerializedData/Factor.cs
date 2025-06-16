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

[CreateAssetMenu(menuName = "Abstract/Factors/LinearFactor")]
public class LinearFactor : Factor
{
    public float change_rate;
    public override float GetImpacted(float origin) { return origin * change_rate; }
    public override float GetImpacted(float origin, float env_data)
    {
        UIManager.Instance.DebugTextAdd("<<Error>>LinearFactor.GetImpacted should not be called with expect_env.");
        return origin;
    }
}
[CreateAssetMenu(menuName = "Abstract/Factors/ZeroFactor")]
public class ZeroFactor : Factor
{
    public override float GetImpacted(float origin) { return 0.0f; }
    public override float GetImpacted(float origin, float env_data) {
        UIManager.Instance.DebugTextAdd("<<Error>>ZeroFactor.GetImpacted should not be called with expect_env.");
        return origin;
    }
}
[CreateAssetMenu(menuName = "Abstract/Factors/EnvFactor")]
public abstract class EnvFactor : Factor
{
    public float standard_env;
    public float up_limit;
    public float down_limit;
    public sealed override float GetImpacted(float origin)
    {
        UIManager.Instance.DebugTextAdd("<<Warning>>EnvFactor.GetImpacted should be called with expect_env.");
        return origin;
    }
}

[CreateAssetMenu(menuName = "Abstract/Factors/LinearEnvFactor")]
public class LinearEnvFactor : EnvFactor
{
    public override float GetImpacted(float origin, float env_data)
    {
        float use_env_data = Mathf.Clamp(env_data, down_limit, up_limit);
        return origin * (1.0f + (use_env_data - standard_env) / (up_limit - down_limit));
    }
}
[CreateAssetMenu(menuName = "Abstract/Factors/LinearDiffEnvFactor")]
public class LinearDiffEnvFactor : EnvFactor
{
    public override float GetImpacted(float origin, float env_data)
    {
        float use_env_data = Mathf.Clamp(env_data, down_limit, up_limit);
        return origin * (1.0f - (Mathf.Abs(standard_env - use_env_data) / (up_limit - down_limit)));
    }
}
[CreateAssetMenu(menuName = "Abstract/Factors/InRangeEnvFactor")]
public class InRangeEnvFactor : EnvFactor {
    public override float GetImpacted(float origin, float env_data) {
        if (env_data < down_limit || env_data > up_limit) return 0.0f; // Outside the range, no impact
        else return origin;
    }
}
#endregion