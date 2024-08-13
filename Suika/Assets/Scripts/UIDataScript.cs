using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIDataScript : MonoBehaviour
{
    [SerializeField] private Text _clientsText, _coinsText, _health;
    [SerializeField] private GameObject _looseMenu;
    public static UIDataScript instance;
    public void Awake()
    {
        if (instance == null)
            instance = this;
    }
    void Start()
    {
        UpdateUIData();
    }

    public void UpdateUIData()
    {
        _clientsText.text = $"{Progress.instance.savedGameData.clients}";
        _coinsText.text = $"{Progress.instance.savedGameData.coins}";
    }

    public void UpdateHealth(int currentHealth)
    {
        _health.text = $"{currentHealth}%";
    }

    public void Restart()
    {
        SceneManager.LoadScene(1);
    }
    public void ToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
    public void ActivateLoseMenu()
    {
        _looseMenu.SetActive(true);
    }
}
