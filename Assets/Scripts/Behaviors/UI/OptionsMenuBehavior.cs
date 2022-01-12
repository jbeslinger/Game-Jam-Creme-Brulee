using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuBehavior : MonoBehaviour
{
    #region Fields
    [Header("UI Controls")]
    public Dropdown windowedScaleDropdown;
    public Toggle fullscreenToggle;
    public Slider musicVolSlider;
    public Slider levelWinVolSlider;
    public Slider bubblePopVolSlider;
    public Slider themeSlider;

    public Image[] bubbleThemePreviewImages;
    #endregion

    #region Methods
    public delegate void OnCloseOptionsMenuDelegate();
    public static OnCloseOptionsMenuDelegate OnCloseOptionsMenu;
    #endregion

    #region Unity Methods
    private void OnEnable()
    {
        if (windowedScaleDropdown == null || fullscreenToggle == null || musicVolSlider == null || levelWinVolSlider == null || bubblePopVolSlider == null || themeSlider == null)
        {
            throw new UnassignedReferenceException("One of your UI Controls is not assigned in the inspector and will not function properly.");
        }

        AttachOnValueChangedEvents();
        LoadValues();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Reset all of the UI controls to their default.
    /// </summary>
    public void ResetOptionsMenu()
    {
        OptionsManagerBehavior.LoadOptions();
        LoadValues();
    }
    
    private void AttachOnValueChangedEvents()
    {
        windowedScaleDropdown.onValueChanged.AddListener(delegate
        {
            switch (windowedScaleDropdown.options[windowedScaleDropdown.value].text)
            {
                case "1x":
                    OptionsManagerBehavior.WindowedScale = 1;
                    break;
                case "2x":
                    OptionsManagerBehavior.WindowedScale = 2;
                    break;
                case "3x":
                    OptionsManagerBehavior.WindowedScale = 3;
                    break;
                case "4x":
                    OptionsManagerBehavior.WindowedScale = 4;
                    break;
            }
        });

        fullscreenToggle.onValueChanged.AddListener(delegate
        {
            OptionsManagerBehavior.Fullscreen = fullscreenToggle.isOn;
        });

        musicVolSlider.onValueChanged.AddListener(delegate
        {
            OptionsManagerBehavior.MusicVol = musicVolSlider.value;
        });

        levelWinVolSlider.onValueChanged.AddListener(delegate
        {
            OptionsManagerBehavior.LevelWinVol = levelWinVolSlider.value;
        });

        bubblePopVolSlider.onValueChanged.AddListener(delegate
        {
            OptionsManagerBehavior.BubblePopVol = bubblePopVolSlider.value;
        });

        themeSlider.onValueChanged.AddListener(delegate
        {
            UpdateThemePreviewImages();
            OptionsManagerBehavior.Theme = (int)themeSlider.value;
        });
    }

    /// <summary>
    /// Loads the stored option values into the UI controls.
    /// </summary>
    private void LoadValues()
    {
        switch (OptionsManagerBehavior.WindowedScale)
        {
            case 1:
                windowedScaleDropdown.value = 0;
                break;
            case 2:
                windowedScaleDropdown.value = 1;
                break;
            case 4:
                windowedScaleDropdown.value = 2;
                break;
            case 8:
                windowedScaleDropdown.value = 3;
                break;
        }

        fullscreenToggle.isOn = OptionsManagerBehavior.Fullscreen;
        
        musicVolSlider.value = OptionsManagerBehavior.MusicVol;
        levelWinVolSlider.value = OptionsManagerBehavior.LevelWinVol;
        bubblePopVolSlider.value = OptionsManagerBehavior.BubblePopVol;

        themeSlider.value = OptionsManagerBehavior.Theme;
    }

    private void UpdateThemePreviewImages()
    {
        // TODO
    }
    #endregion
}
