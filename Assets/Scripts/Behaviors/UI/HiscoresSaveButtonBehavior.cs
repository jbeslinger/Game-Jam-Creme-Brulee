using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HiscoresSaveButtonBehavior : MonoBehaviour
{
    #region Fields
    public InputField nameField;
    #endregion

    #region Members
    private Button _button;
    #endregion

    #region Unity Methods
    private void OnEnable()
    {
        if (nameField == null)
        {
            throw new UnassignedReferenceException(string.Format("Please assign the field {0} in the Inspector before running this script.", nameof(nameField)));
        }

        _button = GetComponent<Button>();
    }
    #endregion

    #region Methods
    public void SaveHighScore()
    {
        if (nameField.text == "")
        {
            return;
        }

        int hiscore = (int)SceneManager.SceneArgs["score_last_game"];
        HiScoreManagerBehavior.SaveHiScore(nameField.text, hiscore, HiScoreManagerBehavior.IsNewHiScore(hiscore));
        _button.interactable = false;
        nameField.interactable = false;
    }

    
    #endregion
}
