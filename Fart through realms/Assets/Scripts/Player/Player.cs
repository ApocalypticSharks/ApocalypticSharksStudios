using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player:MonoBehaviour
{
    public  GameObject instance;
    public PlayerController playerController;
    public Player(int realm, Transform spawnPoint)
    {
        instance = Instantiate(Resources.Load($"{realm}/player"), spawnPoint.position, Quaternion.identity) as GameObject;
        playerController = new PlayerController(instance.transform);
        var playerView = instance.transform.Find("body").GetComponent<PlayerView>();
        playerView.controller = playerController;
        playerController.view = playerView;
    }
}
