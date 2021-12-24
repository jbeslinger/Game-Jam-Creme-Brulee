using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class WinNotifyBehavior : MonoBehaviour
{
    #region Fields
    public GameBoardBehavior board;
    public Text winLoseLabel;
    #endregion

    #region Members
    private Animator _animator;

    private bool _gameIsOver = false;
    #endregion

    #region Unity Methods
    private void Start()
    {
        if (winLoseLabel == null)
        {
            throw new UnassignedReferenceException(string.Format("Please assign the {0} field in the inspector.", nameof(winLoseLabel)));
        }
        if (board == null)
        {
            throw new UnassignedReferenceException(string.Format("Please assign the {0} field in the inspector.", nameof(board)));
        }

        _animator = GetComponent<Animator>();

        board.OnWinConditionMet += () =>
        {
            _gameIsOver = false;
            winLoseLabel.text = "LEVEL UP!";
            _animator.SetTrigger("Level End");
        };

        board.OnGameOver += () =>
        {
            _gameIsOver = true;
            winLoseLabel.text = "GAME OVER";
            _animator.SetTrigger("Level End");
        };
    }
    #endregion

    #region Methods
    private void SlidePiecesOffBoardAnimation()
    {
        StartCoroutine(RandomlyMovePieces());
    }

    private IEnumerator RandomlyMovePieces()
    {
        Vector2 spotAboveTheBoard = new Vector2(0, 12);
        int boardSize = board.pieces.GetLength(0);
        for (int i = 0; i < boardSize / 2; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                GameObject pieceLeft = board.pieces[i, j];
                GameObject pieceRight = board.pieces[boardSize-1-i, j];

                yield return new WaitForSeconds(Random.Range(0.01f, 0.02f));

                spotAboveTheBoard.x = pieceLeft.transform.localPosition.x;
                PieceBehavior pb = pieceLeft.GetComponent<PieceBehavior>();
                pb.MoveTo(spotAboveTheBoard);

                spotAboveTheBoard.x = pieceRight.transform.localPosition.x;
                pb = pieceRight.GetComponent<PieceBehavior>();
                pb.MoveTo(spotAboveTheBoard);
            }
        }
        yield return null;
    }

    private void LoadNextLevel()
    {
        board.LoadNextLevel(_gameIsOver);
    }
    #endregion
}
