using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowerView : MonoBehaviour
{

    public ThrowerPresenter throwerPresenter;
    public Animator animator;

    private void OnEnable()
    {
        throwerPresenter?.Enable();
    }
    private void Update()
    {
        throwerPresenter.SlashTimerTick();
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
        throwerPresenter.Disable();
        transform.parent = null;
        gameObject.SetActive(false);
    }

    public void AttackPrepared()
    {
        throwerPresenter.AttackPrepared();
    }

    public void DoSlash()
    {
        throwerPresenter.DoSlash();
    }
}
