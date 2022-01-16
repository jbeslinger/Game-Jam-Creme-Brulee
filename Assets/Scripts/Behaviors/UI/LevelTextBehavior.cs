using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LevelTextBehavior : MonoBehaviour
{
    #region Unity Methods
    private void Start()
    {
#if UNITY_EDITOR
        try
        {
            GetComponent<Text>().text = string.Format("Level {0}", SceneManager.SceneArgs["level"]);
        }
        catch
        {
            GetComponent<Text>().text = string.Format("Level {0}", 1);
        }
#else
        GetComponent<Text>().text = string.Format("Level {0}", SceneManager.SceneArgs["level"]);
#endif
    }
    #endregion
}
