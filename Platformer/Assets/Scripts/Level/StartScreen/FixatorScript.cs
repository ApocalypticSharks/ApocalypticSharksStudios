using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixatorScript : MonoBehaviour
{
    [SerializeField] private GameObject fixator, exit;
    [SerializeField] private Transform platform;
    private bool triggered;
    private float fallIntesivity = 0.03f;
    private Vector3 fallDestination;

    private void Start()
    {
        fallDestination = new Vector3(13f, -1, 0);
    }

    private void FixedUpdate()
    {
        if (triggered)
        {
            platform.position = Vector3.Lerp(platform.position, fallDestination, 0.02F);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Projectile")
        {
            Destroy(fixator);
            triggered = true;
            exit.SetActive(true);
        }
    }
}
