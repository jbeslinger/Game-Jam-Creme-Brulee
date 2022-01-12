using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovesLeftTextBehavior : MonoBehaviour
{
    #region Fields
    public GameBoardBehavior board;
    public Text numberTextBox;
    #endregion

    #region Unity Methods
    private void Start()
    {
        if (numberTextBox == null)
        {
            throw new MissingReferenceException(string.Format("Please assign the {0} field in the inspector.", nameof(numberTextBox)));
        }
        if (board == null)
        {
            throw new MissingReferenceException(string.Format("Please assign the {0} field in the inspector.", nameof(board)));
        }
        board.OnMovesLeftChange += UpdateUI;
    }
    #endregion

    #region Methods
    private void UpdateUI(int movesLeft)
    {
        numberTextBox.text = movesLeft.ToString();
    }
    #endregion
}
