using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    public PlayerScript _carStats;
    [SerializeField] private Text _HealthText, _moneyText, _deliveredText;
    [SerializeField] private GameObject _looseMenu;

    void Update()
    {
        _HealthText.text = $"{_carStats._carHealthPoints}%";
        _moneyText.text = $"${Progress.instance.savedGameData.coins}";
        _deliveredText.text = $"{Progress.instance.savedGameData.clients}";
    }

    public void ActivateLoseMenu()
    {
        _looseMenu.SetActive(true);
    }
    public void Restart()
    {
        SceneManager.LoadScene(1);
    }
    public void ToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
