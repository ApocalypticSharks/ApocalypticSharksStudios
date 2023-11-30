using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 startPosition, endPosition;
    [SerializeField] private float movingSpeed;
    [SerializeField] private Vector3 rotatingPoint;
    [SerializeField] bool isRotating;

    private void FixedUpdate()
    {
        if (isRotating)
        {
            transform.RotateAround(rotatingPoint, new Vector3(0, 0, 1), movingSpeed * Time.deltaTime);
        }
        else 
        {
            transform.position = Vector3.MoveTowards(transform.position, endPosition, movingSpeed * Time.deltaTime);
            if (transform.position == endPosition)
                transform.position = startPosition;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collision.transform.SetParent(transform, true);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        collision.transform.parent = null;
    }
}
