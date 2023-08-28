using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoleSystem : NetworkBehaviour
{
    private NetworkObject player;
    [SerializeField] private NetworkProperties networkProperties;
    private ulong impostorId;
    private ulong grimoireId;
    [SerializeField] private Transform hubPosition;
    [SerializeField] private GameStateSystem gameStateSystem;
    [SerializeField] private RoomSpawner roomSpawner;
    public override void OnNetworkSpawn()
    {
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerMethods>().statePlayerAction += InnocentStateActions;
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerMethods>().playerControls += InnocentControls;
    }
    [ServerRpc]
    public void AllocateRolesServerRpc()
    {
        impostorId = (ulong)UnityEngine.Random.Range(0, networkProperties.playerCount.Value+1);
        grimoireId = (ulong)UnityEngine.Random.Range(0, networkProperties.playerCount.Value+1);
        while (grimoireId == impostorId)
            grimoireId = (ulong)UnityEngine.Random.Range(0, networkProperties.playerCount.Value+1);
        SetImpostorClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { impostorId } } });
        //SetGrimoireClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { grimoireId } } });
    }
    [ClientRpc]
    private void SetImpostorClientRpc(ClientRpcParams clientId)
    {
        player = NetworkManager.LocalClient.PlayerObject;
        player.GetComponent<PlayerMethods>().statePlayerAction -= InnocentStateActions;
        player.GetComponent<PlayerMethods>().statePlayerAction += ImpostorStateActions;
        player.GetComponent<PlayerMethods>().playerControls -= InnocentControls;
        player.GetComponent<PlayerMethods>().playerControls += ImpostorControls;
        player.tag = "impostor";
    }
    [ClientRpc]
    private void SetGrimoireClientRpc(ClientRpcParams clientId)
    {
        player = NetworkManager.LocalClient.PlayerObject;
        player.GetComponent<PlayerMethods>().statePlayerAction -= InnocentStateActions;
        player.GetComponent<PlayerMethods>().statePlayerAction += GrimoireStateActions;
        player.GetComponent<PlayerMethods>().playerControls -= InnocentControls;
        player.GetComponent<PlayerMethods>().playerControls += GrimoireControls;
        player.tag = "grimoire";
    }

    #region Change state Actions
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
                NetworkManager.LocalClient.PlayerObject.transform.position = new Vector3(0, 1, 0);
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
                NetworkManager.LocalClient.PlayerObject.transform.position = hubPosition.position;
                break;
            case "innocent":
                NetworkManager.LocalClient.PlayerObject.transform.position = hubPosition.position;
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
                NetworkManager.LocalClient.PlayerObject.transform.GetComponent<PlayerMethods>().swapsRemained = 2;
                NetworkManager.LocalClient.PlayerObject.transform.position = new Vector3(0, 1, 0);
                break;
            case "innocent":
                NetworkManager.LocalClient.PlayerObject.transform.position = hubPosition.position;
                break;
        }
    }
    #endregion

    #region Player controls
    private void GrimoireControls()
    {
        if (Input.GetButtonDown("Action") && gameStateSystem.gameState.Value == NetworkManager.LocalClient.PlayerObject.tag)
        {
            if (Physics.Raycast(NetworkManager.LocalClient.PlayerObject.transform.GetChild(0).position, NetworkManager.LocalClient.PlayerObject.transform.GetChild(0).forward, out RaycastHit raycastHit, 3f))
            {
                if (raycastHit.transform.TryGetComponent(out PaintsMethod paintMethod))
                {
                    paintMethod.MovePinServerRpc(raycastHit.point);
                }
            }
        }
        ActiveRolesControls();
    }

    private void ImpostorControls()
    {
        if (Input.GetButtonDown("Action"))
        {
            if (gameStateSystem.gameState.Value == NetworkManager.LocalClient.PlayerObject.tag)
            {
                Collider[] hitColliders = Physics.OverlapSphere(NetworkManager.LocalClient.PlayerObject.transform.position, 2f);
                Collider room = Array.Find(hitColliders, room => room.gameObject.GetComponent<RoomMethods>() != null);
                if (room != null)
                {
                    room.gameObject.GetComponent<RoomMethods>().SelectRoomServerRpc();
                }
            }
            else if (gameStateSystem.gameState.Value == "innocent")
            {
                Collider[] hitColliders = Physics.OverlapSphere(NetworkManager.LocalClient.PlayerObject.transform.position, 2f);
                Collider room = Array.Find(hitColliders, room => room.gameObject.GetComponent<RoomMethods>() != null);
                if (room != null)
                {
                    room.gameObject.GetComponent<RoomMethods>().VoteRoomServerRpc();
                    NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerMethods>().voted = true;
                }
            }
        }
        if (Input.GetButtonDown("SecondaryAction") && gameStateSystem.gameState.Value == NetworkManager.LocalClient.PlayerObject.tag)
        {
            Collider[] hitColliders = Physics.OverlapSphere(NetworkManager.LocalClient.PlayerObject.transform.position, 2f);
            Collider room = Array.Find(hitColliders, room => room.gameObject.GetComponent<RoomMethods>() != null);
            if (room != null && NetworkManager.LocalClient.PlayerObject.transform.GetComponent<PlayerMethods>().swapsRemained > 0)
            {
                NetworkManager.LocalClient.PlayerObject.transform.GetComponent<PlayerMethods>().swapsRemained--;
                Transform child = room.transform.GetChild(0);               
                roomSpawner.ChangeRoomServerRpc(child.name, room.gameObject.name);
            }
        }
        ActiveRolesControls();
    }

    private void InnocentControls()
    {
        if (Input.GetButtonDown("Action") && gameStateSystem.gameState.Value == NetworkManager.LocalClient.PlayerObject.tag && !NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerMethods>().voted)
        {
            Collider[] hitColliders = Physics.OverlapSphere(NetworkManager.LocalClient.PlayerObject.transform.position, 2f);
            Collider room = Array.Find(hitColliders, room => room.gameObject.GetComponent<RoomMethods>() != null);
            if (room != null)
            {
                room.gameObject.GetComponent<RoomMethods>().VoteRoomServerRpc();
                NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerMethods>().voted = true;
            }
        }
    }

    private void ActiveRolesControls()
    {
        if (Input.GetButtonDown("Ready") && gameStateSystem.gameState.Value == NetworkManager.LocalClient.PlayerObject.tag)
        {
            networkProperties.stageReady.Value = true;
        }
    }
    #endregion
}