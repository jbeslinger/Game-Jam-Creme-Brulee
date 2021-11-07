using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoardBehavior : MonoBehaviour
{
    #region Enums
    public enum PushDirection { UP = 0, RIGHT = 1, DOWN = 2, LEFT = 3 };
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

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Z))
        {
            GenerateNewBoard();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            MatchResult result = GetMatches();
            foreach (List<GameObject> match in result.Matches)
            {
                foreach (GameObject piece in match)
                {
                    piece.GetComponent<SpriteRenderer>().color += new Color(0.5f, 0.5f, 0.5f);
                }
            }
        }*/
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

    private Queue<(GameObject, PushDirection)> pathToEmptySpot;
    /// <summary>
    /// Plays a preview animation of where the bubbles are going to move if the player pushes them.
    /// </summary>
    /// <param name="pieceToPush"></param>
    /// <param name="pushDir"></param>
    public void PreviewPlacement(GameObject pieceToPlace, GameObject pieceToPush, PushDirection pushDir)
    {
        Queue<(GameObject, PushDirection)> temp = new Queue<(GameObject, PushDirection)>(); // Var to hold our path to the end; runs backwards
        bool foundFlag = false; // Temp reference variable to tell the method we found the end
        FindPath(pieceToPush, pieceToPlace, pushDir, temp, new BoardMap(), ref foundFlag);
        
        // Clone the steps for later use
        pathToEmptySpot = new Queue<(GameObject, PushDirection)>(new Queue<(GameObject, PushDirection)>(temp));

        while (temp.Count != 0)
        {
            (GameObject, PushDirection) piece = temp.Dequeue();
            PieceBehavior pieceBehavior = piece.Item1.GetComponent<PieceBehavior>();
            pieceBehavior.PreviewMoveTo(piece.Item2, 0.5f);
        }
    }

    /// <summary>
    /// Cancels the current preview.
    /// </summary>
    public void RemovePreviewPlacement()
    {
        while (pathToEmptySpot.Count != 0)
        {
            (GameObject, PushDirection) piece = pathToEmptySpot.Dequeue();
            PieceBehavior pieceBehavior = piece.Item1.GetComponent<PieceBehavior>();
            pieceBehavior.RemovePreview();
        }
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
                newPieceBehavior.Type = (PieceBehavior.PieceType)UnityEngine.Random.Range(0, 7);
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
                    piece.GetComponent<PieceBehavior>().Type = (PieceBehavior.PieceType)UnityEngine.Random.Range(0, 7);
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
    /// Recursive method that finds the quickest path to the empty space for the bubble to fill.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="pathVar"></param>
    /// <returns></returns>
    private void FindPath(GameObject start, GameObject end, PushDirection pushDir, Queue<(GameObject, PushDirection)> pathVar, BoardMap tempMap, ref bool foundFlag)
    {
        (int,int) startIndex = IndexOf(start);
        tempMap.Mark(startIndex);
        (int,int) endIndex = IndexOf(end);
        if (startIndex == endIndex)
        {
            foundFlag = true;
            return;
        }
        else
        {
            // This for loop takes into account 'fromDir' which allows the order of operations to
            // prefer one direction over the others when considering the next step; preference moves clockwise.
            GameObject next = null;
            for (int i = 0; i < 4; i++)
            {
                     if (((int)pushDir + i) % 4 == 0) { next = UpOf(start); pushDir = PushDirection.UP; }
                else if (((int)pushDir + i) % 4 == 1) { next = RightOf(start); pushDir = PushDirection.RIGHT; }
                else if (((int)pushDir + i) % 4 == 2) { next = DownOf(start); pushDir = PushDirection.DOWN; }
                else if (((int)pushDir + i) % 4 == 3) { next = LeftOf(start); pushDir = PushDirection.LEFT; }
                if (next != null && tempMap.IsUnmarked(IndexOf(next)) && !foundFlag)
                {
                    pathVar.Enqueue((start, pushDir));
                    FindPath(next, end, pushDir, pathVar, tempMap, ref foundFlag);
                }
            }
        }
        if (!foundFlag)
        {
            pathVar.Dequeue();
        }
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
            PieceBehavior.PieceType myType = piece.GetComponent<PieceBehavior>().Type;
            piecesOfSameType.Add(piece);
            GameObject upPiece = UpOf(piece);
            if (upPiece != null && upPiece.GetComponent<PieceBehavior>().Type == myType)
            {
                piecesOfSameType.AddRange(CheckPiece(upPiece, map));
            }
            GameObject rightPiece = RightOf(piece);
            if (rightPiece != null && rightPiece.GetComponent<PieceBehavior>().Type == myType)
            {
                piecesOfSameType.AddRange(CheckPiece(rightPiece, map));
            }
            GameObject downPiece = DownOf(piece);
            if (downPiece != null && downPiece.GetComponent<PieceBehavior>().Type == myType)
            {
                piecesOfSameType.AddRange(CheckPiece(downPiece, map));
            }
            GameObject leftPiece = LeftOf(piece);
            if (leftPiece != null && leftPiece.GetComponent<PieceBehavior>().Type == myType)
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

    private class BoardMap
    {
        #region Members
        int[,] _map;
        #endregion

        #region Constructors
        public BoardMap()
        {
            _map = new int[BOARD_SIZE, BOARD_SIZE];
        }
        #endregion

        #region Methods
        public void Mark((int,int) location)
        {
            _map[location.Item1, location.Item2] = 1;
        }

        public bool IsUnmarked((int, int) location)
        {
            return _map[location.Item1, location.Item2] == 0;
        }
        #endregion
    }
    #endregion
}
