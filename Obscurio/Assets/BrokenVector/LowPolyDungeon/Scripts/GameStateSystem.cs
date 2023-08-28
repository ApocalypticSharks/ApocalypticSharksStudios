using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameStateSystem : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> gameState = new NetworkVariable<FixedString64Bytes>("readyCheck", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField]private RoomSpawner roomSpawner;
    public NetworkManagerUI uiManager;

    [ServerRpc]
    public void ChangeGameStateServerRpc()
    {
        switch (gameState.Value.ToString())
        {
            case "readyCheck":
                gameState.Value = "grimoire";
                uiManager.SetGameStateTextClientRpc(gameState.Value);
                CallOnChangeStatePlayerActionClientRpc(gameState.Value);
                roomSpawner.SpawnFirstRoomServerRpc();
                uiManager.ActivateTimerClientRpc(0, 0, 10);
                break;
            case "grimoire":
                gameState.Value = "impostor";
                uiManager.SetGameStateTextClientRpc(gameState.Value);
                CallOnChangeStatePlayerActionClientRpc(gameState.Value);
                roomSpawner.SpawnAllRoomsServerRpc();
                uiManager.ActivateTimerClientRpc(0, 0, 10);
                break;
            case "impostor":
                gameState.Value = "innocent";
                uiManager.SetGameStateTextClientRpc(gameState.Value);
                CallOnChangeStatePlayerActionClientRpc(gameState.Value);
                roomSpawner.DespawnRegularRoomsServerRpc();
                roomSpawner.SpawnAllRoomsServerRpc();
                uiManager.ActivateTimerClientRpc(0, 0, 10);
                break;
        }
    }
    [ClientRpc]
    public void CallOnChangeStatePlayerActionClientRpc(FixedString64Bytes state)
    {
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerMethods>().ChangeStatePlayerAction(state.ToString());
    }
}
