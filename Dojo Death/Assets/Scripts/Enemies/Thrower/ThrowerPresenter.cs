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
        enemy.slashHandler += Slash;
    }

    public void DoSlash()
    {
        enemy.Slash();
    }

    private void Slash(float damage, ref float attackSpeed, float deffaultAttackSpeed, ref float attackPreparedTimer, ref bool attackPrepared)
    {
        if (attackSpeed > 0)
            attackSpeed -= Time.deltaTime;
        else
        {
            if (!attackPrepared)
                thrower.animator.SetTrigger("Attack");
            attackPrepared = true;
            if (attackPreparedTimer > 0)
                attackPreparedTimer -= Time.deltaTime;
            else
            {
                thrower.Slash();
                GameObject kunaiThrown = Instantiate(kunai, thrower.transform.position, Quaternion.identity) as GameObject;
                attackSpeed = deffaultAttackSpeed;
                attackPreparedTimer = 0.5f;
            }
        }
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
        enemy.slashHandler -= Slash;
    }
}
