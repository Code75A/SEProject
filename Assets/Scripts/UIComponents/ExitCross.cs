using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitCross : MonoBehaviour
{
    public GameObject parent;
    void OnMouseDown(){
        parent.SetActive(false);
    }
}
