using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
