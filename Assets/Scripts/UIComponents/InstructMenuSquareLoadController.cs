using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InstructMenuSquareLoadController : MonoBehaviour
{
    public SpriteRenderer texture;
    public TextMeshProUGUI instructName;
    public MouseInteractManager.InstructTypes instruct_type;

    public void Init(MouseInteractManager.InstructTypes type,Sprite sprite){
        instruct_type = type;
        instructName.text = MouseInteractManager.Instance.CastInstructName(type);
        texture.sprite = sprite;
    }

}
