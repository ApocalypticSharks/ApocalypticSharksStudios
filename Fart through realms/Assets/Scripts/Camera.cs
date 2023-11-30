using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    GameObject instance;
    public Camera(Transform player)
    {
        instance = Instantiate(Resources.Load("Main Camera")) as GameObject;
        instance.GetComponent<CameraFollowScript>().player = player;
    }
}
