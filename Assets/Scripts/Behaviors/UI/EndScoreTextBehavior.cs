using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class EndScoreTextBehavior : MonoBehaviour
{
    private void Start()
    {
#if UNITY_EDITOR
        try
        {
            GetComponent<Text>().text = string.Format("{0:n0}", (int)SceneManager.SceneArgs["score_last_game"]);
        }
        catch
        {
            GetComponent<Text>().text = string.Format("{0:n0}", 69000);
        }
#else
        GetComponent<Text>().text = string.Format("{0:n0}", (int)SceneManager.SceneArgs["score_last_game"]);
#endif
    }
}
