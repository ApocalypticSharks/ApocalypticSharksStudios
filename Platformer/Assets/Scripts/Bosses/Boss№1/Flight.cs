using System;
using System.Collections.Generic;
using UnityEngine;

public class Flight : MonoBehaviour
{
    GameObject player;
    Vector3 startPosition = new Vector3(-10, 2);
    Vector3 endPosition = new Vector3(-10, -2);
    float progress;
    bool flag = true;
    bool attack = false;
    [SerializeField]Animator animator;
    private void Start()
    {
        player = GameObject.Find("DrawCharacter");
        transform.position = startPosition;
    }
    void FixedUpdate()
    {

        // if ((transform.position.y == endPosition.y || transform.position.y == startPosition.y) &&
        // Math.Abs(player.transform.position.y) - Math.Abs(transform.position.y) <= 1)
        // {
        //     attack = true;
        //     transform.position = Vector3.Lerp(transform.position, player.transform.position, progress += 0.0002F);
        // }
        // else
        // {
        //     attack = false;
        // }
        handMovement(attack);
    }

    void handMovement(bool attack)
    {
        if (!attack)
        {
            if (flag)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, progress += 0.01F);
                if (transform.position.y == endPosition.y)
                {
                    progress = 0f;
                    flag = !flag;
                }
            }
            else
            {
                transform.position = Vector3.Lerp(endPosition, startPosition, progress += 0.01F);
                if (transform.position.y == startPosition.y)
                {
                    progress = 0f;
                    flag = !flag;
                }
            }
        }
    }
}