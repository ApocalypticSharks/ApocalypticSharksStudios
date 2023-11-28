using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    private GameObject instance;
    public PlayerUIController playerUIController;
    public PlayerUI(int realm)
    {
        playerUIController = new PlayerUIController();
        instance = Instantiate(Resources.Load($"{realm}/playerUI")) as GameObject;
        var playerUIView = instance.GetComponent<PlayerUIView>();
        playerUIView.controller = playerUIController;
        playerUIController.view = playerUIView;
    }
}
