using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class WinScoreTextBehavior : MonoBehaviour
{
    #region Fields
    public GameBoardBehavior board;
    #endregion

    #region Members
    private Text _text;
    #endregion

    #region Unity Methods
    private void Start()
    {
        _text = GetComponent<Text>();
        if (board == null)
        {
            throw new MissingReferenceException("Please assign a GameBoardBehavior to the field.");
        }
        else
        {
            board.OnWinConditionChange += UpdateWinCondition;
        }
    }
    #endregion

    #region Methods
    public void UpdateWinCondition(int pointsToWin)
    {
        _text.text = string.Format("TO WIN: {0:n0}", pointsToWin);
    }
    #endregion
}
