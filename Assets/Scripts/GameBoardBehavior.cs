using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GameBoardBehavior : MonoBehaviour
{
    #region Consts
    private const int BOARD_SIZE = 8;
    private const int BASE_SCORE = 2;
    #endregion

    #region Enums
    private enum BoardState { READY, PAUSED, CHECKING, REFILLING }
    #endregion

    #region Properties
    public bool IsBusy { get => State != BoardState.READY; }

    private BoardState State
    {
        get => _state;
        set
        {
            if (_state == value)
            {
                return;
            }
            BoardState fromState = _state;
            _state = value;
            switch (_state)
            {
                case BoardState.READY:
                    if (_turnOver)
                    {
                        EndOfTurn();
                    }
                    break;
                case BoardState.PAUSED:
                    break;
                case BoardState.CHECKING:
                    break;
            }
        }
    }
    #endregion

    #region Fields
    public GameObject[,] pieces;
    public delegate void OnScoreDelegate(int score);
    public event OnScoreDelegate OnScore;
    #endregion

    #region Members
    private int _score = 0;

    private BoardState _state = BoardState.READY;

    [SerializeField] private GameObject _piecePrefab;
    private List<GameObject> _previewObjects = new List<GameObject>();
    private Stack<BoardTurn> _turns = new Stack<BoardTurn>();

    private bool _turnOver = false;
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
                    State = BoardState.PAUSED;
                    StartCoroutine(BreakMatchingBubbles(result, () => { State = BoardState.REFILLING; }));
                }
                break;
            case BoardState.REFILLING:
                State = BoardState.PAUSED;
                StartCoroutine(RefillBoard(() =>
                {
                    MatchResult result = GetMatches();
                    if (result.Valid)
                    {
                        State = BoardState.CHECKING;
                    }
                    else
                    {
                        State = BoardState.READY;
                    }
                }));
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
                if (GetPiece((r, c)) == piece)
                {
                    return (r, c);
                }
            }
        }
        return (-1, -1);
    }

    /// <summary>
    /// Shorter way of indexing the 2D array of gamepieces.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public GameObject GetPiece((int, int) index)
    {
        return pieces[index.Item1, index.Item2];
    }

    /// <summary>
    /// Shorthand way of setting a spot in the board's array to a piece.
    /// If piece is already on the board, it will set the previous index to null.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="piece"></param>
    public void SetPiece((int, int) index, GameObject piece)
    {
        (int, int) currentIndex = IndexOf(piece);
        // If the piece is in the board already, replace that index with null
        if (currentIndex != (-1, -1))
        {
            pieces[currentIndex.Item1, currentIndex.Item2] = null;
        }
        pieces[index.Item1, index.Item2] = piece;
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
        _turnOver = true;
    }

    /// <summary>
    /// Undoes the last turn the player took.
    /// </summary>
    public void UndoLastTurn()
    {
        // TODO: Make sure the player loses points by undoing if necessary.
        _turnOver = false;
        State = BoardState.PAUSED;
        StartCoroutine(_turns.Pop().UndoExecution(() => { State = BoardState.READY; }));
    }

    /// <summary>
    /// Adds points to the player's score on this board. Make the combo negative if you want to take points away.
    /// </summary>
    /// <param name="numberOfPieces"></param>
    /// <param name="combo"></param>
    private void AwardPoints(int numberOfPieces, int combo)
    {
        numberOfPieces -= 2;
        _score += (int) (Mathf.Pow(BASE_SCORE, numberOfPieces) * 100 * combo);
        if (OnScore != null)
        {
            OnScore(_score);
        }
    }

    /// <summary>
    /// Pops all bubbles in a MatchResult and then invokes the callback.
    /// </summary>
    /// <param name="matches"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    private IEnumerator BreakMatchingBubbles(MatchResult matches, Action callback)
    {
        List<PieceBehavior> piecesBeingDestroyed = new List<PieceBehavior>();
        int combo = 0;
        foreach(List<GameObject> match in matches.Matches)
        {
            AwardPoints(match.Count, combo + 1);
            foreach(GameObject piece in match)
            {
                (int, int) idx = IndexOf(piece);
                SetPiece(idx, null);
                PieceBehavior pb = piece.GetComponent<PieceBehavior>();
                piecesBeingDestroyed.Add(pb);
                pb.Break();
            }
            combo += 1;
        }

        yield return new WaitUntil(() => {
            foreach (PieceBehavior pb in piecesBeingDestroyed)
            {
                if (pb.animating)
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
                SetPiece((col, row), newPiece);
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
    /// Scans the whole board and adds new pieces while moving all pieces up to fill empty spots.
    /// </summary>
    private IEnumerator RefillBoard(Action callback)
    {
        List<PieceBehavior> movingPieces = new List<PieceBehavior>();
        int numberOfNewPiecesPerRow = 0;
        for (int col = 0; col < BOARD_SIZE; col++)
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                if (GetPiece((col, row)) == null)
                {
                    GameObject pieceToReplaceEmptySpace = null;
                    for (int i = row + 1; i < BOARD_SIZE; i++)
                    {
                        pieceToReplaceEmptySpace = GetPiece((col, i));
                        if (pieceToReplaceEmptySpace != null)
                        {
                            break;
                        }
                    }
                    if (pieceToReplaceEmptySpace == null)
                    {
                        pieceToReplaceEmptySpace = Instantiate(_piecePrefab, this.transform);
                        pieceToReplaceEmptySpace.GetComponent<PieceBehavior>().Type = (PieceBehavior.PieceType)UnityEngine.Random.Range(0, 7);
                        pieceToReplaceEmptySpace.transform.position = new Vector2(col, -(BOARD_SIZE + numberOfNewPiecesPerRow)) + (Vector2)transform.position;
                        numberOfNewPiecesPerRow += 1;
                    }
                    SetPiece((col, row), pieceToReplaceEmptySpace);
                    movingPieces.Add(pieceToReplaceEmptySpace.GetComponent<PieceBehavior>());
                    yield return new WaitForEndOfFrame();
                    pieceToReplaceEmptySpace.GetComponent<PieceBehavior>().MoveTo((col, row));
                }
            }
            numberOfNewPiecesPerRow = 0;
        }

        yield return new WaitUntil(() => {
            foreach (PieceBehavior pb in movingPieces)
            {
                if (pb.animating)
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
                    GameObject.Destroy(GetPiece((r, c)));
                }
            }
        }
        this.pieces = new GameObject[BOARD_SIZE, BOARD_SIZE];
    }

    private (int, int) AdjacentTo(GameObject piece, PushDirection directionOf)
    {
        return AdjacentTo(IndexOf(piece), directionOf);
    }

    private (int, int) AdjacentTo((int, int) idx, PushDirection directionOf)
    {
        (int, int) pieceAdjacentToThis = (-1, -1);
        switch (directionOf)
        {
            case PushDirection.UP:
                if (idx.Item2-1 >= 0)
                {
                    pieceAdjacentToThis = (idx.Item1, idx.Item2-1);
                }
                break;
            case PushDirection.RIGHT:
                if (idx.Item1+1 < BOARD_SIZE)
                {
                    pieceAdjacentToThis = (idx.Item1+1, idx.Item2);
                }
                break;
            case PushDirection.DOWN:
                if (idx.Item2+1 < BOARD_SIZE)
                {
                    pieceAdjacentToThis = (idx.Item1, idx.Item2+1);
                }
                break;
            case PushDirection.LEFT:
                if (idx.Item1-1 >= 0)
                {
                    pieceAdjacentToThis = (idx.Item1-1, idx.Item2);
                }
                break;
            case PushDirection.DIRECT:
                throw new ArgumentException("This is an invalid push direction to find the adjacent piece.");
        }
        return pieceAdjacentToThis;
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
                potentialMatch = CheckPiece(GetPiece((r, c)), map);
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
            (int, int) upIdx = AdjacentTo(piece, PushDirection.UP);
            if (upIdx != (-1, -1))
            {
                GameObject upPiece = GetPiece(upIdx);
                if (upPiece.GetComponent<PieceBehavior>().Type == myType)
                {
                    piecesOfSameType.AddRange(CheckPiece(upPiece, map));
                }
            }
            (int, int) rightIdx = AdjacentTo(piece, PushDirection.RIGHT);
            if (rightIdx != (-1, -1))
            {
                GameObject rightPiece = GetPiece(rightIdx);
                if (rightPiece.GetComponent<PieceBehavior>().Type == myType)
                {
                    piecesOfSameType.AddRange(CheckPiece(rightPiece, map));
                }
            }
            (int, int) downIdx = AdjacentTo(piece, PushDirection.DOWN);
            if (downIdx != (-1, -1))
            {
                GameObject downPiece = GetPiece(downIdx);
                if (downPiece.GetComponent<PieceBehavior>().Type == myType)
                {
                    piecesOfSameType.AddRange(CheckPiece(downPiece, map));
                }
            }
            (int, int) leftIdx = AdjacentTo(piece, PushDirection.LEFT);
            if (leftIdx != (-1, -1))
            {
                GameObject leftPiece = GetPiece(leftIdx);
                if (leftPiece.GetComponent<PieceBehavior>().Type == myType)
                {
                    piecesOfSameType.AddRange(CheckPiece(leftPiece, map));
                }
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
            (int, int) next = (-1, -1);
            for (int i = 0; i < 4; i++)
            {
                     if (((int)pushDir + i) % 4 == 0) { next = AdjacentTo(start, PushDirection.UP);     pushDir = PushDirection.UP; }
                else if (((int)pushDir + i) % 4 == 1) { next = AdjacentTo(start, PushDirection.RIGHT);  pushDir = PushDirection.RIGHT; }
                else if (((int)pushDir + i) % 4 == 2) { next = AdjacentTo(start, PushDirection.DOWN);   pushDir = PushDirection.DOWN; }
                else if (((int)pushDir + i) % 4 == 3) { next = AdjacentTo(start, PushDirection.LEFT);   pushDir = PushDirection.LEFT; }
                if (next != (-1, -1) && tempMap.IsUnmarked(next) && !foundFlag)
                {
                    if (!GetPiece(next).GetComponent<PieceBehavior>().Hardened)
                    {
                        pathVar.Push(new GameMove(start, pushDir, IndexOf(start)));
                        FindPath(GetPiece(next), end, pushDir, pathVar, tempMap, ref foundFlag);
                    }
                }
            }
        }
        if (!foundFlag)
        {
            pathVar.Pop();
        }
    }

    /// <summary>
    /// Randomly hardens a number of pieces.
    /// </summary>
    /// <param name="toFreeze">Max number of pieces you want to freeze.</param>
    /// <param name="chanceToFreeze">Normalized percent chance to freeze any piece.</param>
    private void RandomlyFreezePieces(int toFreeze, float chanceToFreeze)
    {
        for (int i = 0; i < toFreeze; i++)
        {
            float roll = (float)UnityEngine.Random.Range(1, 101) / 100f;
            if (roll <= chanceToFreeze)
            {
                (int, int) randomIdx = (UnityEngine.Random.Range(0, BOARD_SIZE), UnityEngine.Random.Range(0, BOARD_SIZE));
                GetPiece(randomIdx).GetComponent<PieceBehavior>().Hardened = true;
            }
        }
    }

    /// <summary>
    /// Called at the end of every turn.
    /// </summary>
    private void EndOfTurn()
    {
        Debug.Log("Turn ended.");

        // On the fifth turn, harden a piece no matter what to teach the player
        if (_turns.Count == 5)
        {
            RandomlyFreezePieces(1, 1.0f);
        }
        // From there on, follow a quadratic formula for chance and increase the number of pieces to
        // potentially freeze by 1 every 5 turns
        else if (_turns.Count > 5)
        {
            float chance = Mathf.Pow(_turns.Count, 2) / 8192;
            RandomlyFreezePieces(_turns.Count / 10, chance);
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
                _board.SetPiece(next.toIndex, next.pieceToMove);
                PieceBehavior pb = next.pieceToMove.GetComponent<PieceBehavior>();
                pb.MoveTo(next.toIndex);
                pieces.Add(pb);
            }

            yield return new WaitUntil(() => {
                foreach (PieceBehavior pb in pieces)
                {
                    if (pb.animating)
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
                _board.SetPiece(next.fromIndex, next.pieceToMove);
                PieceBehavior pb = next.pieceToMove.GetComponent<PieceBehavior>();
                pb.MoveTo(next.fromIndex);
                pieces.Add(pb);
            }

            yield return new WaitUntil(() => {
                foreach (PieceBehavior pb in pieces)
                {
                    if (pb.animating)
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
