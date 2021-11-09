using System;
using UnityEngine;

public class GameMove
{
    #region Fields
    public GameObject pieceToMove;
    public PushDirection dirToMove;
    public readonly Vector2 previousPositionOfPiece, nextPositionOfPiece;
    public readonly (int, int) fromIndex, toIndex;
    #endregion

    #region Constructors
    /// <summary>
    /// For any piece that isn't being grabbed by the player, use this constructor.
    /// </summary>
    /// <param name="pieceToMove"></param>
    /// <param name="dirToMove"></param>
    /// <param name="currentIndex"></param>
    public GameMove(GameObject pieceToMove, PushDirection dirToMove, (int, int) currentIndex)
    {
        this.pieceToMove = pieceToMove ?? throw new ArgumentNullException(nameof(pieceToMove));
        this.dirToMove = dirToMove;
        this.fromIndex = currentIndex;
        switch (dirToMove)
        {
            case PushDirection.UP:
                toIndex = (currentIndex.Item1, currentIndex.Item2 - 1);
                break;
            case PushDirection.RIGHT:
                toIndex = (currentIndex.Item1 + 1, currentIndex.Item2);
                break;
            case PushDirection.DOWN:
                toIndex = (currentIndex.Item1, currentIndex.Item2 + 1);
                break;
            case PushDirection.LEFT:
                toIndex = (currentIndex.Item1 - 1, currentIndex.Item2);
                break;
            case PushDirection.DIRECT:
                break;
        }
        previousPositionOfPiece = new Vector2(fromIndex.Item1, -fromIndex.Item2);
        nextPositionOfPiece = new Vector2(toIndex.Item1, -toIndex.Item2);
    }

    /// <summary>
    /// Gross band-aid workaround constructor to make sure I can get the proper index of the grabbed piece.
    /// </summary>
    /// <param name="pieceToMove"></param>
    /// <param name="currentIndex"></param>
    /// <param name="toIndex"></param>
    public GameMove(GameObject pieceToMove, (int, int) currentIndex, (int, int) toIndex)
    {
        this.pieceToMove = pieceToMove ?? throw new ArgumentNullException(nameof(pieceToMove));
        this.dirToMove = PushDirection.DIRECT;
        this.fromIndex = currentIndex;
        this.toIndex = toIndex;
        previousPositionOfPiece = new Vector2(fromIndex.Item1, -fromIndex.Item2);
        nextPositionOfPiece = new Vector2(toIndex.Item1, -toIndex.Item2);
    }
    #endregion
}