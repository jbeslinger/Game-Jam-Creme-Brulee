using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class PieceBehavior : MonoBehaviour
{
    #region Enums
    public enum PieceType { RED, ORA, YEL, GRE, BLU, PUR, WHI };
    public enum PieceState { STILL, GRABBED, MOVING, POPPED };
    #endregion

    #region Consts
    public float MOVE_SPEED = 5.0f; // In units per second
    #endregion

    #region Properties
    public PieceState State { get => _state; set => ChangeState(value); }
    #endregion

    #region Fields
    public PieceType type = PieceType.WHI;
    public bool hardened = false;
    #endregion

    #region Members
    private SpriteRenderer _sr;
    private Collider2D _cd;

    private PieceState _state = PieceState.STILL;

    private Vector2 _targetPos;
    private float _startTime;
    private float _journeyLength;
    #endregion

    #region Unity Methods
    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _cd = GetComponent<Collider2D>();

        Color pieceColor = Color.white;
        switch (this.type)
        {
            case PieceType.RED:
                pieceColor = Color.red;
                break;
            case PieceType.ORA:
                pieceColor = new Color(1.0f, 0.5f, 0.0f);
                break;
            case PieceType.YEL:
                pieceColor = Color.yellow;
                break;
            case PieceType.GRE:
                pieceColor = Color.green;
                break;
            case PieceType.BLU:
                pieceColor = Color.blue;
                break;
            case PieceType.PUR:
                pieceColor = Color.blue + Color.red;
                break;
        }
        _sr.color = pieceColor;

        this.gameObject.name = this.ToString();
    }

    private void Update()
    {
        switch (State)
        {
            case PieceState.STILL:
                break;
            case PieceState.GRABBED:
                break;
            case PieceState.MOVING:
                if ((Vector2)transform.position != _targetPos)
                {
                    float distCovered = (Time.time - _startTime) * MOVE_SPEED;
                    float fractionOfJourney = distCovered / _journeyLength;
                    transform.position = Vector2.Lerp(transform.position, _targetPos, fractionOfJourney);
                }
                else
                {
                    ChangeState(PieceState.STILL);
                }
                break;
            case PieceState.POPPED:
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
            _journeyLength = Vector2.Distance(transform.position, _targetPos);
        }
    }

    public override string ToString()
    {
        return "Piece [" + type.ToString() + "]";
    }

    private void ChangeState(PieceState newState)
    {
        this._state = newState;
        switch (this.State)
        {
            case PieceState.STILL:
                _cd.enabled = true;
                break;
            case PieceState.GRABBED:
                _cd.enabled = false;
                break;
            case PieceState.MOVING:
                _cd.enabled = false;
                break;
            case PieceState.POPPED:
                break;
        }
    }
    #endregion
}
