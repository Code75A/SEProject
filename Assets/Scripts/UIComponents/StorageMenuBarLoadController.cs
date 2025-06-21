using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StorageMenuBarLoadController : MonoBehaviour
{
    public SpriteRenderer texture;
    public TextMeshPro text;

    public void Init(Sprite sprite, string name)
    {
        texture.sprite = sprite;
        text.text = name;
    }
    public void UpdateText(string newText)
    {
        text.text = newText;
    }
}
