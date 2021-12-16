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
        GetComponent<Text>().text = string.Format("Level {0}", SceneManager.SceneArgs["level"]);
    }
#endregion
}
