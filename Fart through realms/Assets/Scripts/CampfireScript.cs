using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampfireScript : MonoBehaviour
{
    [SerializeField] private AudioClip fireSound;
    void Start()
    {
        SoundSystemScript.instance.PlayConstantSoundFXClip(fireSound, transform, 1f);
    }
}
