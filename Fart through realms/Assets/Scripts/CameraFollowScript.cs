using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraFollowScript : MonoBehaviour
{
    public Transform player;
    //[SerializeField] private float maxHeight, minHeight;

    private void Update()
    {
        if (math.abs(transform.position.y - player.position.y) >= 2)
        {
            Vector3 newPositionY = transform.position;
            newPositionY.y = Mathf.Lerp(transform.position.y, player.transform.position.y, 1 * Time.deltaTime);
            transform.position = newPositionY;
        }
    }
}
