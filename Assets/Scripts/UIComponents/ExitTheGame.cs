using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ExitTheGame : MonoBehaviour
{
#if UNITY_EDITOR
    void OnMouseDown()
    {
        // If the game is running in the editor, stop playing
        UnityEditor.EditorApplication.isPlaying = false;
    }
#else
    void OnMouseDown()
    {
        // If the game is running as a standalone application, quit the application
        Application.Quit();
    }
#endif
}