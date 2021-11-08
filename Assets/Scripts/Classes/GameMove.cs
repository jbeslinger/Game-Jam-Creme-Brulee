using System;
using UnityEngine;

public class GameMove
{
    #region Fields
    public GameObject pieceToMove;
    public PushDirection dirToMove;
    public readonly Vector2 previousLocationOfPiece;
    #endregion

    #region Constructors
    public GameMove(GameObject pieceToMove, PushDirection dirToMove)
    {
        this.pieceToMove = pieceToMove ?? throw new ArgumentNullException(nameof(pieceToMove));
        this.dirToMove = dirToMove;
        this.previousLocationOfPiece = pieceToMove.transform.position;
    }
    #endregion

    #region Methods
    public GameMove GetOpposite()
    {
        PushDirection oppositeDirection = 0;
        switch (dirToMove)
        {
            case PushDirection.UP:
                oppositeDirection = PushDirection.DOWN;
                break;
            case PushDirection.RIGHT:
                oppositeDirection = PushDirection.LEFT;
                break;
            case PushDirection.DOWN:
                oppositeDirection = PushDirection.UP;
                break;
            case PushDirection.LEFT:
                oppositeDirection = PushDirection.RIGHT;
                break;
        }
        return new GameMove(pieceToMove, oppositeDirection);
    }
    #endregion
}