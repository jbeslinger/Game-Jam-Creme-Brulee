using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SaveButtonBehavior : MonoBehaviour
{
    #region Fields
    public Animator canvasAnimator;
    #endregion

    #region Members
    private Button _button;
    #endregion

    #region Unity Methods
    private void OnEnable()
    {
        _button = GetComponent<Button>();
        if (canvasAnimator == null)
        {
            throw new UnassignedReferenceException(string.Format("Please assign the {0} field in the inspector.", nameof(canvasAnimator)));
        }
    }
    #endregion

    #region Methods
    public void SaveAndCloseOptionsMenu()
    {
        // SAVE THE OPTIONS

        canvasAnimator.SetTrigger("Shift Menu");
    }

    public void EnableButton()
    {
        _button.interactable = true;
    }

    public void DisableButton()
    {
        _button.interactable = false;
    }
    #endregion
}
