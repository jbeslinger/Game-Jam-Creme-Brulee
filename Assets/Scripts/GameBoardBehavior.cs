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
            }
        }
    }

    private void ClearBoard()
    {
        this.pieces = new GameObject[BOARD_SIZE, BOARD_SIZE];
    }
    #endregion
}
