using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class EndScoreTextBehavior : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Text>().text = string.Format("{0:n0}", PlayerPrefs.GetInt(PrefStrings.LAST_SCORE));
    }
}
