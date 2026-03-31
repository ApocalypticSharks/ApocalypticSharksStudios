using HelloWorld;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class TeamManager : NetworkBehaviour
{
    public TeamManager instance;
    [SerializeField] private int CriminalsCapacity; 
    public NetworkClient Guard = new NetworkClient();
    public List<NetworkClient> Criminals = new List<NetworkClient>();
    //public static List<NetworkClient> Villagers = new List<NetworkClient>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void DefineTeams()
    {
        IReadOnlyList<NetworkClient> clients = HelloWorldManager.m_NetworkManager.ConnectedClientsList;

        Guard = clients[Random.Range(0, clients.Count - 1)];

        for (int i = 0; i < CriminalsCapacity; i++)
        { 
            NetworkClient potentialCriminal = clients[Random.Range(0, clients.Count - 1)];
            if (Criminals.Any(criminal => criminal == potentialCriminal) || Guard == potentialCriminal)
                i--;
            else
                Criminals.Add(potentialCriminal);
        }
    }
}
