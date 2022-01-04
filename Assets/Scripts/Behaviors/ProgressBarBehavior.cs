using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ProgressBarBehavior : MonoBehaviour
{
    public static GameObject _instance;

    #region Members
    SpriteRenderer _spriteRenderer;
    #endregion

    #region Unity Methods
    private void OnEnable()
    {
        if (_instance == null)
        {
            _instance = this.gameObject;
        }
        else
        {
            DestroyImmediate(this.gameObject);
        }

        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    #endregion

    #region Methods

    #endregion
}
