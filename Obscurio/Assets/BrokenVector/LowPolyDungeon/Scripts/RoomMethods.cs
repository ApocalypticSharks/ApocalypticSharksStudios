using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoomMethods : NetworkBehaviour
{
    public NetworkVariable<int> voteCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

   

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
            voteCount.Value++;

        Debug.Log(OwnerClientId + ": " + voteCount.Value);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
            voteCount.Value--;

        Debug.Log(OwnerClientId + ": " + voteCount.Value);
    }
}
