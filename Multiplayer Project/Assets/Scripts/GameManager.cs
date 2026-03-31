using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] public TeamManager teamManager;
    [SerializeField] public ServerData serverData;
    [SerializeField] public UIItems uiItems;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void ChangeReadyPlayerCount(bool isReady)
    {
        serverData.ChangeReadyPlayerCountRpc(isReady);
    }
}
