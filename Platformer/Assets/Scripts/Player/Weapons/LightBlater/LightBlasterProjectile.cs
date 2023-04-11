using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBlasterProjectile : MonoBehaviour
{
    private float damage = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Boss"))
        {
            //collision.gameObject.GetComponent<Boss>().stats.hp -= damage;
            gameObject.SetActive(false);
        }
    }
}
