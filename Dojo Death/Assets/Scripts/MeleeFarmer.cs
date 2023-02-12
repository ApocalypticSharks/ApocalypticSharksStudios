using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeFarmer : MonoBehaviour
{
    [SerializeField] private float hp = 50;
    [SerializeField] private float damage = 5;
    [SerializeField] private bool stuned, attackPrepared;
    [SerializeField] private float attackSpeed;
    private DojoScript dojoParams;
    private MasterScript master;

    private void Start()
    {
        master = GameObject.Find("Master").GetComponent<MasterScript>();
        dojoParams = GameObject.Find("Dojo").GetComponent<DojoScript>();
        attackSpeed = Random.Range(1.0f, 2.0f);
        StartCoroutine(Slash(attackSpeed));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "hitbox")
        {
            if (attackPrepared)
            {
                stuned = true;
                attackPrepared = false;
                this.gameObject.GetComponent<Renderer>().material.color = new Color(255, 0, 100);
                Debug.Log("Parried");
                StopAllCoroutines();
                StartCoroutine(Stun());
            }
            else
            {
                hp -= master.damage;
                if (hp <= 0)
                    Destroy(this.gameObject);
            }
        }
    }

    IEnumerator Slash(float attackSpeed)
    {
        yield return new WaitForSeconds(attackSpeed);
        attackPrepared = true;
        this.gameObject.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
        yield return new WaitForSeconds(0.3f);
        attackPrepared = false;
        master.hp -= damage;
        this.gameObject.GetComponent<Renderer>().material.color = new Color(255, 255, 255);
        StartCoroutine(Slash(attackSpeed));
    }

    IEnumerator Stun()
    {
        yield return new WaitForSeconds(1);
        this.gameObject.GetComponent<Renderer>().material.color = new Color(0, 0, 0);
        StartCoroutine(Slash(attackSpeed));
    }

    private void OnDestroy()
    {
        dojoParams.yen += 10;
    }
}
