using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SubtitlesScript : MonoBehaviour
{
    public void OnSubtitlesEnd()
    {
        SceneManager.LoadScene(0);
    }
}
