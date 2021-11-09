using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PieceBehavior : MonoBehaviour
{
    #region Enums
    public enum PieceType { RED, ORA, YEL, GRE, BLU, PUR, WHI };
    public enum PieceState { SITTING, GRABBED, MOVING, POPPED, PREVIEW };
    #endregion

    #region Consts
    public float MOVE_SPEED = 5.0f; // In units per second
    #endregion

    #region Properties
    public PieceState State { get => _state; set => ChangeState(value); }
    public PieceType Type
    {
        get => _pieceType;
        set
        {
            _pieceType = value;
            gameObject.name = ToString();
            switch (value)
            {
                case PieceType.WHI:
                    _sprite.color = Color.white;
                    break;
                case PieceType.RED:
                    _sprite.color = Color.red;
                    break;
                case PieceType.ORA:
                    _sprite.color = new Color(1.0f, 0.5f, 0.0f);
                    break;
                case PieceType.YEL:
                    _sprite.color = Color.yellow;
                    break;
                case PieceType.GRE:
                    _sprite.color = Color.green;
                    break;
                case PieceType.BLU:
                    _sprite.color = Color.blue;
                    break;
                case PieceType.PUR:
                    _sprite.color = Color.blue + Color.red;
                    break;
            }
        }
    }
    #endregion

    #region Fields
    public bool hardened = false;

    #endregion

    #region Members
    [SerializeField]
    private SpriteRenderer _sprite;
    private Collider2D _collider;

    private PieceState _state = PieceState.SITTING;
    private PieceType _pieceType = PieceType.WHI;

    [SerializeField]
    private GameObject _arrowIndicator;
    private PushDirection _arrowPointTo = PushDirection.UP;

    private Vector2 _targetPos;
    private float _startTime;
    private float _journeyLength;
    #endregion

    #region Unity Methods
    private void Start()
    {
        _collider = GetComponent<Collider2D>();
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
                    float distCovered = (Time.time - _startTime) * MOVE_SPEED;
                    float fractionOfJourney = distCovered / _journeyLength;
                    transform.localPosition = Vector2.Lerp(transform.localPosition, _targetPos, fractionOfJourney);
                }
                else
                {
                    ChangeState(PieceState.SITTING);
                }
                break;
            case PieceState.POPPED:
                /* TEMPORARY */ gameObject.SetActive(false);
                break;
            case PieceState.PREVIEW:
                {
                    float distCovered = (Time.time - _startTime) * MOVE_SPEED;
                    float fractionOfJourney = distCovered / _journeyLength;
                    _sprite.transform.position = Vector2.Lerp(_sprite.transform.position, _targetPos, fractionOfJourney);
                }
                break;
        }
    }
    #endregion

    #region Methods
    public void MoveTo(Vector2 targetPos)
    {
        if (State != PieceState.MOVING)
        {
            State = PieceState.MOVING;
            _targetPos = targetPos;
            _startTime = Time.time;
            _journeyLength = Vector2.Distance(transform.localPosition, _targetPos);
        }
    }

    public void MoveTo(PushDirection targetDir, float units)
    {
        Vector2 targetPos = transform.localPosition;
        switch (targetDir)
        {
            case PushDirection.UP:
                targetPos.y += units;
                break;
            case PushDirection.RIGHT:
                targetPos.y += units;
                break;
            case PushDirection.DOWN:
                targetPos.y -= units;
                break;
            case PushDirection.LEFT:
                targetPos.y -= units;
                break;
        }
        MoveTo(targetPos);
    }

    public void PreviewMoveTo(Vector2 targetPos)
    {
        State = PieceState.PREVIEW;
        if (State == PieceState.GRABBED)
        {
            return;
        }
        _targetPos = targetPos;
        _startTime = Time.time;
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

    public override string ToString()
    {
        return "Piece [" + _pieceType.ToString() + "]";
    }

    private void ToggleArrowIndicator()
    {
        SpriteRenderer srArrow = _arrowIndicator.GetComponent<SpriteRenderer>();
        srArrow.enabled = !srArrow.enabled;
    }

    private void ChangeState(PieceState newState)
    {
        if (_state == newState)
        {
            return;
        }
        PieceState fromState = _state;
        _state = newState;
        switch (State)
        {
            case PieceState.SITTING:
                _collider.enabled = true;
                break;
            case PieceState.GRABBED:
                _collider.enabled = false;
                break;
            case PieceState.MOVING:
                _collider.enabled = false;
                break;
            case PieceState.POPPED:
                break;
            case PieceState.PREVIEW:
                // Cannot transition from GRABBED TO PREVIEW
                _state = fromState == PieceState.GRABBED ? PieceState.GRABBED : PieceState.PREVIEW;
                break;
        }
    }
    #endregion
}
