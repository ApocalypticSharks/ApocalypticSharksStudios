using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    void Start()
    {
        MusicSystemScript.instance.PlayLevelMusic();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        { 
            pauseMenu.SetActive(!pauseMenu.activeSelf);
        }
    }
}
