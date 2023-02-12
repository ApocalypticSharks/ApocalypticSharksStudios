using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowerView : MonoBehaviour
{

    public ThrowerPresenter throwerPresenter;

    private void OnEnable()
    {
        throwerPresenter?.Enable();
    }
    private void Update()
    {
        throwerPresenter.DoSlash();
    }
    public void TakeDamage(float health)
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "friendlyKunai")
            throwerPresenter.Hit(collision.gameObject.GetComponent<MasterKunaiScript>().damage);
    }

    // Update is called once per frame
    public void Die()
    {
        transform.parent = null;
        gameObject.SetActive(false);
        Debug.Log("Died");
        this.gameObject.GetComponent<Renderer>().material.color = new Color(255, 15, 15);
    }

    public void AttackPrepared()
    {
        this.gameObject.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
    }

    public void Slash()
    {
        this.gameObject.GetComponent<Renderer>().material.color = new Color(255, 255, 255);
    }
}
