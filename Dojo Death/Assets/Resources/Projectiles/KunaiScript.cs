using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KunaiScript : MonoBehaviour
{
    public float damage;
    private GameObject master;
    private Vector3 masterPosition;
    private void Start()
    {
        master = GameObject.Find("Master");
        masterPosition = master.transform.position;
    }
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, masterPosition, 7 * Time.deltaTime);
        if (transform.position == masterPosition)
            Destroy(this.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "catcher")
        {
            if (master.GetComponent<MasterScript>().kunai < 3)
                master.GetComponent<MasterScript>().kunai++;
            Destroy(this.gameObject);
        }
        if (collision.gameObject.tag == "master")
        {
            master.GetComponent<MasterScript>().hp -= damage;
            Destroy(this.gameObject);
        }          
    }
}
