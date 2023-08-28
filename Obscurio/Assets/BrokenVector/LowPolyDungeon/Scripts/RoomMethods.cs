using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RoomMethods : NetworkBehaviour
{
    public NetworkVariable<int> voteCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> selected = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] private GameObject voteCounter;
    [SerializeField] private NetworkManagerUI uiManager;

    [ServerRpc(RequireOwnership = false)]
    public void SelectRoomServerRpc()
    {
        if (selected.Value)
        {
            selected.Value = false;
            transform.Find("Selector").transform.position = GameObject.Find("Selectors").transform.position;
            transform.Find("Selector").transform.SetParent(GameObject.Find("Selectors").transform);
        }
        else if(!selected.Value && GameObject.Find("Selectors").transform.childCount > 0 && transform.childCount <= 0)
        {
            selected.Value = true;
            GameObject.Find("Selectors").transform.GetChild(0).transform.position = transform.position;
            GameObject.Find("Selectors").transform.GetChild(0).transform.SetParent(transform);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void VoteRoomServerRpc()
    {
        voteCount.Value++;
        uiManager.UpdateVoteCountClientRpc(voteCounter.name, voteCount.Value);
    }
}
