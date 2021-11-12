using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoardBehavior : MonoBehaviour
{
    #region Consts
    private const int BOARD_SIZE = 8;
    private const int POINT_REWARD = 200;
    #endregion

    #region Enums
    private enum BoardState { READY, PAUSED, CHECKING }
    #endregion

    #region Properties
    private BoardState State { get => _state; set => ChangeState(value); }
    #endregion

    #region Fields
    public GameObject[,] pieces;
    #endregion

    #region Members
    private BoardState _state = BoardState.READY;

    [SerializeField] private GameObject _piecePrefab;
    private List<GameObject> _previewObjects = new List<GameObject>();
    private Stack<BoardTurn> _turns = new Stack<BoardTurn>();
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
        switch (State)
        {
            case BoardState.READY:
                break;
            case BoardState.PAUSED:
                break;
            case BoardState.CHECKING:
                MatchResult result = GetMatches();
                if (!result.Valid)
                {
                    UndoLastTurn();
                }
                else
                {
                    /* TEMP */ State = BoardState.READY;
                    // TODO: Break the bubbles here
                }
                break;
        }

        
    }
    #endregion

    #region Methods
    /// <summary>
    /// Returns the x and y index of the provided piece.
    /// </summary>
    /// <param name="piece"></param>
    /// <returns></returns>
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

    private Stack<GameMove> _pathToEmptySpot;
    /// <summary>
    /// Plays a preview animation of where the bubbles are going to move if the player pushes them.
    /// </summary>
    /// <param name="pieceToPush"></param>
    /// <param name="pushDir"></param>
    public void PreviewPlacement(GameObject pieceToPlace, GameObject pieceToPush, PushDirection pushDir)
    {
        Stack<GameMove> temp = new Stack<GameMove>(); // Var to hold our path to the end; runs backwards
        bool foundFlag = false; // Temp reference variable to tell the method we found the end
        FindPath(pieceToPush, pieceToPlace, pushDir, temp, new BoardMap(), ref foundFlag);
        
        // Clone the steps for later use
        _pathToEmptySpot = new Stack<GameMove>(new Stack<GameMove>(temp));
        while (temp.Count != 0)
        {
            GameMove move = temp.Pop();
            move.pieceToMove.GetComponent<PieceBehavior>().PreviewMoveTo(move.dirToMove, 0.75f);
        }
    }

    /// <summary>
    /// Cancels the current preview.
    /// </summary>
    public void RemovePreviewPlacement()
    {
        Stack<GameMove> temp = new Stack<GameMove>(new Stack<GameMove>(_pathToEmptySpot));
        while (temp.Count != 0)
        {
            temp.Pop().pieceToMove.GetComponent<PieceBehavior>().RemovePreview();
        }
    }

    /// <summary>
    /// Perform the move requested.
    /// </summary>
    public void PerformMove()
    {
        if (_pathToEmptySpot == null || _pathToEmptySpot.Count <= 0)
        {
            return;
        }
        State = BoardState.PAUSED;
        BoardTurn nextTurn = new BoardTurn(_pathToEmptySpot, this);
        StartCoroutine(nextTurn.Execute(() => { State = BoardState.CHECKING; }));
        _turns.Push(nextTurn);
    }

    /// <summary>
    /// Undoes the last turn the player took.
    /// </summary>
    public void UndoLastTurn()
    {
        // TODO: Make sure the player loses points by undoing if necessary.
        State = BoardState.PAUSED;
        StartCoroutine(_turns.Pop().UndoExecution(() => { State = BoardState.READY; }));
    }

    /// <summary>
    /// Generate a new board and guarantee there will be at most 2 pieces of same color touching.
    /// </summary>
    private void GenerateNewBoard()
    {
        ClearBoard();
        Vector2 offset = this.transform.position;
        for (int col = 0; col < BOARD_SIZE; col++)
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                GameObject newPiece = GameObject.Instantiate(_piecePrefab, this.transform);
                this.pieces[col, row] = newPiece;
                newPiece.transform.position = new Vector2(col, -row) + offset;

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

    GameObject firstPiece;
    /// <summary>
    /// Recursive method that finds the quickest path to the empty space for the bubble to fill.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="pathVar"></param>
    /// <returns></returns>
    private void FindPath(GameObject start, GameObject end, PushDirection pushDir, Stack<GameMove> pathVar, BoardMap tempMap, ref bool foundFlag)
    {
        if (firstPiece == null)
        {
            firstPiece = start;
        }
        (int,int) startIndex = IndexOf(start);
        tempMap.Mark(startIndex);
        (int,int) endIndex = IndexOf(end);
        if (startIndex == endIndex)
        {
            // We know we're back to the piece that was grabbed, so we set direction to Direct
            pathVar.Push(new GameMove(start, endIndex, IndexOf(firstPiece)));
            foundFlag = true;
            firstPiece = null;
            return;
        }
        else
        {
            // This for loop takes into account 'fromDir' which allows the order of operations to
            // prefer one direction over the others when considering the next step; preference moves clockwise.
            GameObject next = null;
            for (int i = 0; i < 4; i++)
            {
                     if (((int)pushDir + i) % 4 == 0) { next = UpOf(start);     pushDir = PushDirection.UP; }
                else if (((int)pushDir + i) % 4 == 1) { next = RightOf(start);  pushDir = PushDirection.RIGHT; }
                else if (((int)pushDir + i) % 4 == 2) { next = DownOf(start);   pushDir = PushDirection.DOWN; }
                else if (((int)pushDir + i) % 4 == 3) { next = LeftOf(start);   pushDir = PushDirection.LEFT; }
                if (next != null && tempMap.IsUnmarked(IndexOf(next)) && !foundFlag)
                {
                    pathVar.Push(new GameMove(start, pushDir, IndexOf(start)));
                    FindPath(next, end, pushDir, pathVar, tempMap, ref foundFlag);
                }
            }
        }
        if (!foundFlag)
        {
            pathVar.Pop();
        }
    }
    
    /// <summary>
    /// Called by the property 'State' at add some entry/exit functionality.
    /// </summary>
    /// <param name="newState"></param>
    private void ChangeState(BoardState newState)
    {
        if (_state == newState)
        {
            return;
        }
        BoardState fromState = _state;
        _state = newState;
        switch (State)
        {
            case BoardState.READY:
                break;
            case BoardState.PAUSED:
                break;
            case BoardState.CHECKING:
                break;
        }
    }
    #endregion


    #region Subclasses
    private class BoardTurn
    {
        #region Members
        /// <summary>
        /// Each layer contains a reference to a piece, the direction it was pushed,
        /// and its original location.
        /// </summary>
        private readonly Stack<GameMove> _moves;
        private readonly GameBoardBehavior _board;
        #endregion

        #region Constructors
        public BoardTurn(Stack<GameMove> moves, in GameBoardBehavior board)
        {
            _moves = moves;
            _board = board;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Calls MoveTo() on each piece in _moves, then invokes the callback method when the pieces are done moving.
        /// </summary>
        public IEnumerator Execute(Action callback)
        {
            List<PieceBehavior> pieces = new List<PieceBehavior>();
            // Clone the original data set so we can keep it for later.
            Stack<GameMove> temp = new Stack<GameMove>(new Stack<GameMove>(_moves));
            while (temp.Count > 0)
            {
                GameMove next = temp.Pop();
                _board.pieces[next.toIndex.Item1, next.toIndex.Item2] = next.pieceToMove;
                PieceBehavior pb = next.pieceToMove.GetComponent<PieceBehavior>();
                pb.MoveTo(next.nextPositionOfPiece);
                pieces.Add(pb);
            }

            yield return new WaitUntil(() => {
                foreach (PieceBehavior pb in pieces)
                {
                    if (pb.IsAnimating())
                    {
                        return false;
                    }
                }
                return true;
            });

            callback?.Invoke();
            yield return null;
        }

        /// <summary>
        /// Calls MoveTo() on each piece with its original location. Also brings pieces back from the dead.
        /// </summary>
        public IEnumerator UndoExecution(Action callback)
        {
            List<PieceBehavior> pieces = new List<PieceBehavior>();
            while (_moves.Count > 0)
            {
                GameMove next = _moves.Pop();
                _board.pieces[next.fromIndex.Item1, next.fromIndex.Item2] = next.pieceToMove;
                PieceBehavior pb = next.pieceToMove.GetComponent<PieceBehavior>();
                pb.MoveTo(next.previousPositionOfPiece);
                pieces.Add(pb);
            }

            yield return new WaitUntil(() => {
                foreach (PieceBehavior pb in pieces)
                {
                    if (pb.IsAnimating())
                    {
                        return false;
                    }
                }
                return true;
            });

            callback?.Invoke();
            yield return null;
        }
        #endregion
    }

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

    /// <summary>
    /// This class is used in pathfinding as a easily readible way of keeping track
    /// what paths have already been traversed.
    /// </summary>
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
