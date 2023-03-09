using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowerPresenter : MonoBehaviour
{
    private Enemy enemy;
    private ThrowerView thrower;
    private GameObject kunai;

    public ThrowerPresenter(ThrowerView thrower, Enemy enemy, GameObject kunai)
    {
        this.thrower = thrower;
        this.enemy = enemy;
        this.kunai = kunai;
    }

    public void Enable()
    {
        enemy.death += Die;
        enemy.takenDamage += TakeDamage;
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
                thrower.animator.SetTrigger("Attack");
            }
        }
    }

    private void SwitchAttackPrepared(ref bool attackPrepared)
    {
        Debug.Log(attackPrepared);
        if (attackPrepared)
            attackPrepared = false;
        else
            attackPrepared = true;
    }

    private void Throw(float deffaultAttackSpeed, ref float attackSpeed)
    {
        GameObject kunaiThrown = Instantiate(kunai, thrower.transform.position, Quaternion.identity) as GameObject;
        attackSpeed = deffaultAttackSpeed;
        AttackPrepared();
    }

    public void Hit(float damage)
    {
        enemy.takeDamage(damage);
    }

    private void TakeDamage(float health)
    {
        thrower.TakeDamage(health);
    }

    private void Die()
    {
        thrower.animator.SetTrigger("Death");
    }

    public void Disable()
    {
        enemy.death -= Die;
        enemy.takenDamage -= TakeDamage;
        enemy.slashTickHandler -= SlashTimer;
        enemy.attackPreparedHandler -= SwitchAttackPrepared;
        enemy.throwWeaponHandler -= Throw;
    }
}
