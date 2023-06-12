using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField] private Button serverButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button HostButton;
    [SerializeField] private TextMeshProUGUI playersReadyText, RoleText;
    [SerializeField] private NetworkProperties networkProperties;
    [SerializeField] private RoleSystem roleSystem;
    [SerializeField] private GameStateSystem gameStateSystem;

    private void FixedUpdate()
    {
        UpdateReadyCounterClientRpc();
        if (Input.GetButtonDown("Ready"))
        {
            ReadyCounterServerRpc();
        }
        if (networkProperties.readyCount.Value >= networkProperties.playerCount.Value && gameStateSystem.gameState.Value == "readyCheck" && IsHost) 
        {
            roleSystem.AllocateRolesServerRpc();
            UpdateRoleClientRpc();
            gameStateSystem.ChangeGameStateServerRpc("grimoire");
        }
    }
    private void Awake()
    {
        serverButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });

        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });

        HostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReadyCounterServerRpc()
    {
        networkProperties.readyCount.Value++;
        UpdateReadyCounterClientRpc();
    }

    [ClientRpc]
    private void UpdateReadyCounterClientRpc()
    {
        playersReadyText.SetText(networkProperties.readyCount.Value + "/" + networkProperties.playerCount.Value);
    }

    [ClientRpc]
    private void UpdateRoleClientRpc()
    {
        RoleText.SetText(NetworkManager.LocalClient.PlayerObject.tag);
    }
}
