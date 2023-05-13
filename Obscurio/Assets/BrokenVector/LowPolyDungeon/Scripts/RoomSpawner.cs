using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
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
        for (int i = 0; i < roomSpawnPointsList.Count; i++)
        {
            List<Rooms> freeRooms = roomsPool.Where(room => !room.roomEntity.activeSelf).ToList();
            freeRooms[Random.Range(0, freeRooms.Count)].ActivateDeactivte(roomSpawnPointsList[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
