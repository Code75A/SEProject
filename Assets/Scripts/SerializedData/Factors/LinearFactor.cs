using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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