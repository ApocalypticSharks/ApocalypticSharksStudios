using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : NetworkBehaviour
{
    [SerializeField] public float playerSpeed, sprintSpeed, walkSpeed;
    private PlayerInput playerInput;
    public Rigidbody2D rigidbody2D;
    [SerializeField] private WeaponScript weapon;
    [SerializeField] private Transform weaponTransform;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private ReliquaryCarrier reliquaryCarrier;

    public NetworkVariable<bool> IsSprinting = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public bool IsDead { get; private set; }

    private void Awake()
    {
        playerSpeed = walkSpeed;
        playerInput = new PlayerInput();
        playerInput.Enable();
        playerInput.PlayerActions.Sprint.started += StartSprint;
        playerInput.PlayerActions.Sprint.canceled += StopSprint;
        playerInput.PlayerActions.Attack.started += StartAttack;
        playerInput.PlayerActions.Attack.canceled += FinishAttack;
        playerInput.PlayerActions.Aim.started += StartAim;
        playerInput.PlayerActions.Aim.canceled += StopAim;
        playerInput.PlayerActions.Reload.performed += Reload;
        playerInput.PlayerActions.Interact.performed += Interact;
        playerInput.PlayerActions.UseItem.performed += UseItem;
        playerInput.PlayerActions.DropItem.performed += DropItem;
        rigidbody2D = GetComponent<Rigidbody2D>();
        reliquaryCarrier = GetComponent<ReliquaryCarrier>();
    }

    public override void OnNetworkSpawn()
    {
        weapon.owner = OwnerClientId;
        weapon.Initialize(GetComponent<Inventory>());
        weapon.Holster();

        if (IsOwner)
        {
            var cameraFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
            if (cameraFollow != null)
                cameraFollow.SetTarget(transform);
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner || IsDead)
            return;

        if (IsCarryingReliquary())
        {
            IsSprinting.Value = false;
            playerSpeed = walkSpeed;
        }

        rigidbody2D.linearVelocity = playerInput.PlayerActions.Movement.ReadValue<Vector2>() * playerSpeed;
    }

    private void LateUpdate()
    {
        if (!IsOwner || IsDead)
            return;

        ApplyLookDirection();
    }

    private void ApplyLookDirection()
    {
        var lookDirection = GetLookDirection();
        if (lookDirection.sqrMagnitude > 0.0001f)
            transform.up = -lookDirection;

        if (!weapon.IsEquipped)
            return;

        weaponTransform.up = lookDirection;
    }

    private Vector2 GetLookDirection()
    {
        if (Camera.main == null)
            return Vector2.zero;

        var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(
            mouseWorldPosition.x - transform.position.x,
            mouseWorldPosition.y - transform.position.y).normalized;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer || playerHealth.IsDead.Value)
            return;

        switch (collision.gameObject.tag)
        {
            case "bullet":
                var bullet = collision.gameObject.GetComponent<BulletScript>();
                if (OwnerClientId != bullet.owner.Value)
                {
                    playerHealth.TakeDamage(bullet.damage.Value);
                    bullet.DestroyBulletRpc();
                }
                break;
            case "meleeHitBox":
                var melee = collision.gameObject.GetComponent<MeleeHitboxScript>();
                if (OwnerClientId != melee.owner.Value)
                    playerHealth.TakeDamage(melee.damage.Value);
                break;
        }
    }

    public void HandleDeath()
    {
        IsDead = true;

        if (IsServer && reliquaryCarrier != null)
            reliquaryCarrier.DropOnDeath(transform.position);

        if (IsOwner)
        {
            playerInput.Disable();
            rigidbody2D.linearVelocity = Vector2.zero;
            IsSprinting.Value = false;
        }

        weapon.gameObject.SetActive(false);

        var collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;
    }

    private void StartSprint(InputAction.CallbackContext context)
    {
        if (IsOwner && !IsDead && !IsCarryingReliquary())
            IsSprinting.Value = true;
    }

    private void StopSprint(InputAction.CallbackContext context)
    {
        if (IsOwner)
            IsSprinting.Value = false;
    }

    public void StopSprintFromStamina()
    {
        if (IsOwner)
            IsSprinting.Value = false;

        playerSpeed = walkSpeed;
    }

    private void StartAttack(InputAction.CallbackContext context)
    {
        if (IsOwner && !IsDead && !IsCarryingReliquary())
            weapon.StartAttack();
    }

    private void FinishAttack(InputAction.CallbackContext context)
    {
        if (IsOwner)
            weapon.FinishAttack();
    }

    private void Reload(InputAction.CallbackContext context)
    {
        if (IsOwner && !IsDead && !IsCarryingReliquary())
            weapon.Reload();
    }

    private void StartAim(InputAction.CallbackContext context)
    {
        if (IsOwner && !IsDead && !IsCarryingReliquary())
            weapon.SetAiming(true);
    }

    private void StopAim(InputAction.CallbackContext context)
    {
        if (IsOwner)
            weapon.SetAiming(false);
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (IsOwner && !IsDead && !IsCarryingReliquary())
            playerInteraction.TryInteractNearby();
    }

    public void UseItem(InputAction.CallbackContext context)
    {
        if (IsOwner && !IsDead && !IsCarryingReliquary())
            playerInteraction.TryUseSelectedItem();
    }

    private void DropItem(InputAction.CallbackContext context)
    {
        if (IsOwner && !IsDead)
            playerInteraction.TryDropSelectedItem();
    }

    public bool IsCarryingReliquary()
    {
        return reliquaryCarrier != null && reliquaryCarrier.IsCarrying.Value;
    }
}
