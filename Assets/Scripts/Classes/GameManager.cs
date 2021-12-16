using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameManager
{
    #region Methods
    /// <summary>
    /// Returns a Hashtable full of information about a new level based on the level parameter.
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public static Hashtable NewLevel(int level, int previousScore)
    {
        Hashtable newGameArgs = new Hashtable();
        newGameArgs.Add("level", level);

        float x = (Mathf.Pow(level, 1.5f) * 10000);
        x += previousScore;
        int winScore = (int) (x >= 500 ? x + 1000 : x - x % 1000);
        winScore -= winScore % 1000;

        newGameArgs.Add("win_score", winScore);
        newGameArgs.Add("hard_pieces", (level-1)*2);
        newGameArgs.Add("score_last_game", previousScore);

        return newGameArgs;
    }
    #endregion
}
