using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBlaster : Weapons
{
    public override void Shoot(Transform collidingLight, Vector3 playerPosition)
    {
        GameObject activeProjectile = projectile.ActivateProjectile(projectile.lightBlaster);
        activeProjectile.GetComponent<Rigidbody2D>().velocity = (playerPosition - collidingLight.position).normalized * 15;
    }
}
