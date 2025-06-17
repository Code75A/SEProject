using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abstract/Factors/InRangeEnvFactor")]
public class InRangeEnvFactor : EnvFactor {
    public override float GetImpacted(float origin, float env_data) {
        if (env_data < down_limit || env_data > up_limit) return 0.0f; // Outside the range, no impact
        else return origin;
    }
}
