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
    
    private GameObject _selectedObj, _objUnderCursor;
    private Vector2 _lastPosition;

    private PushDirection _directionToPush;
    private bool _hasRequestedPreview = false;
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
        if (_gb.IsBusy)
        {
            return;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        /* ON MOUSE CLICK */
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject.GetComponent<PieceBehavior>() != null)
            {
                if (!hit.collider.GetComponent<PieceBehavior>().Hardened)
                {
                    _selectedObj = hit.collider.gameObject;
                    _selectedObj.GetComponent<PieceBehavior>().State = PieceBehavior.PieceState.GRABBED;
                    _lastPosition = _selectedObj.transform.localPosition;
                }
            }
        }

        /* MOUSE DRAGGING */
        else if (Input.GetMouseButton(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (_selectedObj != null && hit.collider != null && hit.collider.gameObject.GetComponent<PieceBehavior>() != null)
            {
                if (_hasRequestedPreview && (hit.collider.gameObject != _objUnderCursor))
                {
                    EndPreview();
                }
                else
                {
                    if (!hit.collider.GetComponent<PieceBehavior>().Hardened)
                    {
                        _objUnderCursor = hit.collider.gameObject;
                        PreviewPlacement();
                    }
                }

            }
            else
            {
                _objUnderCursor = null;
            }
            if (_selectedObj != null)
            {
                _selectedObj.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        /* ON MOUSE RELEASE */
        else if (Input.GetMouseButtonUp(0))
        {
            EndPreview();
            if (_selectedObj != null)
            {
                if (_objUnderCursor != null)
                {
                    RequestPlacement();
                }
                else
                {
                    _selectedObj.GetComponent<PieceBehavior>().MoveTo(_gb.IndexOf(_selectedObj));
                }
            }
            _selectedObj = null;
            _objUnderCursor = null;
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Makes a move on the gameboard. Must be called after PreviewPlacement().
    /// </summary>
    /// <param name="objectToPush"></param>
    /// <param name="currentMousePos"></param>
    private void RequestPlacement()
    {
        _gb.PerformMove();
    }

    /// <summary>
    /// Calls for a preview of the piece placement on the gameboard. Must be caled before RequestPlacement().
    /// </summary>
    private void PreviewPlacement()
    {
        if (!_hasRequestedPreview)
        {
            _hasRequestedPreview = true;
            _gb.PreviewPlacement(_selectedObj, _objUnderCursor, _directionToPush);
        }
    }

    /// <summary>
    /// Calls to end the preview on the gameboard.
    /// </summary>
    private void EndPreview()
    {
        if (_hasRequestedPreview)
        {
            _hasRequestedPreview = false;
            _gb.RemovePreviewPlacement();
        }
    }
    #endregion
}
