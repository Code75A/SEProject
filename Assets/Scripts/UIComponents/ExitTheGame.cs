using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ExitTheGame : MonoBehaviour
{
#if UNITY_EDITOR
    void OnMouseDown()
    {
        UnityEditor.EditorApplication.isPlaying = false;
    }
#else
    void OnMouseDown()
    {
        Application.Quit();
    }
#endif
}