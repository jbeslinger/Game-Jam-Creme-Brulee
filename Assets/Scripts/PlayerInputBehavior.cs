using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputBehavior : MonoBehaviour
{
    #region Fields
    public GameObject playerBoard;
    #endregion

    #region Members
    private GameBoardBehavior _gb;
    
    private GameObject _selectedObj, _objToReplace;
    private Vector2 _lastPosition;
    #endregion

    #region Unity Methods
    private void Start()
    {
        if (playerBoard == null)
        {
            throw new NullReferenceException("Please assign a gameboard to this script.");
        }
        else if (playerBoard.GetComponent<GameBoardBehavior>() == null)
        {
            throw new MissingComponentException("This game object does not have the GameBoard behavior");
        }
        else
        {
            _gb = playerBoard.GetComponent<GameBoardBehavior>();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // First click
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject.GetComponent<PieceBehavior>() != null)
            {
                _selectedObj = hit.collider.gameObject;
                _selectedObj.GetComponent<PieceBehavior>().State = PieceBehavior.PieceState.GRABBED;
                _lastPosition = _selectedObj.transform.position;
            }
        }
        else if (Input.GetMouseButton(0)) // Dragging
        {
            if (_selectedObj != null)
            {
                _selectedObj.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }
        else if (Input.GetMouseButtonUp(0)) // Letting go
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject.GetComponent<PieceBehavior>() != null)
            {
                _objToReplace = hit.collider.gameObject;
                _gb.SwapPieces(_selectedObj, _objToReplace);
            }
            else
            {
                _selectedObj.GetComponent<PieceBehavior>().MoveTo(_lastPosition);
            }
            _selectedObj = null;
            _objToReplace = null;
        }

    }
    #endregion
}
