using Unity.Netcode;
using UnityEngine;
public class WeaponScript : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;
    private float shotCooldown, reloadTime, meleeCooldown;
    private bool triggerPressed;
    private int bulletsShoot;
    public ulong owner;
    private bool aimDown;
    [SerializeField] private Transform meleePosition;
    private NetworkWeaponScript networkWeaponScript;

    private void Awake()
    {
        networkWeaponScript = GameObject.Find("NetworkScriptsAndVariables").GetComponent<NetworkWeaponScript>();
    }
    private void FixedUpdate()
    {
        if(shotCooldown>0)
            shotCooldown -= Time.deltaTime;
        if (meleeCooldown > 0)
            meleeCooldown -= Time.deltaTime;
        if (reloadTime>0)
            reloadTime -= Time.deltaTime;
        Shoot();
    }
    public void StartAttack()
    {
        if (aimDown)
            triggerPressed = true;
        else
            MeleeAttack();
    }
    public void FinishAttack()
    {
        triggerPressed = false;
    }
    private void PressWeaponTrigger()
    {
        triggerPressed = true;
    }
    private void ReleaseWeaponTrigger()
    {
        triggerPressed = false;
    }
    private void Shoot()
    {
        if (shotCooldown <= 0 && reloadTime <= 0 && triggerPressed && bulletsShoot < weaponData.AmmoAmount)
        {
            networkWeaponScript.ShootRpc(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.position, weaponData.FireSpread, weaponData.Damage, owner);
            bulletsShoot++;
            shotCooldown = 1/weaponData.FireRate;
        }
        if (!weaponData.Auto)
        {
            triggerPressed = false;
        }
    }
    private void MeleeAttack()
    {
        if (meleeCooldown <= 0)
        { 
            networkWeaponScript.MeleeRpc(meleePosition.position, transform.rotation, owner, weaponData.MeleeDamage);
            meleeCooldown = weaponData.MeleeRate;
        }
    }
    public void Reload()
    {
        bulletsShoot = 0;
        reloadTime = weaponData.ReloadSpeed;
    }
    public void AimDown()
    {
        if (meleeCooldown <= 0)
            aimDown = !aimDown;
    }
}
