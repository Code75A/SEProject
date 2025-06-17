using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abstract/Factors/LinearEnvFactor")]
public class LinearEnvFactor : EnvFactor
{
    public override float GetImpacted(float origin, float env_data)
    {
        float use_env_data = Mathf.Clamp(env_data, down_limit, up_limit);
        return origin * (1.0f + (use_env_data - standard_env) / (up_limit - down_limit));
    }
}