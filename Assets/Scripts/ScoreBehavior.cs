using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ScoreBehavior : MonoBehaviour
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
            board.OnScore += UpdateScore;
        }
    }
    #endregion

    #region Methods
    public void UpdateScore(int score)
    {
        _text.text = string.Format("SCORE: {0}", score);
    }
    #endregion

}
