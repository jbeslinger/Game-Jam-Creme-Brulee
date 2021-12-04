using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SceneManager
{
    #region Properties
    public static Hashtable SceneArgs { get => _sceneArgs; }
    #endregion

    #region Members
    private static Hashtable _sceneArgs;
    #endregion

    #region Methods
    public static void LoadScene(string sceneName, Hashtable args)
    {
        _sceneArgs = args;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
    #endregion
}
