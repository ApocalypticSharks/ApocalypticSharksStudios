using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSystemScript : MonoBehaviour
{
    public static SoundSystemScript instance;
    [SerializeField] private AudioSource soundFXObject;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayConstantSoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        AudioSource constantAudioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        constantAudioSource.clip = audioClip;
        constantAudioSource.volume = volume;
        constantAudioSource.Play();
    }

    public AudioSource PlayConstantSoundFXClipWithAudioSourceAccess(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        AudioSource constantAudioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        constantAudioSource.clip = audioClip;
        constantAudioSource.volume = volume;
        constantAudioSource.Play();
        return constantAudioSource;
    }
}
