using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    public static OptionsManager instance;

    #region Properties
    public static int WindowedScale
    {
        get => windowedScale;
        set
        {
            if (windowedScale == value)
            {
                return;
            }
            windowedScale = value;
            instance.OnWindowedScaleChange?.Invoke(value);
        }
    }
    public static bool Fullscreen
    {
        get => fullscreen;
        set
        {
            if (fullscreen == value)
            {
                return;
            }
            fullscreen = value;
            instance.OnFullscreenChange?.Invoke(value);
        }
    }
    public static float MusicVol
    {
        get => musicVol;
        set
        {
            if (musicVol == value)
            {
                return;
            }
            musicVol = value;
            instance.OnMusicVolChange?.Invoke(value);
        }
    }
    public static float LevelWinVol
    {
        get => levelWinVol;
        set
        {
            if (levelWinVol == value)
            {
                return;
            }
            levelWinVol = value;
            instance.OnLevelWinVolChange?.Invoke(value);
        }
    }
    public static float BubblePopVol
    {
        get => bubblePopVol;
        set
        {
            if (bubblePopVol == value)
            {
                return;
            }
            bubblePopVol = value;
            instance.OnBubblePopVolChange?.Invoke(value);
        }
    }
    public static int Theme
    {
        get => theme;
        set
        {
            if (theme == value)
            {
                return;
            }
            theme = value;
            instance.OnThemeChange?.Invoke(value);
        }
    }
    #endregion

    #region Fields
    private static int windowedScale = 1;
    private static bool fullscreen = false;
    private static float musicVol = 1f;
    private static float levelWinVol = 1f;
    private static float bubblePopVol = 1f;
    private static int theme = 0;
    #endregion

    #region Events
    public delegate void OnWindowedScaleChangeDelegate(int windowedScale);
    public OnWindowedScaleChangeDelegate OnWindowedScaleChange;

    public delegate void OnFullscreenChangeDelegate(bool fullscreen);
    public OnFullscreenChangeDelegate OnFullscreenChange;

    public delegate void OnMusicVolChangeDelegate(float musicVol);
    public OnMusicVolChangeDelegate OnMusicVolChange;

    public delegate void OnLevelWinVolChangeDelegate(float levelWinVol);
    public OnLevelWinVolChangeDelegate OnLevelWinVolChange;

    public delegate void OnBubblePopVolChangeDelegate(float bubblePopVol);
    public OnBubblePopVolChangeDelegate OnBubblePopVolChange;

    public delegate void OnThemeChangeDelegate(int theme);
    public OnThemeChangeDelegate OnThemeChange;
    #endregion

    #region Unity Methods
    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(this.gameObject);
        }

        LoadOptions();
    }
    #endregion

    #region Methods
    public static void SaveOptions()
    {
        PlayerPrefs.SetInt(nameof(WindowedScale), WindowedScale);
        PlayerPrefs.SetInt(nameof(Fullscreen), Fullscreen == true ? 1 : 0);

        PlayerPrefs.SetFloat(nameof(MusicVol), MusicVol);
        PlayerPrefs.SetFloat(nameof(LevelWinVol), LevelWinVol);
        PlayerPrefs.SetFloat(nameof(BubblePopVol), BubblePopVol);

        PlayerPrefs.SetInt(nameof(Theme), Theme);

        PlayerPrefs.Save();
    }

    public static void LoadOptions()
    {
        // Check if the player has played the game before
        // If they have, save the default values of each option to PlayerPrefs
        // If not, load the PlayerPrefs
        if (PlayerPrefs.GetInt("firstTimeSetupFlag") == 0)
        {
            PlayerPrefs.SetInt("firstTimeSetupFlag", 1);
            SaveOptions();
        }
        else
        {
            WindowedScale = PlayerPrefs.GetInt(nameof(WindowedScale));
            Fullscreen = PlayerPrefs.GetInt(nameof(Fullscreen)) == 1 ? true : false;

            MusicVol = PlayerPrefs.GetFloat(nameof(MusicVol));
            LevelWinVol = PlayerPrefs.GetFloat(nameof(LevelWinVol));
            BubblePopVol = PlayerPrefs.GetFloat(nameof(BubblePopVol));
        
            Theme = PlayerPrefs.GetInt(nameof(Theme));
        }
    }
    #endregion
}