using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player:MonoBehaviour
{
    private GameObject instance;
    public PlayerController playerController;
    public Player(int realm)
    {
        playerController = new PlayerController();
        instance = Instantiate(Resources.Load($"{realm}/player")) as GameObject;
        var playerView = instance.GetComponent<PlayerView>();
        playerView.controller = playerController;
        playerController.view = playerView;
    }
}
