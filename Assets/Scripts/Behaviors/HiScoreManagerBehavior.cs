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

        int lastScore = PlayerPrefs.GetInt(PrefStrings.LAST_SCORE);

        string firstName = PlayerPrefs.GetString(PrefStrings.FIRST_PLAYER);
        int firstScore = PlayerPrefs.GetInt(PrefStrings.FIRST_PLAYER_SCORE);
        if (firstName != "")
        {
            first.text = string.Format("1. {0} - {1:n0}", firstName, firstScore);
        }

        string secondName = PlayerPrefs.GetString(PrefStrings.SECOND_PLAYER);
        int secondScore = PlayerPrefs.GetInt(PrefStrings.SECOND_PLAYER_SCORE);
        if (secondName != "")
        {
            second.text = string.Format("1. {0} - {1:n0}", secondName, secondScore);
        }

        string thirdName = PlayerPrefs.GetString(PrefStrings.THIRD_PLAYER);
        int thirdScore = PlayerPrefs.GetInt(PrefStrings.THIRD_PLAYER_SCORE);
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
