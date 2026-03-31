using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : NetworkBehaviour
{
    [SerializeField] public float playerSpeed, sprintSpeed, walkSpeed,
                                    Value, staminaRecoverDelay, staminaRecoverCooldown;
    private PlayerInput playerInput;
    public Rigidbody2D rigidbody2D;
    [SerializeField] private WeaponScript weapon;
    [SerializeField] private Transform weaponTransform;
    [SerializeField]public bool isSprinting;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Inventory inventory;

    private void Awake()
    {
        playerSpeed = walkSpeed;
        playerInput = new PlayerInput();
        playerInput.Enable();
        playerInput.PlayerActions.Sprint.started += StartSprint;
        playerInput.PlayerActions.Sprint.canceled += StopSprint;
        playerInput.PlayerActions.Attack.started += StartAttack;
        playerInput.PlayerActions.Attack.canceled += FinishAttack;
        playerInput.PlayerActions.Aim.started += AimDown;
        playerInput.PlayerActions.Aim.canceled += AimDown;
        playerInput.PlayerActions.Reload.performed += Reload;
        playerInput.PlayerActions.Interact.performed += Interact;
        rigidbody2D = GetComponent<Rigidbody2D>();
    }
    public override void OnNetworkSpawn()
    {
        weapon.owner = OwnerClientId;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (IsOwner)
        { 
            rigidbody2D.velocity = playerInput.PlayerActions.Movement.ReadValue<Vector2>() * playerSpeed;
            var lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            weaponTransform.up = new Vector2(lookDirection.x - weaponTransform.position.x, lookDirection.y - weaponTransform.position.y).normalized;
            if (playerHealth.Value <= 0)
                NetworkManager.Singleton.Shutdown();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsServer)
        {
            switch (collision.gameObject.tag)
            {
                case "bullet":
                    if (OwnerClientId != collision.gameObject.GetComponent<BulletScript>().owner.Value)
                    {
                        playerHealth.Value -= collision.gameObject.GetComponent<BulletScript>().damage.Value;
                        collision.gameObject.GetComponent<BulletScript>().DestroyBulletRpc();
                    }
                    break;
                case "meleeHitBox":
                    if (OwnerClientId != collision.gameObject.GetComponent<MeleeHitboxScript>().owner.Value)
                    {
                        playerHealth.Value -= collision.gameObject.GetComponent<MeleeHitboxScript>().damage.Value;                    }
                    break;
            }
        }
    }
    private void StartSprint(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            isSprinting = true;
        }
    }
    private void StopSprint(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            isSprinting = false;
        }
    }
    private void StartAttack(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            weapon.StartAttack();
        }
    }
    private void FinishAttack(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            weapon.FinishAttack();
        }
    }
    private void Reload(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            weapon.Reload();
        }
    }
    private void AimDown(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            weapon.AimDown();
        }
    }
    public void StopActions()
    {
        playerSpeed = walkSpeed;
        isSprinting = false;
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            inventory.AddItemRpc("bandage");
        }
    }
}
