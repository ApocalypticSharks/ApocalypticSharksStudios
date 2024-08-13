using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDestroyMusicScript : MonoBehaviour
{
    public Action playNextMusic;

    private void OnDestroy()
    {
        playNextMusic?.Invoke();
    }
}
