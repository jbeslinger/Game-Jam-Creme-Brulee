using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class GameBoardBehavior : MonoBehaviour
{
    #region Consts
    private const int BOARD_SIZE = 8; // 8 x 8 board size
    private const int BASE_SCORE = 2; // Base score of 2 = 200 points for 3 pieces, 2^2*100 for 4
    private const int MAX_MOVES_LEFT = 3;
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
                    if (_turnIsOver)
                    {
                        MovesLeft = MAX_MOVES_LEFT;
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
    public int Score
    {
        get => _score;
        set
        {
            if (_score != value)
            {
                OnScoreChange?.Invoke();
                _score = value;
            }
        }
    }
    public int PointsToWin
    {
        get => _pointsToWin;
        set
        {
            if (OnWinConditionChange != null)
            {
                OnWinConditionChange(value);
            }
            else
            {
                Debug.LogError("You never subscribed to the OnWinConditionChange event, jackass.");
            }
            _pointsToWin = value;
        }
    }
    public int MovesLeft
    {
        get => _movesLeft;
        set
        {
            if (OnMovesLeftChange != null)
            {
                OnMovesLeftChange(value);
            }
            if (value <= 0)
            {
                EndGame();
            }
            _movesLeft = value;
        }
    }
    #endregion

    #region Events
    public delegate void OnScoreChangeDelegate();
    public event OnScoreChangeDelegate OnScoreChange;

    public delegate void OnWinConditionMetDelegate();
    public OnWinConditionMetDelegate OnWinConditionMet;

    public delegate void OnWinConditionChangeDelegate(int pointsToWin);
    public OnWinConditionChangeDelegate OnWinConditionChange;

    public delegate void OnMovesLeftChangeDelegate(int movesLeft);
    public OnMovesLeftChangeDelegate OnMovesLeftChange;

    public delegate void OnPointsAwardedDelegate(int pointsAwarded);
    public OnPointsAwardedDelegate OnPointsAwarded;

    public delegate void OnGameOverDelegate();
    public OnGameOverDelegate OnGameOver;
    #endregion

    #region Fields
    public GameObject[,] pieces;
    #endregion

    #region Members
    private int _pointsToWin;
    private int _currentLevel;
    private int _score;
    private int _movesLeft;
    private int _hardenedPieces = 0;

    private BoardState _state = BoardState.READY;

    [SerializeField] private GameObject _piecePrefab;
    private List<GameObject> _previewObjects = new List<GameObject>();
    private Stack<BoardTurn> _turns = new Stack<BoardTurn>();

    private bool _turnIsOver = false;
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
#if UNITY_EDITOR
            try
            {
                _currentLevel = (int)SceneManager.SceneArgs["level"];
                PointsToWin = (int)SceneManager.SceneArgs["win_score"];
                int hardPieces = (int)SceneManager.SceneArgs["hard_pieces"];
                GenerateNewBoard(hardPieces);
                Score = (int)SceneManager.SceneArgs["score_last_game"];
            }
            catch
            {
                _currentLevel = 1;
                PointsToWin = 11000;
                int hardPieces = 0;
                GenerateNewBoard(hardPieces);
                Score = 0;
            }
#else
            _currentLevel = (int) SceneManager.SceneArgs["level"];
            PointsToWin = (int) SceneManager.SceneArgs["win_score"];
            int hardPieces = (int) SceneManager.SceneArgs["hard_pieces"];
            GenerateNewBoard(hardPieces);
            Score = (int)SceneManager.SceneArgs["score_last_game"];
#endif
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.W))
        {
            WinLevel();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            EndGame();
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            RandomlyHardenPieces(1, 1.0f);
        }
