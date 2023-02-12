using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeThrower : MonoBehaviour
{
    [SerializeField] private float hp = 50;
    [SerializeField] private bool stuned, attackPrepared;
    [SerializeField] private float attackSpeed;
    private GameObject character;
    [SerializeField] private Object weapon;
    private DojoScript dojoParams;

    private void Start()
    {
        dojoParams = GameObject.Find("Dojo").GetComponent<DojoScript>();
        character = GameObject.Find("Master");
        attackSpeed = Random.Range(1.0f, 2.0f);
        StartCoroutine(Throw(attackSpeed));
    }

    IEnumerator Throw(float attackSpeed)
    {
        yield return new WaitForSeconds(attackSpeed);
        attackPrepared = true;
        this.gameObject.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
        yield return new WaitForSeconds(0.3f);
        attackPrepared = false;
        GameObject projectile = Instantiate(
            weapon,
            transform.position,
            Quaternion.FromToRotation(transform.position, character.transform.position)) as GameObject;
        this.gameObject.GetComponent<Renderer>().material.color = new Color(255, 255, 255);
        StartCoroutine(Throw(attackSpeed));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "friendlyKunai")
        {
            hp -= collision.gameObject.GetComponent<MasterKunaiScript>().damage;
            if (hp <= 0)
                Destroy(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        dojoParams.yen += 10;
    }
}
