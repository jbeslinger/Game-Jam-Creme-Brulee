using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PieceBehavior : MonoBehaviour
{
    #region Enums
    public enum PieceType { RED, ORA, YEL, GRE, BLU, PUR, WHI };
    #endregion

    #region Fields
    public PieceType type = PieceType.WHI;
    public bool hardened = false;
    #endregion

    #region Members
    private SpriteRenderer _sr;
    #endregion

    #region Unity Methods
    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        SetRandomColor();
        this.gameObject.name = this.ToString();
    }
    #endregion

    #region Methods
    public override string ToString()
    {
        return "Piece [" + type.ToString() + "]";
    }

    private void SetRandomColor()
    {
        this.type = (PieceType)UnityEngine.Random.Range(0, 7);
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
    }
    #endregion
}
