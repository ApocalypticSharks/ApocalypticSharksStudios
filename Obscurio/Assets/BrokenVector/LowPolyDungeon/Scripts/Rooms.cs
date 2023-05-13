using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rooms : MonoBehaviour
{
    public GameObject roomEntity;
    public Rooms(Object roomObject)
    {
        roomEntity = Instantiate(roomObject) as GameObject;
        roomEntity.SetActive(false);
        roomEntity.transform.SetParent(GameObject.Find("RoomPool").transform);
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
            roomEntity.SetActive(true);
            roomEntity.GetComponent<Transform>().position = spawnPoint.position;
            roomEntity.transform.SetParent(spawnPoint);
        }
    }
}
