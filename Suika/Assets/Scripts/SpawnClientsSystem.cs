using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SpawnClientsSystem : MonoBehaviour
{
    [SerializeField]private List<Transform> _spawnPoints;
    [SerializeField] private List<Transform> _clients;
    private Transform _activeClient;
    private Transform _clientSpawnPoint;
    [SerializeField]private Transform _destinationPoint;
    [SerializeField]private Timer _timer;
    private float _timerMultipier = 0.8f;
    private float _minimalTimerMultipier = 0.15f;

    private void Start()
    {
        SpawnClient();
    }
    public void SpawnClient()
    {
        _activeClient = null;
        int randomSpawnIndex = Random.Range(0, _spawnPoints.Count);
        int randomClientIndex = Random.Range(0, _clients.Count);
        _activeClient = _clients[randomClientIndex];
        Transform spawnPoint = _spawnPoints[randomSpawnIndex];
        _clientSpawnPoint = spawnPoint;
        _activeClient.position = spawnPoint.position;
        _activeClient.gameObject.SetActive(true);
        _timer.seconds = 5 + Mathf.RoundToInt(Vector3.Distance(_destinationPoint.position, _clientSpawnPoint.position) * _timerMultipier);
        _timer.StopTimer();
        _timer.StartTimer();
    }
    public void DespawnClient()
    {
        _activeClient.gameObject.SetActive(false);
    }
    public void SpawnDestinationPoint()
    {
        Transform destinationPosition;
        do
        {
            int randomSpawnIndex = Random.Range(0, _spawnPoints.Count);
            destinationPosition = _spawnPoints[randomSpawnIndex];
        } while (destinationPosition == _clientSpawnPoint);
        _destinationPoint.position = destinationPosition.position;
        _destinationPoint.gameObject.SetActive(true);
        _timer.seconds = 6 + Mathf.RoundToInt(Vector3.Distance(_destinationPoint.position, _clientSpawnPoint.position) * _timerMultipier);
        _timer.StopTimer();
        _timer.StartTimer();
        _timerMultipier = _timerMultipier > _minimalTimerMultipier ? _timerMultipier -= 0.05f : _minimalTimerMultipier;
    }
    public void DespawnDestinationPoint()
    {
        _destinationPoint.gameObject.SetActive(false);
    }
}
