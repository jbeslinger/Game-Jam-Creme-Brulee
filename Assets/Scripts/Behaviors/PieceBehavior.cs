using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class PieceBehavior : MonoBehaviour
{
    #region Enums
    public enum PieceType { RED, ORA, YEL, GRE, BLU, PUR, WHI };
    public enum PieceState { SITTING, GRABBED, MOVING, POPPED, PREVIEW };
    #endregion

    #region Consts
    private const float MOVE_SPEED = 8f; // In units per second
    private const float PREVIEW_MOVE_SPEED = MOVE_SPEED / 2f; // In units per second

    public readonly Dictionary<PieceType, Color> colorDict = new Dictionary<PieceType, Color>()
    {
        { PieceType.RED, new Color32(0xFF, 0x00, 0x00, 0xFF) }, 
        { PieceType.ORA, new Color32(0xFF, 0x80, 0x00, 0xFF) }, 
        { PieceType.YEL, new Color32(0xFF, 0xFF, 0x00, 0xFF) }, 
        { PieceType.GRE, new Color32(0x00, 0xFF, 0x00, 0xFF) }, 
        { PieceType.BLU, new Color32(0x00, 0x00, 0xFF, 0xFF) }, 
        { PieceType.PUR, new Color32(0x80, 0x00, 0x80, 0xFF) }, 
        { PieceType.WHI, new Color32(0xFF, 0xFF, 0xFF, 0xFF) }  
    };
    #endregion

    #region Events
    public delegate void OnBreakDelegate();
    public OnBreakDelegate OnBreak;
    #endregion

    #region Properties
    public PieceState State { get => _state; set
        {
            if (_state == value)
            {
                return;
            }
            PieceState fromState = _state;
            _state = value;
            switch (State)
            {
                case PieceState.SITTING:
                    animating = false;
                    _coll.enabled = true;
                    break;
                case PieceState.GRABBED:
                    _coll.enabled = false;
                    break;
                case PieceState.MOVING:
                    animating = true;
                    _coll.enabled = false;
                    break;
                case PieceState.POPPED:
                    _coll.enabled = false;
                    break;
                case PieceState.PREVIEW:
                    // Cannot transition from GRABBED TO PREVIEW
                    _state = fromState == PieceState.GRABBED ? PieceState.GRABBED : PieceState.PREVIEW;
                    animating = _state == PieceState.PREVIEW ? true : false;
                    break;
            }
        }
    }
    public PieceType Type
    {
        get => _pieceType;
        set
        {
            _pieceType = value;
            gameObject.name = ToString();
            _sprite.color = colorDict[value];
        }
    }
    public bool Hardened { get => _hardened; set
        {
            _hardened = value;
            if (_hardened)
            {
                _hardFlag = true;
            }
        }
    }
    #endregion

    #region Fields
    public bool animating;
    #endregion

    #region Members
    [SerializeField]
    private SpriteRenderer _sprite;
    private Collider2D _coll;
    private Animator _anim;

    private PieceState _state = PieceState.SITTING;
    private PieceType _pieceType = PieceType.WHI;

    private bool _hardened = false;
    private bool _hardFlag = false;

    [SerializeField]
    private GameObject _arrowIndicator;
    private PushDirection _arrowPointTo = PushDirection.UP;

    private Vector2 _targetPos;
    private float _framesElapsed;
    private float _journeyLength;
    #endregion

    #region Unity Methods
    private void Start()
    {
        _coll = GetComponent<Collider2D>();
        _anim = GetComponent<Animator>();
        if (_sprite == null)
        {
            throw new MissingMemberException("Please provide a SpriteRenderer that's apart from this GameObject in the Inspector.");
        }
        if (_arrowIndicator == null)
        {
            throw new MissingMemberException("Please provide a GameObject to the Arrow Indicator field.");
        }
        else if (_arrowIndicator.GetComponent<SpriteRenderer>() == null)
        {
            throw new MissingComponentException("The GameObject you provided as the Arrow Indicator has no Sprite.");
        }
        _arrowIndicator.GetComponent<SpriteRenderer>().enabled = false;
        SoundManagerBehavior.RegisterGamePiece(this);
    }

    private void Update()
    {
        switch (State)
        {
            case PieceState.SITTING:
                break;
            case PieceState.GRABBED:
                break;
            case PieceState.MOVING:
                if ((Vector2)transform.localPosition != _targetPos)
                {
                    float distCovered = _framesElapsed * MOVE_SPEED;
                    float fractionOfJourney = distCovered / _journeyLength;
                    transform.localPosition = Vector2.Lerp(transform.localPosition, _targetPos, fractionOfJourney);
                    _framesElapsed += (1f / 60f);
                }
                else
                {
                    State = PieceState.SITTING;
                }
                break;
            case PieceState.POPPED:
                break;
            case PieceState.PREVIEW:
                {
                    float distCovered = _framesElapsed * PREVIEW_MOVE_SPEED;
                    float fractionOfJourney = distCovered / _journeyLength;
                    _sprite.transform.position = Vector2.Lerp(_sprite.transform.position, _targetPos, fractionOfJourney);
                    _framesElapsed += (1f / 60f);
                }
                break;
        }

        if (_hardFlag)
        {
            _hardFlag = false;
            _anim.SetBool("Hardened", true);
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Animate the piece to slide into a targetPosition.
    /// </summary>
    /// <param name="targetPos"></param>
    public void MoveTo(Vector2 targetPos)
    {
        if (State != PieceState.MOVING)
        {
            State = PieceState.MOVING;
            _targetPos = targetPos;
            _framesElapsed = 0;
            _journeyLength = Vector2.Distance(transform.localPosition, _targetPos);
        }
    }

    /// <summary>
    /// Overridden MoveTo that takes the index on the board you want the piece to move to and translates it to Vector2.
    /// </summary>
    /// <param name="targetIndex">An index on the board; column then row.</param>
    public void MoveTo((int, int) targetIndex)
    {
        MoveTo(new Vector2(targetIndex.Item1, -targetIndex.Item2));
    }

    public void PreviewMoveTo(Vector2 targetPos)
    {
        State = PieceState.PREVIEW;
        if (State == PieceState.GRABBED)
        {
            return;
        }
        _targetPos = targetPos;
        _framesElapsed = 0;
        _journeyLength = Vector2.Distance(_sprite.transform.position, _targetPos);
    }

    public void PreviewMoveTo(PushDirection targetDir, float units)
    {
        State = PieceState.PREVIEW;
        if (State == PieceState.GRABBED)
        {
            return;
        }
        Vector2 targetPos = _sprite.transform.position;
        switch (targetDir)
        {
            case PushDirection.UP:
                targetPos.y += units;
                break;
            case PushDirection.RIGHT:
                targetPos.x += units;
                break;
            case PushDirection.DOWN:
                targetPos.y -= units;
                break;
            case PushDirection.LEFT:
                targetPos.x -= units;
                break;
        }
        _arrowPointTo = targetDir;
        ToggleArrowIndicator();
        RotateArrowIndicator();
        PreviewMoveTo(targetPos);
    }

    public void RemovePreview()
    {
        if (State == PieceState.GRABBED)
        {
            return;
        }
        ToggleArrowIndicator();
        _arrowPointTo = PushDirection.UP;
        _sprite.transform.position = transform.position;
        State = PieceState.SITTING;
    }

    public void RotateArrowIndicator()
    {
        float degrees = 0.0f;
        switch (_arrowPointTo)
        {
            case PushDirection.UP:
                degrees = 0.0f;
                break;
            case PushDirection.LEFT:
                degrees = 90.0f;
                break;
            case PushDirection.DOWN:
                degrees = 180.0f;
                break;
            case PushDirection.RIGHT:
                degrees = 270.0f;
                break;
        }
        _arrowIndicator.transform.rotation = Quaternion.Euler(0.0f, 0.0f, degrees);
    }

    public void Break()
    {
        State = PieceState.POPPED;
        animating = true;
        _anim.SetTrigger("Popped");
        OnBreak?.Invoke();
    }

    public void Revive()
    {
        State = PieceState.SITTING;
        _anim.SetTrigger("Revived");
    }

    public void SetAnimating(int to)
    {
        if (to >= 1)
        {
            animating = true;
        }
        else
        {
            animating = false;
        }
    }

    public override string ToString()
    {
        return "Piece [" + _pieceType.ToString() + "]";
    }

    private void ToggleArrowIndicator()
    {
        SpriteRenderer srArrow = _arrowIndicator.GetComponent<SpriteRenderer>();
        srArrow.enabled = !srArrow.enabled;
    }
    #endregion
}
