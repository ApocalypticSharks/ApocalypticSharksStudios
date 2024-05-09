using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fullscreenToggle : MonoBehaviour
{
    public void ToggleFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}
