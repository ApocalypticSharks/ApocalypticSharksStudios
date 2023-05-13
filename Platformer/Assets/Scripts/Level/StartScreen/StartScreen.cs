using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreen : MonoBehaviour
{
    private Player player;
    [SerializeField]private Animator animator;
    private void Awake()
    {
        player = new Player(false, new Vector3(-10, 0, 0), transform.rotation);
    }

    private void Update()
    {
        if (Input.anyKeyDown) 
        {
            player.EnableScripts();
            animator.SetTrigger("ZoomOut");
        }

    }
    private void LoadLevel()
    {
        SceneManager.LoadScene("FirstLevel");
    }
}
