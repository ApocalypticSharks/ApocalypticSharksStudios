using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterKunaiScript : MonoBehaviour
{
    public float damage;
    public Vector3 direction;
    private void Start()
    {
        damage = 25;
    }
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, direction, 7 * Time.deltaTime);
        if (transform.position == direction)
            Destroy(this.gameObject);
    }

}
