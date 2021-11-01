using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoardBehavior : MonoBehaviour
{
    #region Consts
    private const int BOARD_SIZE = 8;
    private const int POINT_REWARD = 200;
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

    /// <summary>
    /// Generate a new board and guarantee there will be at most 2 pieces of same color touching.
    /// </summary>
    private void GenerateNewBoard()
    {
        ClearBoard();
        Vector2 offset = this.transform.position;
        for (int r = 0; r < BOARD_SIZE; r++)
        {
            for (int c = 0; c < BOARD_SIZE; c++)
            {
                GameObject newPiece = GameObject.Instantiate(_piecePrefab, this.transform);
                newPiece.GetComponent<PieceBehavior>().type = (PieceBehavior.PieceType)UnityEngine.Random.Range(0, 7);
                newPiece.transform.position = new Vector2(r, -c) + offset;
                this.pieces[r, c] = newPiece;
            }
        }
    }

    private void ClearBoard()
    {
        this.pieces = new GameObject[BOARD_SIZE, BOARD_SIZE];
    }

    /// <summary>
    /// Check entire board for valid matches, then build and return a MatchResult.
    /// </summary>
    /// <returns></returns>
    private MatchResult GetMatches()
    {
        // TODO: implement
        MatchResult result = new MatchResult();
        return result;
    }

    /// <summary>
    /// Returns the GameObject above the supplied piece. Returns null if OOB.
    /// </summary>
    /// <param name="piece"></param>
    private GameObject UpOf(GameObject piece)
    {
        GameObject result = null;
        (int, int) idx = IndexOf(piece);
        idx.Item2 -= 1;
        if (idx.Item2 >= 0)
        {
            result = pieces[idx.Item1, idx.Item2];
        }
        return result;
    }

    /// <summary>
    /// Returns the GameObject to the right of the supplied piece. Returns null if OOB.
    /// </summary>
    /// <param name="piece"></param>
    private GameObject RightOf(GameObject piece)
    {
        GameObject result = null;
        (int, int) idx = IndexOf(piece);
        idx.Item1 += 1;
        if (idx.Item1 < BOARD_SIZE)
        {
            result = pieces[idx.Item1, idx.Item2];
        }
        return result;
    }

    /// <summary>
    /// Returns the GameObject to the left of the supplied piece. Returns null if OOB.
    /// </summary>
    /// <param name="piece"></param>
    private GameObject LeftOf(GameObject piece)
    {
        GameObject result = null;
        (int, int) idx = IndexOf(piece);
        idx.Item1 -= 1;
        if (idx.Item1 >= 0)
        {
            result = pieces[idx.Item1, idx.Item2];
        }
        return result;
    }

    /// <summary>
    /// Returns the GameObject below the supplied piece. Returns null if OOB.
    /// </summary>
    /// <param name="piece"></param>
    private GameObject DownOf(GameObject piece)
    {
        GameObject result = null;
        (int, int) idx = IndexOf(piece);
        idx.Item2 += 1;
        if (idx.Item2 < BOARD_SIZE)
        {
            result = pieces[idx.Item1, idx.Item2];
        }
        return result;
    }
    #endregion

    #region Subclasses
    /// <summary>
    /// This class will tell the GameBoard whether or not the player just made a valid move
    /// </summary>
    private class MatchResult
    {
        #region Properties
        public bool Valid { get => Matches.Count > 0; }
        public List<GameObject[]> Matches { get => _matches; }
        #endregion

        #region Members
        private List<GameObject[]> _matches;
        #endregion

        #region Constructors
        public MatchResult()
        {
            _matches = new List<GameObject[]>();
        }
        #endregion

        #region Methods
        public void AddMatch(GameObject[] pieces)
        {
            _matches.Add(pieces);
        }
        #endregion
    }
    #endregion
}
