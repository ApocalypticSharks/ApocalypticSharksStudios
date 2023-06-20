using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class RoomSpawner : NetworkBehaviour
{
    [SerializeField] private List<Object> roomsList = new List<Object>();
    [SerializeField] private List<Transform> roomSpawnPointsList = new List<Transform>();
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
        List<Transform> freeRoomLocations = roomSpawnPointsList.Where(roomLocation => roomLocation.childCount == 0).ToList();
        if (freeRoomLocations.Count > 0)
        {
            List<Rooms> freeRooms = roomsPool.Where(room => !room.roomEntity.activeSelf).ToList();
            freeRooms[Random.Range(0, freeRooms.Count)].ActivateDeactivte(roomSpawnPointsList[Random.Range(0, freeRoomLocations.Count)]);
        }
    }
}
