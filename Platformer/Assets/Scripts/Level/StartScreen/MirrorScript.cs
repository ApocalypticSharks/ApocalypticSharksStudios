using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorScript : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Slap":
                animator.SetTrigger("Switch");
                break;
            case "Projectile":
                collision.transform.position = transform.position;
                collision.gameObject.GetComponent<Rigidbody2D>().velocity = transform.right.normalized * 15;
                break;
        }
    }
}
