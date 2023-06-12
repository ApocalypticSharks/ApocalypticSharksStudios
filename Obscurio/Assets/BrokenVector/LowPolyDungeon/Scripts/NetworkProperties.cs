using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkProperties : NetworkBehaviour
{
    public NetworkVariable<int> readyCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> playerCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        PlayerCounterServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerCounterServerRpc()
    {
        playerCount.Value++;
    }
}
