using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField]private SpawnClientsSystem spawnSystem;
    [SerializeField]private List<GameObject> _carList;
    private bool _gameEnded;
    [SerializeField] private GameObject _pauseMenu;
    public PrometeoCarController _controller;

    [SerializeField] private Timer _timer;
    public PlayerScript _carStats;
    public int bonusCoins;

    private void Awake()
    {
        bonusCoins = 0;
        _carList[DataToKeep.selectedCar].SetActive(true);
    }

    private void Start()
    {
        MusicSystemScript.instance.PlayLevelMusic();
        _gameEnded = false;
        _carStats.onClientTaken += spawnSystem.SpawnDestinationPoint;
        _carStats.onClientTaken += spawnSystem.DespawnClient;
        _carStats.onDelivered += spawnSystem.SpawnClient;
        _carStats.onDelivered += spawnSystem.DespawnDestinationPoint;
    }
    private void Update()
    {
        if (_carStats._carHealthPoints <= 0 && !_gameEnded)
        {
            _gameEnded = true;
            EndGame();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _pauseMenu.SetActive(!_pauseMenu.activeSelf);
        }
    }

    public void EndGame()
    {
        Time.timeScale = 0;
        _controller.tireScreechSound.Stop();
        UIDataScript.instance.ActivateLoseMenu();
        Progress.instance.SaveData();
        Yandex.instance.ShowAds();
    }
}
