using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkProperties : NetworkBehaviour
{
    public NetworkVariable<int> health = new NetworkVariable<int>(20, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> readyCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> playerCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> timeUp = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> stageReady = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        PlayerCounterServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerCounterServerRpc()
    {
        playerCount.Value++;
    }

    [ServerRpc]
    private void TimeUpServerRpc()
    {
        timeUp.Value = true;
    }

    public void TimerUp()
    {
        if (IsHost)
            TimeUpServerRpc();
    }
}
