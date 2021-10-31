using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputBehavior : MonoBehaviour
{
    #region Members
    private Transform _selectedObj;
    #endregion

    #region Unity Methods
    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // First click
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null)
            {
                _selectedObj = hit.collider.transform;
            }
        }
        else if (Input.GetMouseButton(0)) // Dragging
        {
            if (_selectedObj != null)
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                _selectedObj.transform.position = mousePos;
            }
        }
        else if (Input.GetMouseButtonUp(0)) // Letting go
        {
            if (_selectedObj != null)
            {
                _selectedObj = null;
            }
        }

    }
    #endregion
}
