using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ResolutionSwapBehavior : MonoBehaviour
{
    public static GameObject instance;

    #region Constants
    private const int RENDER_WIDTH = 270, RENDER_HEIGHT = 480;
    private const float TARGET_ASPECT = (float)RENDER_WIDTH / (float)RENDER_HEIGHT;
    private const int REF_PPU = 24;
    #endregion

    #region Members
    private int _scale;
    private bool _fullscreen;

    private bool _resolutionChangeRequested = false;
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

        LoadPrefs();
        UpdateResolution();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _scale = 1;
            _fullscreen = false;
            _resolutionChangeRequested = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _scale = 2;
            _fullscreen = false;
            _resolutionChangeRequested = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _scale = 3;
            _fullscreen = false;
            _resolutionChangeRequested = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _scale = 4;
            _fullscreen = false;
            _resolutionChangeRequested = true;
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            _fullscreen = !_fullscreen;
            _resolutionChangeRequested = true;
        }

        if (_resolutionChangeRequested)
        {
            UpdateResolution();
            SavePrefs();
            _resolutionChangeRequested = false;
        }
    }
    #endregion

    #region Methods
    private void UpdateResolution()
    {
        if (_fullscreen)
        {
            Screen.SetResolution((int)(Screen.currentResolution.height * TARGET_ASPECT), Screen.currentResolution.height, true);
        }
        else
        {
            Screen.SetResolution(RENDER_WIDTH * _scale, RENDER_HEIGHT * _scale, false);
        }
        
    }

    private void SavePrefs()
    {
        PlayerPrefs.SetInt("scale", _scale);
        PlayerPrefs.SetInt("fullscreen", _fullscreen == true ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadPrefs()
    {
        _scale = PlayerPrefs.GetInt("scale");
        _fullscreen = PlayerPrefs.GetInt("fullscreen") == 1 ? true : false;
    }
    #endregion
}
