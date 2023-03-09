using System;
using UnityEngine;

public class Enemy: MonoBehaviour
{
    protected float damage, hp, attackSpeed, deffaultAttackSpeed, attackPreparedTimer, maxHp, minAttackSpeed, maxAttackSpeed;
    private float stunTime = 2, stunRecover;
    protected bool stunned, attackPrepared;

    public event Action death;
    public event Action<float> takenDamage;

    public delegate void slash(ref float attackSpeed, ref bool attackPrepared);
    public slash slashTickHandler;

    public delegate void attackPreparation(ref bool attackPrepared);
    public attackPreparation attackPreparedHandler;

    public delegate void dealDamage(float damage, float deffaultAttackSpeed, ref float attackSpeed);
    public dealDamage dealDamageHandler;

    public delegate void throwWeapon(float deffaultAttackSpeed, ref float attackSpeed);
    public throwWeapon throwWeaponHandler;

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

    public void SlashTick()
    {
        if(!stunned)
            slashTickHandler?.Invoke(ref  attackSpeed, ref attackPrepared);
    }

    public void SwitchAttackPrepared()
    {
            attackPreparedHandler?.Invoke(ref attackPrepared);
    }

    public void DealDamage()
    {
        if (!stunned)
            dealDamageHandler?.Invoke(damage, deffaultAttackSpeed, ref attackSpeed);
    }

    public void ThrowWeapon()
    {
        if (!stunned)
            throwWeaponHandler?.Invoke(deffaultAttackSpeed, ref attackSpeed);
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
