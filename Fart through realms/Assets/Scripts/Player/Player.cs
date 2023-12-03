using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player:MonoBehaviour
{
    public  GameObject instance;
    public PlayerController playerController;
    public Player(int realm)
    {
        playerController = new PlayerController();
        instance = Instantiate(Resources.Load($"{realm}/player")) as GameObject;
        var playerView = instance.transform.Find("body").GetComponent<PlayerView>();
        playerView.controller = playerController;
        playerController.view = playerView;
    }
}
