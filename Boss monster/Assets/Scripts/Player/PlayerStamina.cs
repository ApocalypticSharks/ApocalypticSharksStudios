using Unity.Netcode;
using UnityEngine;

public class PlayerStamina : NetworkBehaviour
{
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRecoverDelay = 4f;
    [SerializeField] private float sprintDrainRate = 20f;
    [SerializeField] private float recoverRate = 10f;
    [SerializeField] private PlayerScript playerScript;

    public NetworkVariable<float> Value = new NetworkVariable<float>(100f);
    public NetworkVariable<float> RecoverCooldown = new NetworkVariable<float>(0f);
    public float MaxValue => maxStamina;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Value.Value = maxStamina;
            RecoverCooldown.Value = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (IsServer)
            SimulateStamina();

        if (IsOwner)
            ApplyMovementSpeed();
    }

    private void SimulateStamina()
    {
        if (playerScript.IsDead)
            return;

        bool sprinting = playerScript.IsSprinting.Value
            && !playerScript.IsCarryingReliquary()
            && playerScript.rigidbody2D.linearVelocity.sqrMagnitude > 0.01f;

        if (sprinting)
        {
            if (Value.Value > 0f)
            {
                Value.Value = Mathf.Max(0f, Value.Value - sprintDrainRate * Time.fixedDeltaTime);
                RecoverCooldown.Value = staminaRecoverDelay;
            }
            else
            {
                playerScript.StopSprintFromStamina();
            }
        }
        else if (Value.Value < maxStamina)
        {
            if (RecoverCooldown.Value > 0f)
                RecoverCooldown.Value = Mathf.Max(0f, RecoverCooldown.Value - Time.fixedDeltaTime);
            else
                Value.Value = Mathf.Min(maxStamina, Value.Value + recoverRate * Time.fixedDeltaTime);
        }
    }

    private void ApplyMovementSpeed()
    {
        if (playerScript.IsDead)
            return;

        if (playerScript.IsCarryingReliquary())
        {
            playerScript.playerSpeed = playerScript.walkSpeed;
            if (IsOwner)
                playerScript.IsSprinting.Value = false;
            return;
        }

        if (playerScript.IsSprinting.Value && Value.Value > 0f)
            playerScript.playerSpeed = playerScript.sprintSpeed;
        else
            playerScript.playerSpeed = playerScript.walkSpeed;
    }
}
