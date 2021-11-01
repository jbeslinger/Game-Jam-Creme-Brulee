using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoardBehavior : MonoBehaviour
{
    #region Enums
    public enum PushDirection { UP, RIGHT, DOWN, LEFT };
    #endregion

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

    private List<GameObject> _previewObjects = new List<GameObject>();
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
    public void PushPieces(GameObject pieceA, GameObject pieceB, PushDirection pushDir)
    {
        /*
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
        */

        // TODO: Implement
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
    /// Plays a preview animation of where the bubbles are going to move if the player pushes them.
    /// </summary>
    /// <param name="pieceToPush"></param>
    /// <param name="pushDir"></param>
    public void PreviewPlacement(GameObject pieceToPush, PushDirection pushDir)
    {
        // TODO: Implement
    }

    /// <summary>
    /// Cancels the current preview.
    /// </summary>
    public void RemovePreviewPlacement()
    {
        // TODO: Implement
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
                this.pieces[r, c] = newPiece;
                newPiece.transform.position = new Vector2(r, -c) + offset;

                PieceBehavior newPieceBehavior = newPiece.GetComponent<PieceBehavior>();
                newPieceBehavior.pieceType = (PieceBehavior.PieceType)UnityEngine.Random.Range(0, 7);

                newPiece.name = newPieceBehavior.ToString();
            }
        }

        // Check for pre-emptive matches, fix them, and check until none are left
        MatchResult result = GetMatches();
        while (result.Valid)
        {
            foreach (List<GameObject> match in result.Matches)
            {
                foreach (GameObject piece in match)
                {
                    piece.GetComponent<PieceBehavior>().pieceType = (PieceBehavior.PieceType)UnityEngine.Random.Range(0, 7);
                }
            }
            result = GetMatches();
        }
    }

    /// <summary>
    /// Clears the board and destroys all piece GameObjects.
    /// </summary>
    private void ClearBoard()
    {
        if (this.pieces != null)
        {
            for (int r = 0; r < BOARD_SIZE; r++)
            {
                for (int c = 0; c < BOARD_SIZE; c++)
                {
                    GameObject.Destroy(this.pieces[r, c]);
                }
            }
        }
        this.pieces = new GameObject[BOARD_SIZE, BOARD_SIZE];
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

    /// <summary>
    /// Check entire board for valid matches, then build and return a MatchResult.
    /// </summary>
    /// <returns></returns>
    private MatchResult GetMatches()
    {
        MatchResult result = new MatchResult();
        int[,] map = new int[BOARD_SIZE, BOARD_SIZE]; // Empty int map to keep track of where we checked

        List<GameObject> potentialMatch = new List<GameObject>();

        for (int r = 0; r < BOARD_SIZE; r++)
        {
            for (int c = 0; c < BOARD_SIZE; c++)
            {
                potentialMatch = CheckPiece(pieces[r, c], map);
                if (potentialMatch.Count >= 3)
                {
                    result.AddMatch(potentialMatch);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Recursive method to check all cardinal directions of provided piece and return a list of matching colors.
    /// </summary>
    /// <param name="piece"></param>
    /// <returns></returns>
    private List<GameObject> CheckPiece(GameObject piece, int[,] map)
    {
        List<GameObject> piecesOfSameType = new List<GameObject>();
        (int, int) idx = IndexOf(piece);
        if (map[idx.Item1, idx.Item2] == 0)
        {
            map[idx.Item1, idx.Item2] = 1;
            PieceBehavior.PieceType myType = piece.GetComponent<PieceBehavior>().pieceType;
            piecesOfSameType.Add(piece);
            GameObject upPiece = UpOf(piece);
            if (upPiece != null && upPiece.GetComponent<PieceBehavior>().pieceType == myType)
            {
                piecesOfSameType.AddRange(CheckPiece(upPiece, map));
            }
            GameObject rightPiece = RightOf(piece);
            if (rightPiece != null && rightPiece.GetComponent<PieceBehavior>().pieceType == myType)
            {
                piecesOfSameType.AddRange(CheckPiece(rightPiece, map));
            }
            GameObject downPiece = DownOf(piece);
            if (downPiece != null && downPiece.GetComponent<PieceBehavior>().pieceType == myType)
            {
                piecesOfSameType.AddRange(CheckPiece(downPiece, map));
            }
            GameObject leftPiece = LeftOf(piece);
            if (leftPiece != null && leftPiece.GetComponent<PieceBehavior>().pieceType == myType)
            {
                piecesOfSameType.AddRange(CheckPiece(leftPiece, map));
            }
        }
        return piecesOfSameType;
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
        public List<List<GameObject>> Matches { get => _matches; }
        #endregion

        #region Members
        private List<List<GameObject>> _matches;
        #endregion

        #region Constructors
        public MatchResult()
        {
            _matches = new List<List<GameObject>>();
        }
        #endregion

        #region Methods
        public void AddMatch(List<GameObject> pieces)
        {
            _matches.Add(pieces);
        }
        #endregion
    }
    #endregion
}
