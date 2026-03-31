using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkWeaponScript : NetworkBehaviour
{
    [SerializeField] Object bullet;
    [SerializeField] Object meleeHitbox;
    [Rpc(SendTo.Server)]
    public void ShootRpc(Vector2 mousePosition, Vector3 playerPosition, int fireSpread, int damage, ulong owner)
    {
        GameObject thrownBullet = Instantiate(bullet, playerPosition, Quaternion.identity) as GameObject;
        NetworkObject thrownBulletNetworkObject = thrownBullet.GetComponent<NetworkObject>();
        thrownBulletNetworkObject.Spawn();
        thrownBulletNetworkObject.GetComponent<BulletScript>().owner.Value = owner;
        thrownBulletNetworkObject.GetComponent<BulletScript>().damage.Value = damage;
        thrownBulletNetworkObject.GetComponent<BulletScript>().target = Random.insideUnitCircle * 0.5f * (fireSpread / 10 + Vector2.Distance((Vector2)playerPosition, mousePosition) / 8) + mousePosition;
    }
    [Rpc(SendTo.Server)]
    public void MeleeRpc(Vector3 position, Quaternion rotation, ulong owner, int damage)
    {
        GameObject thrownMelee = Instantiate(meleeHitbox, position, rotation) as GameObject;
        NetworkObject thrownMeleeNetworkObject = thrownMelee.GetComponent<NetworkObject>();
        thrownMeleeNetworkObject.Spawn();
        thrownMeleeNetworkObject.GetComponent<MeleeHitboxScript>().owner.Value = owner;
        thrownMeleeNetworkObject.GetComponent<MeleeHitboxScript>().damage.Value = damage;
    }
}
