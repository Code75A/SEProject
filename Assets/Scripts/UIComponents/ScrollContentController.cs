using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollContentController : MonoBehaviour
{
    public MapManager mapManager;
    public RectTransform gridTransform;
    private RectTransform selfTransform; 

    public bool isScrolling = false;
    public const float MAX_SCALE = 2.0f;
    public const float MIN_SCALE = 0.2f;
    [Range(0.1f, 0.3f)]
    public float scale_speed;

    void Start(){
        scale_speed=0.1f;
        
        selfTransform = GetComponent<RectTransform>();
        selfTransform.sizeDelta = new Vector2(gridTransform.localScale.x * MapManager.MAP_SIZE, 
                                              gridTransform.localScale.y * MapManager.MAP_SIZE);
    }

    void Update()
    {
        float dir = Input.mouseScrollDelta.y;
        if(dir != 0){
            isScrolling = true;
            Vector3 new_scale = selfTransform.localScale + new Vector3(dir * scale_speed, dir * scale_speed, 0);
            
            new_scale.x = Mathf.Clamp(new_scale.x, MIN_SCALE, MAX_SCALE);
            new_scale.y = Mathf.Clamp(new_scale.y, MIN_SCALE, MAX_SCALE);

            selfTransform.localScale = new_scale;
        }
        else
            isScrolling = false;
    }

}