#endif
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
        Stack<GameMove> path = FindPath(pieceToPush, pieceToPlace);
        
        // Clone the steps for later use
        _pathToEmptySpot = new Stack<GameMove>(new Stack<GameMove>(path));
        while (path.Count != 0)
        {
            GameMove move = path.Pop();
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
        if (_pathToEmptySpot == null || _pathToEmptySpot.Count == 0)
        {
            return;
        }

        State = BoardState.PAUSED;
        BoardTurn nextTurn = new BoardTurn(_pathToEmptySpot, this);
        StartCoroutine(nextTurn.Execute(() => { State = BoardState.CHECKING; }));
        _turns.Push(nextTurn);
        _turnIsOver = true;
    }

    /// <summary>
    /// Undoes the last turn the player took.
    /// </summary>
    public void UndoLastTurn()
    {
        // TODO: Make sure the player loses points by undoing if necessary.
        _turnIsOver = false;
        State = BoardState.PAUSED;
        StartCoroutine(_turns.Pop().UndoExecution(() => { State = BoardState.READY; MovesLeft -= 1; }));
    }

    /// <summary>
    /// Called by animation handlers to transition to the next scene of the game when the scene transition animation is over.
    /// </summary>
    /// <param name="gameIsOver"></param>
    public void LoadNextLevel(bool gameIsOver)
    {
        if (gameIsOver)
        {
            Hashtable args = new Hashtable();
            args.Add("score_last_game", Score);
            SceneManager.LoadScene("GameEndScene", args);
        }
        else
        {
            SceneManager.LoadScene("GameScene", GameManager.NewLevel(_currentLevel + 1, Score));
        }
    }

    /// <summary>
    /// Adds points to the player's score on this board. Make the combo negative if you want to take points away.
    /// </summary>
    /// <param name="numberOfPieces"></param>
    /// <param name="combo"></param>
    private void AwardPoints(int numberOfPieces, int combo)
    {
        numberOfPieces -= 2;
        int pointsToAward = (int) (Mathf.Pow(BASE_SCORE, numberOfPieces) * 100 * combo);
        OnPointsAwarded(pointsToAward);
        Score += pointsToAward;
        if (OnScoreChange != null)
        {
            OnScoreChange();
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
                
                if (pb.Hardened)
                {
                    _hardenedPieces -= 1;
                }
                
                piecesBeingDestroyed.Add(pb);
                StartCoroutine(pb.Break(Random.Range(0.0f, 0.2f)));
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
    /// Generates a new board and guarantees there will be at most 2 pieces of same color touching.
    /// </summary>
    private void GenerateNewBoard(int numberOfFrozenPieces)
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

        MovesLeft = MAX_MOVES_LEFT;
        RandomlyHardenPieces(numberOfFrozenPieces, 1.0f);
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

    /// <summary>
    /// Determines the path to the empty spot on the grid and which bubbles to push where.
    /// </summary>
    /// <param name="start">The piece the player is dropping their piece on.</param>
    /// <param name="end">The piece that the player is grabbing</param>
    private Stack<GameMove> FindPath(GameObject start, GameObject end)
    {
        Stack<GameMove> path = new Stack<GameMove>();
        (int, int) startIdx = IndexOf(start);
        (int, int) endIdx = IndexOf(end);
        (int, int) currentIdx = startIdx;

        path.Push(new GameMove(end, endIdx, startIdx));

        while (currentIdx.Item2 != endIdx.Item2)
        {
            if (currentIdx.Item2 < endIdx.Item2)
            {
                path.Push(new GameMove(GetPiece(currentIdx), PushDirection.DOWN, currentIdx));
                currentIdx.Item2 += 1;
            }
            else
            {
                path.Push(new GameMove(GetPiece(currentIdx), PushDirection.UP, currentIdx));
                currentIdx.Item2 -= 1;
            }
        }
        while (currentIdx.Item1 != endIdx.Item1)
        {
            if (currentIdx.Item1 < endIdx.Item1)
            {
                path.Push(new GameMove(GetPiece(currentIdx), PushDirection.RIGHT, currentIdx));
                currentIdx.Item1 += 1;
            }
            else
            {
                path.Push(new GameMove(GetPiece(currentIdx), PushDirection.LEFT, currentIdx));
                currentIdx.Item1 -= 1;
            }
        }

        return path;
    }

    /// <summary>
    /// Randomly hardens a number of pieces.
    /// </summary>
    /// <param name="toHarden">Max number of pieces you want to harden.</param>
    /// <param name="chanceToHarden">Normalized percent chance to harden any piece.</param>
    private void RandomlyHardenPieces(int toHarden, float chanceToHarden)
    {
        for (int i = 0; i < toHarden; i++)
        {
            float roll = (float)UnityEngine.Random.Range(1, 101) / 100f;
            if (roll <= chanceToHarden)
            {
                (int, int) randomIdx = (UnityEngine.Random.Range(0, BOARD_SIZE), UnityEngine.Random.Range(0, BOARD_SIZE));
                PieceBehavior pieceToHarden = GetPiece(randomIdx).GetComponent<PieceBehavior>();
                if (pieceToHarden.Hardened)
                {
                    i -= 1;
                }
                else
                {
                    GetPiece(randomIdx).GetComponent<PieceBehavior>().Hardened = true;
                    _hardenedPieces += 1;
                }
            }
        }
    }

    /// <summary>
    /// Called at the end of every turn.
    /// </summary>
    private void EndOfTurn()
    {
        float chance = Mathf.Pow(_turns.Count, 2) / 8192;
        RandomlyHardenPieces(_turns.Count / 10, chance);
        CheckForGameEnd();
    }

    /// <summary>
    /// Check several variables to see if the game must end.
    /// </summary>
    private void CheckForGameEnd()
    {
        // If the player has:
        //      2. No moves left
        //      3. One or less soft piece left on the board
        // Then the game is over

        if (Score >= PointsToWin)
        {
            WinLevel();
        }
        else if (_hardenedPieces >= (Mathf.Pow(BOARD_SIZE, 2) - 1))
        {
            EndGame();
        }
    }

    /// <summary>
    /// Transitions the player to the next level.
    /// </summary>
    private void WinLevel()
    {
        OnWinConditionMet?.Invoke();
    }

    /// <summary>
    /// Transition the player to the game over screen.
    /// </summary>
    private void EndGame()
    {
        OnGameOver?.Invoke();
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

            yield return new WaitUntil(() =>
            {
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

            yield return new WaitUntil(() =>
            {
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
