using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HiScoreManagerBehavior : MonoBehaviour
{
    #region Fields
    public GameObject pbMessage, nameInputBox;
    public Text first, second, third;
    #endregion

    #region Unity Methods
    private void Start()
    {
        if (first == null || second == null || third == null)
        {
            throw new UnassignedReferenceException("Please assign the three Text UI components for the top three players.");
        }

        int lastScore = (int) SceneManager.SceneArgs["score_last_game"];

        string firstName = PlayerPrefs.GetString(nameof(firstName));
        int firstScore = PlayerPrefs.GetInt(nameof(firstScore));
        if (firstName != "")
        {
            first.text = string.Format("1. {0} - {1:n0}", firstName, firstScore);
        }

        string secondName = PlayerPrefs.GetString(nameof(secondName));
        int secondScore = PlayerPrefs.GetInt(nameof(secondScore));
        if (secondName != "")
        {
            second.text = string.Format("1. {0} - {1:n0}", secondName, secondScore);
        }

        string thirdName = PlayerPrefs.GetString(nameof(thirdName));
        int thirdScore = PlayerPrefs.GetInt(nameof(thirdScore));
        if (thirdName != "")
        {
            third.text = string.Format("1. {0} - {1:n0}", thirdName, thirdScore);
        }

        bool newHiScore = (lastScore > firstScore || lastScore > secondScore || lastScore > thirdScore);
        pbMessage.SetActive(newHiScore);
        nameInputBox.SetActive(newHiScore);
    }
    #endregion
}
