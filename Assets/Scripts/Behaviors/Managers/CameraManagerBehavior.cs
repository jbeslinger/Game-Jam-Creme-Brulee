using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraManagerBehavior : MonoBehaviour
{
    public static GameObject instance;

    #region Constants
    private const int RENDER_WIDTH = 270, RENDER_HEIGHT = 480;
    private const float TARGET_ASPECT = (float)RENDER_WIDTH / (float)RENDER_HEIGHT;
    private const int REF_PPU = 24;
    #endregion

    #region Unity Methods
    private void OnEnable()
    {
        GetComponent<Camera>().orthographicSize = ((float)RENDER_HEIGHT / (float)REF_PPU) * 0.5f;   
    }

    private void Start()
    {
        if (instance == null)
        {
            instance = this.gameObject;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            DestroyImmediate(this.gameObject);
        }

        OptionsManagerBehavior.instance.OnFullscreenChange += ToggleFullscreen;
        OptionsManagerBehavior.instance.OnWindowedScaleChange += ChangeWindowedScale;
    }
    #endregion

    #region Methods
    private void ToggleFullscreen(bool fullscreen)
    {
        if (fullscreen)
        {
            Screen.SetResolution((int)(Screen.currentResolution.height * TARGET_ASPECT), Screen.currentResolution.height, true);
        }
        else
        {
            Screen.SetResolution(RENDER_WIDTH * OptionsManagerBehavior.WindowedScale, RENDER_HEIGHT * OptionsManagerBehavior.WindowedScale, false);
        }
        
    }

    private void ChangeWindowedScale(int windowedScale)
    {
        if (!OptionsManagerBehavior.Fullscreen)
        {
            Screen.SetResolution(RENDER_WIDTH * windowedScale, RENDER_HEIGHT * windowedScale, false);
        }
    }
    #endregion
}
