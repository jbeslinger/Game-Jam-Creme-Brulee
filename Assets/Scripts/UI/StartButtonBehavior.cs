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
        Hashtable newGameArgs = new Hashtable();
        newGameArgs.Add("win_score", 25000);
        newGameArgs.Add("hard_pieces", 0);
        
        SceneManager.LoadScene("GameScene", newGameArgs);
    }
}
