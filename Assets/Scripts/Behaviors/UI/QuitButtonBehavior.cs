using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitButtonBehavior : MonoBehaviour
{
    /// <summary>
    /// Exit the application or playmode if in editor.
    /// </summary>
    public void onClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
