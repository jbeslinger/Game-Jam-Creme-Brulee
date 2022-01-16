using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HyperlinkButtonBehavior : MonoBehaviour
{
    public void OpenHyperlink(string url)
    {
        Application.OpenURL(url);
    }
}
