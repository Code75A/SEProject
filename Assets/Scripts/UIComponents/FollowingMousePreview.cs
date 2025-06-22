using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// 有一些显示方面的小瑕疵，需要Debug
public class FollowingMousePreview : MonoBehaviour
{
    public GameObject content;
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        gameObject.SetActive(false);
    }
    void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Color color = spriteRenderer.color;
        color.a = 0.5f;
        spriteRenderer.color = color;
    }

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.nearClipPlane;

        transform.localScale = content.transform.localScale;

        transform.position = Camera.main.ScreenToWorldPoint(mousePosition);
    }
}
