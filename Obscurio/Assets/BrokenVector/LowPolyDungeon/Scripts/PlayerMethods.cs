using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerMethods : NetworkBehaviour
{
    private GameStateSystem gameStateSystem;
    private NetworkManagerUI networkManagerUI;
    public event Action<string> statePlayerAction;
    public event Action playerControls;
    public int swapsRemained = 0;
    public bool voted;
    private void Awake()
    {
        gameStateSystem = GameObject.Find("GameStateSystem").GetComponent<GameStateSystem>();
        networkManagerUI = GameObject.Find("NetworkManagerUI").GetComponent<NetworkManagerUI>();
    }
    private void FixedUpdate()
    {
        playerControls?.Invoke();
        GeneralControls();
    }
    public void ChangeStatePlayerAction(string state)
    {
        statePlayerAction?.Invoke(state);
    }

    private void GeneralControls()
    {
        if (gameObject == NetworkManager.LocalClient.PlayerObject.gameObject)
        {
            if (Input.GetButtonDown("Ready"))
            {
                if (gameStateSystem.gameState.Value == "readyCheck")
                {
                    networkManagerUI.ReadyCounterServerRpc();
                    Debug.Log(NetworkManager.LocalClient.ClientId);
                }
            }
        }
    }
}
