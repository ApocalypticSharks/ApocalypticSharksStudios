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
        enemy.slashHandler += Slash;
        enemy.stunHandler += Stun;
    }

    public void DoSlash()
    { 
        enemy.Slash();
    }
    public void Hit()
    {
        enemy.takeDamage(master.damage);
    }
    public void StunCheck()
    {
        enemy.StunCheck();
    }

    private void Slash(float damage, ref float attackSpeed, float deffaultAttackSpeed, ref float attackPreparedTimer,ref bool attackPrepared)
    {
        if (attackSpeed > 0)
            attackSpeed -= Time.deltaTime;
        else
        {
            if (!attackPrepared)
                farmer.animator.SetTrigger("Attack");
            attackPrepared = true;
            if (attackPreparedTimer > 0)
                attackPreparedTimer -= Time.deltaTime;
            else 
            {
                farmer.Slash();
                master.hp -= damage;
                attackSpeed = deffaultAttackSpeed;
                attackPreparedTimer = 0.5f;
            }
        }
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
        enemy.slashHandler -= Slash;
    }
}
