using Unity.Netcode;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private NetworkWeaponScript networkWeaponScript;
    [SerializeField] private Transform meleePosition;
    [SerializeField] private SpriteRenderer weaponSprite;
    [SerializeField] private Transform weaponVisualTransform;

    private float shotCooldown, reloadTime, meleeCooldown;
    private bool triggerPressed;
    private int roundsInMagazine;
    private bool awaitingMagazine;
    public ulong owner;
    private bool aimDown;
    private Inventory inventory;

    public bool IsEquipped => weaponData != null;
    public bool IsMeleeWeapon => weaponData != null && weaponData.IsMelee;

    private bool IsLocalOwner =>
        NetworkManager.Singleton != null && owner == NetworkManager.Singleton.LocalClientId;

    private void Awake()
    {
        if (networkWeaponScript == null)
            networkWeaponScript = NetworkWeaponScript.Instance;
        if (weaponSprite == null)
            weaponSprite = GetComponentInChildren<SpriteRenderer>(true);

        if (weaponVisualTransform == null && weaponSprite != null)
            weaponVisualTransform = weaponSprite.transform;
    }

    public void Initialize(Inventory playerInventory)
    {
        inventory = playerInventory;
    }

    public void Equip(WeaponData data)
    {
        if (data == null)
        {
            Holster();
            return;
        }

        weaponData = data;
        gameObject.SetActive(true);

        if (weaponSprite != null)
        {
            if (data.Sprite != null)
                weaponSprite.sprite = data.Sprite;
            weaponSprite.enabled = data.Sprite != null;
        }

        ApplyWeaponVisualScale(data);

        roundsInMagazine = data.AmmoAmount;
        reloadTime = 0f;
        awaitingMagazine = false;
        shotCooldown = 0f;
        meleeCooldown = 0f;
        triggerPressed = false;
        aimDown = false;
    }

    public void Holster()
    {
        weaponData = null;
        triggerPressed = false;
        aimDown = false;
        awaitingMagazine = false;
        reloadTime = 0f;
        if (weaponVisualTransform != null)
            weaponVisualTransform.localScale = Vector3.one;
        gameObject.SetActive(false);
    }

    public void RefreshEquippedVisual()
    {
        if (weaponData == null)
            return;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (weaponSprite == null)
            weaponSprite = GetComponentInChildren<SpriteRenderer>(true);

        if (weaponVisualTransform == null && weaponSprite != null)
            weaponVisualTransform = weaponSprite.transform;

        if (weaponSprite == null)
            return;

        if (weaponData.Sprite != null)
            weaponSprite.sprite = weaponData.Sprite;
        weaponSprite.enabled = weaponData.Sprite != null;
        ApplyWeaponVisualScale(weaponData);
    }

    private void ApplyWeaponVisualScale(WeaponData data)
    {
        if (weaponVisualTransform == null || data == null)
            return;

        float scale = SpriteWorldScale.GetEquippedWeaponScale(data);
        weaponVisualTransform.localScale = Vector3.one * scale;
    }

    private void FixedUpdate()
    {
        if (!IsLocalOwner || weaponData == null)
            return;

        if (shotCooldown > 0)
            shotCooldown -= Time.deltaTime;
        if (meleeCooldown > 0)
            meleeCooldown -= Time.deltaTime;

        if (reloadTime > 0)
        {
            reloadTime -= Time.deltaTime;
            if (reloadTime <= 0f)
                TryConsumeMagazineFromInventory();
        }

        Shoot();
    }

    public void StartAttack()
    {
        if (!IsEquipped)
            return;

        if (weaponData.IsMelee || !aimDown)
        {
            MeleeAttack();
        }
        else
        {
            triggerPressed = true;
        }
    }

    public void FinishAttack()
    {
        triggerPressed = false;
    }

    private void Shoot()
    {
        if (weaponData.IsMelee)
            return;

        if (networkWeaponScript == null || shotCooldown > 0 || reloadTime > 0 || !triggerPressed || roundsInMagazine <= 0)
            return;

        networkWeaponScript.ShootRpc(
            Camera.main.ScreenToWorldPoint(Input.mousePosition),
            transform.position,
            weaponData.FireSpread,
            weaponData.Damage,
            owner);
        roundsInMagazine--;
        shotCooldown = 1 / weaponData.FireRate;

        if (!weaponData.Auto)
            triggerPressed = false;
    }

    private void MeleeAttack()
    {
        if (networkWeaponScript == null || meleeCooldown > 0)
            return;

        networkWeaponScript.MeleeRpc(meleePosition.position, transform.rotation, owner, weaponData.MeleeDamage);
        meleeCooldown = weaponData.MeleeRate;
    }

    public void Reload()
    {
        if (!IsLocalOwner || !IsEquipped || inventory == null || reloadTime > 0 || awaitingMagazine)
            return;

        if (weaponData.IsMelee)
            return;

        if (roundsInMagazine >= weaponData.AmmoAmount)
            return;

        roundsInMagazine = 0;
        reloadTime = weaponData.ReloadSpeed;
        awaitingMagazine = true;
    }

    private void TryConsumeMagazineFromInventory()
    {
        if (!awaitingMagazine || inventory == null || weaponData == null)
            return;

        inventory.ConsumeMagazineRpc(weaponData.AmmoItemId);
    }

    public void ApplyMagazineReload()
    {
        if (weaponData == null)
            return;

        awaitingMagazine = false;
        roundsInMagazine = weaponData.AmmoAmount;
    }

    public void CancelMagazineReload()
    {
        awaitingMagazine = false;
        roundsInMagazine = 0;
    }

    public void AimDown()
    {
        SetAiming(!aimDown);
    }

    public void SetAiming(bool aiming)
    {
        if (!IsEquipped || meleeCooldown > 0)
        {
            if (!aiming)
                aimDown = false;
            return;
        }

        aimDown = aiming;
    }
}
