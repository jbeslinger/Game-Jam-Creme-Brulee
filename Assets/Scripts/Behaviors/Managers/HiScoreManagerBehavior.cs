using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HiScoreManagerBehavior : MonoBehaviour
{
    #region Fields
    public GameObject pbMessage, nameInputBox, saveButton;
    #endregion

    #region Unity Methods
    private void Start()
    {
        bool newHiScore = HiScoreManagerBehavior.IsNewHiScore((int)SceneManager.SceneArgs["score_last_game"]) >= 0;
        pbMessage.SetActive(newHiScore);
        nameInputBox.SetActive(newHiScore);
        saveButton.SetActive(newHiScore);
    }
    #endregion

    #region Members
    public static List<(string, int)> GetHiScores()
    {
        int numberOfHiscores = 10;
        List<(string, int)> hiscores = new List<(string, int)>(numberOfHiscores);
        for (int i = 0; i < numberOfHiscores; i++)
        {
            string playerName = PlayerPrefs.GetString(string.Format("player{0}Name", i));
            int playerScore = PlayerPrefs.GetInt(string.Format("player{0}Score", i));
            hiscores.Add((playerName, playerScore));
        }
        return hiscores;
    }

    /// <summary>
    /// Save the hiscore in the database based.
    /// </summary>
    /// <param name="score"></param>
    /// <param name="rank"></param>
    public static void SaveHiScore(string name, int score, int rank)
    {
        if (rank > 9)
        {
            throw new PlayerPrefsException(string.Format("There are only 10 hiscore entries; {0} is invalid.", rank));
        }
        
        var hiscores = GetHiScores();
        hiscores.Insert(rank, (name, score));

        for (int i = 0; i < 10; i++)
        {
            PlayerPrefs.SetString(string.Format("player{0}Name", i), hiscores[i].Item1);
            PlayerPrefs.SetInt(string.Format("player{0}Score", i), hiscores[i].Item2);
        }
    }

    /// <summary>
    /// Returns the index of the new ranking for the provided score based on the top ten table. Returns -1 if the score provided is not a hiscore.
    /// </summary>
    /// <param name="score"></param>
    /// <returns></returns>
    public static int IsNewHiScore(int score)
    {
        var hiscores = GetHiScores();
        int i = -1;
        
        if (score > hiscores[9].Item2)
        {
            for (i = 0; i < 10; i++)
            {
                if (score > hiscores[i].Item2)
                {
                    break;
                }
            }
        }

        return i;
    }
    #endregion
}
