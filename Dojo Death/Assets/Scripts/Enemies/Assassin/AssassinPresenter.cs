using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssassinPresenter : MonoBehaviour
{
    private Enemy enemy;
    private AssassinView assassin;
    private GameObject kunai;

    public AssassinPresenter(AssassinView assassin, Enemy enemy, GameObject kunai)
    {
        this.assassin = assassin;
        this.enemy = enemy;
        this.kunai = kunai;
    }

    public void Enable()
    {
        enemy.slashTickHandler += SlashTimer;
        enemy.attackPreparedHandler += SwitchAttackPrepared;
        enemy.throwWeaponHandler += Throw;
    }

    public void SlashTimerTick()
    {
        enemy.SlashTick();
    }

    public void AttackPrepared()
    {
        enemy.SwitchAttackPrepared();
    }
    public void DoSlash()
    {
        enemy.ThrowWeapon();
    }
    private void SlashTimer(ref float attackSpeed, ref bool attackPrepared)
    {
        if (!attackPrepared)
        {
            if (attackSpeed > 0)
                attackSpeed -= Time.deltaTime;
            else
            {
                AttackPrepared();
                assassin.animator.SetTrigger("Attack");
            }
        }
    }

    private void SwitchAttackPrepared(ref bool attackPrepared)
    {
        if (attackPrepared)
            attackPrepared = false;
        else
            attackPrepared = true;
    }

    private void Throw(float deffaultAttackSpeed, ref float attackSpeed)
    {
        assassin.StartCoroutine(ThrowKunai());
        attackSpeed = deffaultAttackSpeed;
        AttackPrepared();
    }

    IEnumerator ThrowKunai()
    {
        Debug.Log("Here");
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.1f);
            GameObject kunaiThrown = Instantiate(kunai, assassin.transform.position, Quaternion.identity) as GameObject;
        }
        Disable();
    }

    public void Disable()
    {
        enemy.slashTickHandler -= SlashTimer;
        enemy.attackPreparedHandler -= SwitchAttackPrepared;
        enemy.throwWeaponHandler -= Throw;
    }
}
