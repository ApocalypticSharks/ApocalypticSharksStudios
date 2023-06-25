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
        SpawnRoom();
    }

    [ServerRpc]
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
            ActivateRoomClientRpc(freeRooms[Random.Range(0, freeRooms.Count)].name, roomSpawnPointsList[Random.Range(0, freeRoomLocations.Count)].name);
        }
    }

    [ClientRpc]
    private void ActivateRoomClientRpc(FixedString64Bytes roomName, FixedString64Bytes roomLocatioName)
    {
        roomsPool.Find(room => room.name == roomName).ActivateDeactivte(roomSpawnPointsList.Find(location => location.name == roomLocatioName).transform);
    }
}
