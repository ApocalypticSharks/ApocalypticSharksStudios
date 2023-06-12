using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoleSystem : NetworkBehaviour
{
    private NetworkObject player;
    [SerializeField]private NetworkProperties networkProperties;
    private ulong impostorId;
    private ulong grimoireId;
    [SerializeField]private Transform hubPosition;
    public override void OnNetworkSpawn()
    {
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerMethods>().statePlayerAction += InnocentStateActions;
    }
    [ServerRpc]
    public void AllocateRolesServerRpc()
    {
        impostorId = (ulong)Random.Range(0,networkProperties.playerCount.Value);
        grimoireId = (ulong)Random.Range(0, networkProperties.playerCount.Value);
        while(grimoireId == impostorId)
            grimoireId = (ulong)Random.Range(0, networkProperties.playerCount.Value);
        SetImpostorClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { impostorId } } });
        SetGrimoireClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { grimoireId } } });
    }
    [ClientRpc]
    private void SetImpostorClientRpc(ClientRpcParams clientId)
    {
        player = NetworkManager.LocalClient.PlayerObject;
        player.GetComponent<PlayerMethods>().statePlayerAction -= InnocentStateActions;
        player.GetComponent<PlayerMethods>().statePlayerAction += ImpostorStateActions;
        player.tag = "impostor";
    }
    [ClientRpc]
    private void SetGrimoireClientRpc(ClientRpcParams clientId)
    {
        player = NetworkManager.LocalClient.PlayerObject;
        player.GetComponent<PlayerMethods>().statePlayerAction -= InnocentStateActions;
        player.GetComponent<PlayerMethods>().statePlayerAction += GrimoireStateActions;
        player.tag = "grimoire";
    }

    private void InnocentStateActions(string state)
    {
        switch (state)
        {
            case "grimoire":
                NetworkManager.LocalClient.PlayerObject.transform.position = hubPosition.position;
                break;
            case "impostor":
                break;
            case "innocent":
                break;
        }
    }

    private void GrimoireStateActions(string state)
    {
        switch (state)
        {
            case "grimoire":
                NetworkManager.LocalClient.PlayerObject.transform.position = new Vector3(0, 1, 0);
                break;
            case "impostor":
                break;
            case "innocent":
                break;
        }
    }

    private void ImpostorStateActions(string state)
    {
        switch (state)
        {
            case "grimoire":
                NetworkManager.LocalClient.PlayerObject.transform.position = hubPosition.position;
                break;
            case "impostor":
                break;
            case "innocent":
                break;
        }
    }
}
