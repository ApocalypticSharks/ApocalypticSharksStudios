using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    public void SetMasterVolume(float level)
    {
        audioMixer.SetFloat("masterVolume", Mathf.Log10(level) * 20);
    }

    public void SetSoundFXVolume(float level)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(level) * 20);
    }

    public void SetMusicVolume(float level)
    {
        audioMixer.SetFloat("musicVolume", Mathf.Log10(level) * 20);
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        { 
            AudioListener.volume = 0;
        }
        else 
        { 
            AudioListener.volume = 1; 
        }
    }
}
