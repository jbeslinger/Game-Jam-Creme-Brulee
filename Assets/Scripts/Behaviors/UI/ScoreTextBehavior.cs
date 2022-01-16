using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ScoreTextBehavior : MonoBehaviour
{
    #region Fields
    public GameBoardBehavior board;
    public ScoreFXTextBehavior scoreFx;
    #endregion

    #region Members
    private Text _text;

    private int _currentScore = 0; // Used to animate the scoreboard
    private int _targetScore = 0; // Used to animate the scoreboard
    private int _tickSpeed = 10; // Number of points to increment per frame
    #endregion

    #region Unity Methods
    private void Start()
    {
        _text = GetComponent<Text>();
        if (board == null)
        {
            throw new MissingReferenceException(string.Format("Please assign the {0} field in the inspector.", nameof(board)));
        }
        if (scoreFx == null)
        {
            throw new MissingReferenceException(string.Format("Please assign the {0} field in the inspector.", nameof(scoreFx)));
        }

#if UNITY_EDITOR
        try
        {
            _currentScore = (int)SceneManager.SceneArgs["score_last_game"];
        }
        catch
        {
            _currentScore = 0;
        }
#else
        _currentScore = (int)SceneManager.SceneArgs["score_last_game"];
#endif

        UpdateUI();

        board.OnScoreChange +=
            () =>
            {
                _targetScore = board.Score;
            };

        scoreFx.OnPointsAwarded +=
            (int points) =>
            {
                StopAllCoroutines();
                StartCoroutine(TickScore());
            };
    }
#endregion

#region Methods
    public void UpdateUI()
    {
        _text.text = string.Format("SCORE: {0:n0}", _currentScore);
    }

    private IEnumerator TickScore()
    {
        while (_currentScore <= _targetScore)
        {
            UpdateUI();
            _currentScore += _tickSpeed;
            yield return new WaitForEndOfFrame();
        }
        _currentScore = _targetScore;
        yield return null;
    }
#endregion
}
