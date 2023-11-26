using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Unity.Collections;

public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField] private Button serverButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button HostButton;
    [SerializeField] private TextMeshProUGUI playersReadyText, RoleText, GameState;
    [SerializeField] private NetworkProperties networkProperties;
    [SerializeField] private RoleSystem roleSystem;
    [SerializeField] private GameStateSystem gameStateSystem;
    [SerializeField] private Timer timer;
    private int totalVotes;

    private void FixedUpdate()
    {
        UpdateReadyCounterClientRpc();
        if (IsHost)
        {
            if (networkProperties.readyCount.Value >= networkProperties.playerCount.Value && gameStateSystem.gameState.Value == "readyCheck")
            {
                roleSystem.AllocateRolesServerRpc();
                UpdateRoleClientRpc();
                gameStateSystem.ChangeGameStateServerRpc();
            }
            else if ((networkProperties.timeUp.Value && new FixedString64Bytes[] { "grimoire", "impostor", "innocent" }.Contains(gameStateSystem.gameState.Value)) || networkProperties.stageReady.Value)
            {
                networkProperties.timeUp.Value = false;
                networkProperties.stageReady.Value = false;
                gameStateSystem.ChangeGameStateServerRpc();
            }
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
    public void ReadyCounterServerRpc()
    {
        networkProperties.readyCount.Value++;
        UpdateReadyCounterClientRpc();
    }

    [ClientRpc]
    public void ActivateTimerClientRpc(int hours, int minutes, int seconds)
    {
        timer.ActivateTimer(hours, minutes, seconds);
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

    [ClientRpc]
    public void SetGameStateTextClientRpc(FixedString64Bytes state)
    {
        GameState.SetText(state.ToString());
    }

    [ClientRpc]
    public void UpdateVoteCountClientRpc(FixedString64Bytes counterName, int voteCount)
    {
        TextMeshProUGUI counter = GameObject.Find(counterName.ToString()).transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        counter.SetText(voteCount.ToString());
        totalVotes++;
        if (totalVotes == networkProperties.playerCount.Value && IsHost)
            networkProperties.stageReady.Value = true;
    }
}
