using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapons : MonoBehaviour
{
    protected ProjectilePool projectile;
    public float damage;
    public float attackSpeed;

    private void Awake()
    {
        projectile = GetComponent<ProjectilePool>();
    }
    public Weapons()
    {
        this.damage = 1;
        this.attackSpeed = 0.25f;
    }

    public abstract void Shoot(Transform collidingLight, Vector3 playerPosition);
}
