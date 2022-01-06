using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ProgressBarBehavior : MonoBehaviour
{
    #region Constants
    /// <summary>
    /// How long the progress bar linearly interpolates to its target height; normalized value.
    /// </summary>
    private const float PROGRESS_BAR_SPEED = 10f;
    private const float START_Y_POS = -8.75f;
    private const float END_Y_POS = -1.0f;
    #endregion

    #region Properties
    private float CurrentProgress { get => _currentProgress; set => _currentProgress = Mathf.Clamp01(value); }
    #endregion

    #region Fields
    public GameBoardBehavior board;
    #endregion

    #region Members
    SpriteRenderer _spriteRenderer;

    /// <summary>
    /// The total score that the player had at the beginning of the level.
    /// </summary>
    private float _totalScoreLastFrame;
    /// <summary>
    /// The running score on this level.
    /// </summary>
    private float _scoreThisLevel;
    /// <summary>
    /// The number of points to win minus the player's total score at the beginning of the level.
    /// </summary>
    private float _pointsToWinThisLevel;

    private float _progressBeforeLerp = 0.0f;
    /// <summary>
    /// The percentage that the progress bar has climbed this frame.
    /// </summary>
    private float _currentProgress = 0.0f;
    /// <summary>
    /// The percentage that the progress bar is currently climbing to.
    /// </summary>
    private float _targetProgress = 0.0f;
    #endregion

    #region Events
    private delegate void OnProgressBarRaiseDelegate();
    private event OnProgressBarRaiseDelegate OnProgressBarRaise;
    #endregion

    #region Unity Methods
    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (board == null)
        {
            throw new UnassignedReferenceException(string.Format("Please assign the {0} field in the inspector.", nameof(board)));
        }

        _totalScoreLastFrame = board.Score;
        _pointsToWinThisLevel = board.PointsToWin - _totalScoreLastFrame;

        board.OnScoreChange += ChangeProgress;

        OnProgressBarRaise +=
        () =>
        {
            Vector2 newPos = Vector2.Lerp(new Vector2(transform.localPosition.x, START_Y_POS), new Vector2(transform.localPosition.x, END_Y_POS), Ease.EaseOut(CurrentProgress));
            transform.localPosition = newPos;
        };
    }
    #endregion

    #region Methods
    private void ChangeProgress()
    {
        _scoreThisLevel = board.Score - _totalScoreLastFrame;
        _targetProgress = _scoreThisLevel / _pointsToWinThisLevel;
        _progressBeforeLerp = CurrentProgress;
        StopAllCoroutines();
        StartCoroutine(RaiseProgressBar());
    }

    private IEnumerator RaiseProgressBar()
    {
        while (Round(CurrentProgress, 1) < Round(_targetProgress, 1))
        {
            CurrentProgress += Time.deltaTime / PROGRESS_BAR_SPEED;            
            OnProgressBarRaise?.Invoke();
            yield return null;
        }

        CurrentProgress = _targetProgress;
        OnProgressBarRaise?.Invoke();
        yield return null;
    }

    private float Round(float toRound, uint precision)
    {
        return Mathf.Round(toRound * 100f) * (1 * Mathf.Pow(10, -precision));
    }

    private IEnumerator AnimateColor()
    {

        while (true)
        {
            yield return null;
        }

        yield return null;
    }
    #endregion
}
