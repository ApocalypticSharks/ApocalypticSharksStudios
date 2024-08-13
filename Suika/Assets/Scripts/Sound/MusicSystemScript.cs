using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSystemScript : MonoBehaviour
{
    public static MusicSystemScript instance;
    [SerializeField] private AudioSource musicObject;
    [SerializeField] public List<AudioClip> levelMusic;
    private Action playNextClip;
    private AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void PlayLevelMusic()
    {
        playNextClip = null;
        audioSource = Instantiate(musicObject, transform.position, Quaternion.identity);
        int randomValue = UnityEngine.Random.Range(0, levelMusic.Count);
        audioSource.clip = levelMusic[randomValue];
        audioSource.volume = 1f;
        audioSource.Play();
        float clipLength = audioSource.clip.length;
        playNextClip += PlayLevelMusic;
        StartCoroutine(ScheduleNextSound(clipLength));
    }

    IEnumerator ScheduleNextSound(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
        StopAudioClip();
        PlayNextClip();
    }

    public void StopAudioClip()
    {
        Destroy(audioSource.gameObject);
    }

    public void PlayNextClip()
    {
        playNextClip?.Invoke();
    }
}
