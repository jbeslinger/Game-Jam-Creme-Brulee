using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartButtonBehavior : MonoBehaviour
{
    public void GoToTitle()
    {
        SceneManager.LoadScene("TitleScene", null);
    }
}
