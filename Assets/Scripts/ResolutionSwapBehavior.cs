using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class ResolutionSwapBehavior : MonoBehaviour
{
    private const int RENDER_WIDTH = 480, RENDER_HEIGHT = 270;
    private const int REF_PPU = 24;

    private void OnEnable()
    {
        this.GetComponent<Camera>().orthographicSize = ((float)RENDER_HEIGHT / (float)REF_PPU) * 0.5f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Screen.SetResolution(RENDER_WIDTH, RENDER_HEIGHT, false);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Screen.SetResolution(RENDER_WIDTH * 2, RENDER_HEIGHT * 2, false);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Screen.SetResolution(RENDER_WIDTH * 3, RENDER_HEIGHT * 3, false);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Screen.SetResolution(RENDER_WIDTH * 4, RENDER_HEIGHT * 4, false);
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            if (Screen.fullScreen)
                Screen.SetResolution(RENDER_WIDTH, RENDER_HEIGHT, false);
            else
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        }
    }
}
