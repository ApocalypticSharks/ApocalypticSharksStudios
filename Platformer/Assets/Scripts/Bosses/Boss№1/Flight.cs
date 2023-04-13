using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flight : MonoBehaviour
{
    GameObject player;
    Vector3 startPosition = new Vector3(-10, 1.4f);
    Vector3 endPosition = new Vector3(-10, -2.774701f);
    float progress;
    bool flag = true;
    bool canAttack = false;
    static int attackDirection = 0;
    bool AttackForward = false;
    int AttackCounter = 0;
    private void Start()
    {
        player = GameObject.Find("Gufik");
        transform.position = startPosition;
    }
    void FixedUpdate()
    {
        HandMovement(canAttack);
        //Добавить флаг для увеличения скорости между атаками
        if (AttackCounter < 3)
        {
            if (attackDirection == 0)
            {
                attackDirection = UnityEngine.Random.Range(0, 101);
            }
            if (attackDirection >= 30)
            {
                if (player.transform.position.y < 0 && transform.position.y == endPosition.y)
                {
                    AttackDown();
                }

                if (player.transform.position.y > 0 && transform.position.y == startPosition.y)
                {
                    AttackUp();
                }
            }
            else
            {
                if (player.transform.position.y > 0 && transform.position.y == endPosition.y)
                {
                    AttackDown();
                }

                if (player.transform.position.y < 0 && transform.position.y == startPosition.y)
                {
                    AttackUp();
                }
            }

        }
        else
        {
            StartCoroutine(AttackCooldown());
        }
    }

    void HandMovement(bool canAttack)
    {
        if (!canAttack)
        {
            if (flag)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, progress += 0.03F);
                if (transform.position.y == endPosition.y)
                {
                    progress = 0f;
                    flag = !flag;
                }
            }
            else
            {
                transform.position = Vector3.Lerp(endPosition, startPosition, progress += 0.03F);
                if (transform.position.y == startPosition.y)
                {
                    progress = 0f;
                    flag = !flag;
                }
            }
        }
    }
    void AttackDown()
    {
        canAttack = true;
        {

            if (AttackForward)
            {
                transform.position = Vector3.Lerp(transform.position, new Vector3(10, transform.position.y), progress = 0.06F);
                if (Vector3.Distance(transform.position, new Vector3(10, transform.position.y)) <= 0.1)
                {
                    AttackForward = !AttackForward;
                }
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, new Vector3(-10, transform.position.y), progress = 0.06F);
                if (Vector3.Distance(transform.position, new Vector3(-10, transform.position.y)) <= 0.1)
                {
                    AttackForward = !AttackForward;
                    attackDirection = 0;
                    canAttack = false;
                    AttackCounter++;
                }
            }
        }

    }

    void AttackUp()
    {
        canAttack = true;
        {

            if (AttackForward)
            {
                transform.position = Vector3.Lerp(transform.position, new Vector3(10, transform.position.y), progress = 0.06F);
                if (Vector3.Distance(transform.position, new Vector3(10, transform.position.y)) <= 0.1)
                {
                    AttackForward = !AttackForward;
                }
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, new Vector3(-10, transform.position.y), progress = 0.06F);
                if (Vector3.Distance(transform.position, new Vector3(-10, transform.position.y)) <= 0.1)
                {
                    AttackForward = !AttackForward;
                    attackDirection = 0;
                    canAttack = false;
                    AttackCounter++;
                }
            }
        }
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(2f);
        AttackCounter = 0;
        canAttack = false;
    }
}