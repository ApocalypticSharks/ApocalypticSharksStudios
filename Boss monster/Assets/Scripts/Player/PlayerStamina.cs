using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStamina : NetworkBehaviour
{
    public float Value, staminaRecoverCooldown, staminaRecoverDelay;
    [SerializeField] private PlayerScript playerScript;
    private void FixedUpdate()
    {
        if (playerScript.isSprinting && playerScript.rigidbody2D.velocity != Vector2.zero)
        {
            if (TryUseStamina(20))
            {
                playerScript.playerSpeed = playerScript.sprintSpeed;
                staminaRecoverCooldown += staminaRecoverDelay;
            }
        }
        else if (!playerScript.isSprinting)
        {
            playerScript.playerSpeed = playerScript.walkSpeed;
            Debug.Log("here");
            StaminaRecover();
        }

        Value = Mathf.Clamp(Value, 0, 100);
        staminaRecoverCooldown = Mathf.Clamp(staminaRecoverCooldown, 0, staminaRecoverDelay);
    }

    private bool TryUseStamina(float amount)
    {
        if (Value > 0)
        {
            Value -= amount * Time.deltaTime;
            return true;
        }
        playerScript.StopActions();
        return false;
    }
    private void StaminaRecover()
    {
        if (Value < 100)
        {
            if (staminaRecoverCooldown > 0)
            {
                Debug.Log("here1");
                staminaRecoverCooldown -= Time.deltaTime;
            }
            else
            {
                Value += 10 * Time.deltaTime;
            }
        }
    }
}
