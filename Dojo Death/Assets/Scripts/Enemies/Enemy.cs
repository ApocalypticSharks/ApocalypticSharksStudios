using System;
using UnityEngine;

public class Enemy: MonoBehaviour
{
    protected float damage, hp, attackSpeed, deffaultAttackSpeed, attackPreparedTimer, maxHp, minAttackSpeed, maxAttackSpeed;
    private float stunTime = 2, stunRecover;
    protected bool stunned, attackPrepared;

    public event Action death;
    public event Action<float> takenDamage;

    public delegate void slash(float damage, ref float attackSpeed, float deffaultAttackSpeed, ref float attackPreparedTimer, ref bool attackPrepared);
    public slash slashHandler;

    public delegate void stun(ref bool stunned, float stunTime, ref float stunRecover);
    public stun stunHandler;

    public void takeDamage(float damage)
    {
        if (attackPrepared)
            stunned = true;

        hp -= damage;
        if (hp > 0)
            takenDamage?.Invoke(hp);
        else
        {
            death?.Invoke();
            resetStats();
        }
    }

    public void Slash()
    {
        if(!stunned)
            slashHandler?.Invoke(damage, ref  attackSpeed,  deffaultAttackSpeed,  ref attackPreparedTimer,  ref attackPrepared);
    }

    public void StunCheck()
    {
        stunHandler?.Invoke(ref stunned, stunTime, ref stunRecover);
    }

    public void resetStats()
    {
        hp = maxHp;
        attackSpeed = UnityEngine.Random.Range(minAttackSpeed, maxAttackSpeed);
    }

    public Enemy(float damage, float maxHp, float minAttackSpeed, float maxAttackSpeed, float attackPreparedTimer)
    {
        this.damage = damage;
        this.maxHp = maxHp;
        this.hp = maxHp;
        this.maxAttackSpeed = maxAttackSpeed;
        this.minAttackSpeed = minAttackSpeed;
        this.attackSpeed = UnityEngine.Random.Range(minAttackSpeed, maxAttackSpeed);
        this.attackPreparedTimer = attackPreparedTimer;
        deffaultAttackSpeed = attackSpeed;
    }

    public Enemy(float damage, float minAttackSpeed, float maxAttackSpeed, float attackPreparedTimer)
    {
        this.damage = damage;
        this.attackSpeed = UnityEngine.Random.Range(minAttackSpeed, maxAttackSpeed);
        this.attackPreparedTimer = attackPreparedTimer;
        deffaultAttackSpeed = attackSpeed;
    }
}
