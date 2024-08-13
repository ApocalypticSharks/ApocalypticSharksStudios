using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LooseMenu : MonoBehaviour
{
    [SerializeField] private GameObject _adsButton;
    public void WatchAds()
    {
        _adsButton.SetActive(false);
        Yandex.instance.ShowRewardedAds(100);
    }
    public void Replay()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }
    public void Exit()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
