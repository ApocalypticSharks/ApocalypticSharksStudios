using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmerPresenter: MonoBehaviour
{
    private Enemy enemy;
    private FarmerView farmer;
    private MasterScript master;

    public FarmerPresenter(FarmerView farmer, Enemy enemy, MasterScript master)
    {
        this.farmer = farmer;
        this.enemy = enemy;
        this.master = master;
    }

    public void Enable()
    {
        enemy.death += Die;
        enemy.takenDamage += TakeDamage;
        enemy.slashTickHandler += SlashTimer;
        enemy.stunHandler += Stun;
        enemy.attackPreparedHandler += SwitchAttackPrepared;
        enemy.dealDamageHandler += DealDamage;
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
        enemy.DealDamage();
    }
    public void Hit()
    {
        enemy.takeDamage(master.damage);
    }
    public void StunCheck()
    {
        enemy.StunCheck();
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
                farmer.animator.SetTrigger("Attack");
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

    private void DealDamage(float damage, float deffaultAttackSpeed, ref float attackSpeed)
    {
        master.hp -= damage;
        attackSpeed = deffaultAttackSpeed;
        AttackPrepared();
    }
    private void Stun(ref bool stunned, float stunTime, ref float stunRecover)
    {
        if (stunned)
        {
            stunRecover -= Time.deltaTime;
            if (stunRecover <= 0)
            {
                stunned = false;
                stunRecover = stunTime;
            }

        }
    }

    private void TakeDamage(float health)
    {
        farmer.TakeDamage(health);
    }

    private void Die()
    {
        farmer.animator.SetTrigger("Death");
    }

    public void Disable()
    {
        enemy.death -= Die;
        enemy.takenDamage -= TakeDamage;
        enemy.slashTickHandler -= SlashTimer;
        enemy.stunHandler -= Stun;
        enemy.attackPreparedHandler -= SwitchAttackPrepared;
        enemy.dealDamageHandler -= DealDamage;
    }
}
