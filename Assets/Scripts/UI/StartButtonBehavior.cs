using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButtonBehavior : MonoBehaviour
{
    /// <summary>
    /// Take the player to the game scene.
    /// </summary>
    public void onClick()
    {
        SceneManager.LoadScene(SceneIndexes.GAME_SCREEN);
    }
}
