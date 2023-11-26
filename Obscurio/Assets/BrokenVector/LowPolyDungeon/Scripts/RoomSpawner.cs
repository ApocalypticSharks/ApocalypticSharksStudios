using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class RoomSpawner : NetworkBehaviour
{
    [SerializeField] private List<Object> roomsList = new List<Object>();
    [SerializeField] private List<GameObject> roomSpawnPointsList = new List<GameObject>();
    List<GameObject> freeRoomLocations;
    List<Rooms> freeRooms;
    private List<Rooms> roomsPool = new List<Rooms>();
    [SerializeField] private NetworkProperties networkProperties;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < roomsList.Count; i++)
        {
            roomsPool.Add(new Rooms(roomsList[i]));
        }
    }

    [ServerRpc]
    public void SpawnFirstRoomServerRpc()
    {
        SpawnFirstRoom();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnAllRoomsServerRpc()
    {
        for (int i = 0; i < roomSpawnPointsList.Count - 1; i++)
        {
            SpawnRoom();
        }
    }

    private void SpawnRoom()
    {
        freeRoomLocations = roomSpawnPointsList.Where(roomLocation => roomLocation.transform.childCount == 0).ToList();
        if (freeRoomLocations.Count > 0)
        {
            freeRooms = roomsPool.Where(room => !room.roomEntity.activeSelf).ToList();
            ActivateRoomClientRpc(freeRooms[Random.Range(0, freeRooms.Count)].name, freeRoomLocations[Random.Range(0, freeRoomLocations.Count)].name);
        }
    }

    private void SpawnFirstRoom()
    {
        freeRoomLocations = roomSpawnPointsList.Where(roomLocation => roomLocation.transform.childCount == 0).ToList();
        if (freeRoomLocations.Count > 0)
        {
            freeRooms = roomsPool.Where(room => !room.roomEntity.activeSelf).ToList();
            Rooms firstRoom = freeRooms[Random.Range(0, freeRooms.Count)];
            firstRoom.isExit = true;
            GameObject spawnPoint = roomSpawnPointsList[Random.Range(0, freeRoomLocations.Count)];
            ActivateRoomClientRpc(firstRoom.name, spawnPoint.name);
            GameObject.Find("ExitMarker").transform.position = spawnPoint.transform.position;
            GameObject.Find("ExitMarker").transform.SetParent(spawnPoint.transform);
        }
    }

    [ClientRpc]
    private void ActivateRoomClientRpc(FixedString64Bytes roomName, FixedString64Bytes roomLocatioName)
    {
        roomsPool.Find(room => room.name == roomName).ActivateDeactivte(roomSpawnPointsList.Find(location => location.name == roomLocatioName).transform);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeRoomServerRpc(FixedString64Bytes roomName, FixedString64Bytes roomLocatioName)
    {
        ActivateRoomClientRpc(roomName, roomLocatioName);
        SpawnRoom();
    }

    [ServerRpc]
    public void DespawnRegularRoomsServerRpc()
    {
        roomSpawnPointsList.ForEach(spawnPoint =>
            {
                if (!spawnPoint.transform.Find("Selector") && !spawnPoint.transform.Find("ExitMarker"))
                {
                    ActivateRoomClientRpc(spawnPoint.transform.GetChild(0).name, spawnPoint.name);
                }
            }
        );
    }

    [ServerRpc]
    public void UnselectAllRoomsOnRoundResultsServerRpc()
    {
        roomSpawnPointsList.ForEach(spawnPoint =>
            {
                spawnPoint.GetComponent<RoomMethods>().SelectRoomServerRpc();
            }
        );
    }

    [ServerRpc]
    public void CalculateVotesServerRpc()
    {
        roomSpawnPointsList.ForEach(spawnPoint =>
        {
            if (!roomsPool.Find(room => room.name == spawnPoint.transform.GetChild(0).name).isExit)
            {
               networkProperties.health.Value -= spawnPoint.GetComponent<RoomMethods>().voteCount.Value;
            }
        }
);
    }
}
