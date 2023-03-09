using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssassinView : MonoBehaviour
{
    public AssassinPresenter assassinPresenter;
    public Animator animator;

    private void OnEnable()
    {
        assassinPresenter?.Enable();
    }
    private void Update()
    {
        assassinPresenter.SlashTimerTick();
    }

    public void TakeDamage(float health)
    {

    }

    // Update is called once per frame
    public void Die()
    {
        transform.parent = null;
        gameObject.SetActive(false);
    }

    public void AttackPrepared()
    {
        assassinPresenter.AttackPrepared();
    }

    public void DoSlash()
    {
        assassinPresenter.DoSlash();
    }
}
