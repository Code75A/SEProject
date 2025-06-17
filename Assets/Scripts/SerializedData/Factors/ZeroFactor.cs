using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abstract/Factors/ZeroFactor")]
public class ZeroFactor : Factor
{
    public override float GetImpacted(float origin) { return 0.0f; }
    public override float GetImpacted(float origin, float env_data) {
        UIManager.Instance.DebugTextAdd("<<Error>>ZeroFactor.GetImpacted should not be called with expect_env.");
        return origin;
    }
}