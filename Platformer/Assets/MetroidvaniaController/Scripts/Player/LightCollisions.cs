using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightCollisions : MonoBehaviour
{
    [SerializeField] private Attack _playerAttacks;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Light"))
            _playerAttacks.collidingLight = collision.transform;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Light"))
            _playerAttacks.collidingLight = null;
    }
}
