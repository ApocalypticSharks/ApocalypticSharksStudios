using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameStateSystem : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> gameState = new NetworkVariable<FixedString64Bytes>("readyCheck", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField]private RoomSpawner roomSpawner;

    [ServerRpc]
    public void ChangeGameStateServerRpc(string state)
    {
        gameState.Value = state;

        switch (gameState.Value.ToString())
        {
            case "grimoire":
                CallOnChangeStatePlayerActionClientRpc(gameState.Value);
                roomSpawner.SpawnFirstRoomServerRpc();
                break;
        }
    }
    [ClientRpc]
    public void CallOnChangeStatePlayerActionClientRpc(FixedString64Bytes state)
    {
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerMethods>().ChangeStatePlayerAction(state.ToString());
    }
}
