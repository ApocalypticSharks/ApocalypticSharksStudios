using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 startPosition, endPosition;
    [SerializeField] private float movingSpeed;
    [SerializeField] bool isRotating, isTraveling;
    private Vector3 destination;

    private void Awake()
    {
        destination = endPosition;
    }
    private void FixedUpdate()
    {
        if (isRotating)
        {
            transform.GetChild(0).RotateAround(transform.position, new Vector3(0, 0, 1), movingSpeed * Time.deltaTime);
        }
        else
        {
            if (isTraveling)
            {
                transform.position = Vector3.MoveTowards(transform.position, destination, movingSpeed * Time.deltaTime);
                if (transform.position == endPosition)
                    destination = startPosition;
                if (transform.position == startPosition)
                    destination = endPosition;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, endPosition, movingSpeed * Time.deltaTime);
                if (transform.position == endPosition)
                    transform.position = startPosition;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collision.transform.parent.SetParent(transform, true);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        collision.transform.parent.parent = null;
    }
}
