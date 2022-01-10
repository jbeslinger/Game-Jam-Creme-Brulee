using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenuBehavior : MonoBehaviour
{
    #region Fields
    public Animator canvasAnimator;
    #endregion

    #region Unity Methods
    private void OnEnable()
    {
        if (canvasAnimator == null)
        {
            throw new UnassignedReferenceException(string.Format("Please assign the {0} field in the inspector.", nameof(canvasAnimator)));
        }
    }
    #endregion

    #region Methods
    public void OpenOptionsMenu()
    {
        canvasAnimator.SetTrigger("Shift Menu");
    }
    #endregion
}
