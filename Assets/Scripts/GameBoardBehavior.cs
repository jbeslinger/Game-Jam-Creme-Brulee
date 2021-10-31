using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoardBehavior : MonoBehaviour
{
    #region Consts
    private const int BOARD_SIZE = 8;
    #endregion

    #region Fields
    public GameObject[,] pieces;
    #endregion

    #region Members
    [SerializeField]
    private GameObject _piecePrefab;
    #endregion

    #region Unity Methods
    private void Start()
    {
        if (_piecePrefab == null)
        {
            throw new NullReferenceException("Please provide the gamepiece prefab in the Debug inspector.");
        }
        else if (_piecePrefab.GetComponent<PieceBehavior>() == null)
        {
            throw new MissingComponentException("This GameObject doesn't have the PieceBehavior component.");
        }
        else
        {
            GenerateNewBoard();
        }
    }
    #endregion

    #region Methods
    public void SwapPieces(GameObject pieceA, GameObject pieceB)
    {
        (int, int) idxA = IndexOf(pieceA);
        (int, int) idxB = IndexOf(pieceB);
        
        Vector2 offset = this.gameObject.transform.position;
        Vector2 posA = new Vector2(idxA.Item1, -idxA.Item2) + offset;
        Vector2 posB = new Vector2(idxB.Item1, -idxB.Item2) + offset;

        this.pieces[idxA.Item1, idxA.Item2] = pieceB;
        this.pieces[idxB.Item1, idxB.Item2] = pieceA;

        pieceA.GetComponent<PieceBehavior>().MoveTo(posB);
        pieceB.GetComponent<PieceBehavior>().MoveTo(posA);

        //pieceA.transform.position = posB;
        //pieceB.transform.position = posA;
    }

    public (int, int) IndexOf(GameObject piece)
    {
        for (int r = 0; r < BOARD_SIZE; r++)
        {
            for (int c = 0; c < BOARD_SIZE; c++)
            {
                if (this.pieces[r, c] == piece)
                {
                    return (r, c);
                }
            }
        }
        return (-1, -1);
    }

    private void GenerateNewBoard()
    {
        ClearBoard();
        Vector2 offset = this.transform.position;
        for (int r = 0; r < BOARD_SIZE; r++)
        {
            for (int c = 0; c < BOARD_SIZE; c++)
            {
                GameObject newPiece = GameObject.Instantiate(_piecePrefab, this.transform);
                newPiece.transform.position = new Vector2(r, -c) + offset;
                this.pieces[r, c] = newPiece;
            }
        }
    }

    private void ClearBoard()
    {
        this.pieces = new GameObject[BOARD_SIZE, BOARD_SIZE];
    }
    #endregion
}
