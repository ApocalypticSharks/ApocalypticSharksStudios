using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
    private Rigidbody2D rigidBody;
    private PlayerInputs playerInputs;
    [SerializeField] private float rotationSpeed, maxChargePower;
    [SerializeField]private float chargePower = 1, chargingSpeed;
    private bool isCharging;
    [SerializeField] private Transform chargingMeter;
    private void Awake()
    {
        playerInputs = new PlayerInputs();
        rigidBody = GetComponent<Rigidbody2D>();
        playerInputs.Player.Enable();
        playerInputs.Player.Jump.started += JumpCharge;
        playerInputs.Player.Jump.canceled += JumpRelease;
    }
    private void FixedUpdate()
    {
        float rotationDirection = playerInputs.Player.Rotate.ReadValue<float>();
        transform.Rotate(new Vector3(0,0,-5) * rotationDirection);
        if (isCharging && chargePower < maxChargePower)
            chargePower += chargingSpeed * Time.deltaTime;
        ChargeMeter(chargingMeter.GetChild(0), chargePower, maxChargePower);
    }
    public void JumpCharge(InputAction.CallbackContext context)
    {
        chargingMeter.gameObject.SetActive(true);
        isCharging = true;
    }
    public void JumpRelease(InputAction.CallbackContext context) {
        chargingMeter.gameObject.SetActive(false);
        isCharging = false;
        int layerMask = 1 << 3;
        chargePower = chargePower < maxChargePower ? chargePower : maxChargePower;
        if (Physics2D.Raycast(transform.position, -transform.up, 2f, layerMask))
        {
            rigidBody.AddForce(transform.up.normalized * chargePower, ForceMode2D.Impulse);
        }         
        chargePower = 1;
    }
    private void ChargeMeter(Transform meter, float chargePower, float maxChargePower)
    {
        float chargePercent = 1/maxChargePower;
        meter.localScale = new Vector3(chargePercent*chargePower,1, 1);
    }
}
