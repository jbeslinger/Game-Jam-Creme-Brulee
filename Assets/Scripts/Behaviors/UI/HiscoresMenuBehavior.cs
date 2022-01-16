using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HiscoresMenuBehavior : MonoBehaviour
{
    #region Fields
    public Text[] playerNameLabels = new Text[10];
    public Text[] playerScoreLabels = new Text[10];
    #endregion

    #region Unity Methods
    private void OnEnable()
    {
        for (int i = 0; i < 10; i++)
        {
            playerNameLabels[i].text = "";
            playerScoreLabels[i].text = "";
        }
    }

    private void Start()
    {
        UpdateLabels();
    }
    #endregion

    #region Methods
    private void UpdateLabels()
    {
        int idx = 0;
        var hiscores = HiScoreManagerBehavior.GetHiScores();
        foreach ((string, int) player in hiscores)
        {
            if (hiscores[idx].Item1 != "")
            {
                playerNameLabels[idx].text = hiscores[idx].Item1;
                playerScoreLabels[idx].text = string.Format("{0:n0}", hiscores[idx].Item2);
                idx++;
            }
        }
    }
    #endregion
}
