using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Rooms : MonoBehaviour
{
    public GameObject roomEntity;
    public FixedString64Bytes name;
    public bool isExit;
    public Rooms(Object roomObject)
    {
        roomEntity = Instantiate(roomObject) as GameObject;
        roomEntity.SetActive(false);
        roomEntity.transform.SetParent(GameObject.Find("RoomPool").transform);
        name = roomEntity.name;
    }
    public void ActivateDeactivte(Transform spawnPoint)
    {
        if (roomEntity.activeSelf)
        {
            roomEntity.SetActive(false);
            roomEntity.transform.SetParent(GameObject.Find("RoomPool").transform);
        }
        else
        {
            isExit = false;
            roomEntity.SetActive(true);
            roomEntity.GetComponent<Transform>().position = spawnPoint.position;
            roomEntity.transform.SetParent(spawnPoint);
        }
    }
}
