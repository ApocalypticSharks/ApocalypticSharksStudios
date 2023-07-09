using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameStateSystem : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> gameState = new NetworkVariable<FixedString64Bytes>("readyCheck", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField]private RoomSpawner roomSpawner;
    public Timer timer;

    [ServerRpc]
    public void ChangeGameStateServerRpc()
    {
        switch (gameState.Value.ToString())
        {
            case "readyCheck":
                gameState.Value = "grimoire";
                CallOnChangeStatePlayerActionClientRpc(gameState.Value);
                roomSpawner.SpawnFirstRoomServerRpc();
                timer.ActivateTimerClientRpc(0, 1, 0);
                break;
            case "grimoire":
                gameState.Value = "impostor";
                CallOnChangeStatePlayerActionClientRpc(gameState.Value);
                timer.ActivateTimerClientRpc(0, 1, 0);
                break;
        }
    }
    [ClientRpc]
    public void CallOnChangeStatePlayerActionClientRpc(FixedString64Bytes state)
    {
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerMethods>().ChangeStatePlayerAction(state.ToString());
    }
}
