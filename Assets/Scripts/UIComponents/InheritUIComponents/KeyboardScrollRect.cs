using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeyboardScrollRect : ScrollRect
{
    public bool allowKeyboardInput = true;

    const float SCROLL_SPEED = 0.4f; 

    public override void OnScroll(PointerEventData data)
    {
        //base.OnScroll(data);
    }
    protected override void Start()
    {
        horizontal = false;
        vertical = false;
    }

    void Update()
    {
        if (!allowKeyboardInput) return;

        float extra_speed = 1.0f;

        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            extra_speed = 2.0f;
        }

        float ver = Input.GetAxisRaw("Vertical"); 

        if (Mathf.Abs(ver) > 0.01f)
        {
            verticalNormalizedPosition += ver * Time.deltaTime * SCROLL_SPEED * extra_speed;
            verticalNormalizedPosition = Mathf.Clamp01(verticalNormalizedPosition);

        }

        float hor = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(hor) > 0.01f)
        {
            horizontalNormalizedPosition += hor * Time.deltaTime * SCROLL_SPEED * extra_speed; 
            horizontalNormalizedPosition = Mathf.Clamp01(horizontalNormalizedPosition);
        }
    }
}
