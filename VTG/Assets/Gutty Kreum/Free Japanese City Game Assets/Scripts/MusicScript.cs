using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicScript : MonoBehaviour
{
    public AudioClip[] songs;
    int currentSong = 0;

    void Update()
    {
        if (GetComponent<AudioSource>().isPlaying == false)
        {
            currentSong++;
            if (currentSong >= songs.Length)
            {
                currentSong = 0;
            }
            GetComponent<AudioSource>().clip = songs[currentSong];
            GetComponent<AudioSource>().Play();
        }
    }
}
