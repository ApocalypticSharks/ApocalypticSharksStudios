using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerData : NetworkBehaviour
{
    public NetworkVariable<int> readyPlayerCount;

    [Rpc(SendTo.Server)]
    public void ChangeReadyPlayerCountRpc(bool isReady)
    {
        if (isReady)
        {
            readyPlayerCount.Value++;
        }
        else
        {
            readyPlayerCount.Value--;
        }
        Debug.Log($"ready player count {readyPlayerCount.Value}");
    }
}
