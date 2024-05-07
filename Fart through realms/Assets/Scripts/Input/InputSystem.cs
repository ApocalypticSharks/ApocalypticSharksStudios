using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
    private Rigidbody2D rigidBody;
    public PlayerInputs playerInputs;
    [SerializeField] private float rotationSpeed, maxChargePower;
    [SerializeField]private float chargePower = 1, chargingSpeed;
    private bool isCharging;
    [SerializeField] private Transform chargingMeter, particles;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioClip fartSound;
    private void Awake()
    {
        playerInputs = new PlayerInputs();
        rigidBody = GetComponent<Rigidbody2D>();
        playerInputs.Player.Enable();
        playerInputs.Player.Jump.started += JumpCharge;
        playerInputs.Player.Jump.canceled += JumpRelease;
        playerInputs.Player.Jump.started += StartGame;
#if UNITY_EDITOR
        playerInputs.Player.Respawn.performed += Respawn;
#endif
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
        animator.SetTrigger("isCharging");
        transform.Find("FantasyCloud").gameObject.SetActive(false);
    }
    public void JumpRelease(InputAction.CallbackContext context) {
        chargingMeter.gameObject.SetActive(false);
        isCharging = false;
        int layerMask = 1 << 3;
        chargePower = chargePower < maxChargePower ? chargePower : maxChargePower;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up, 2f, layerMask);
        if (hit)
        {
            Instantiate(particles, hit.point, Quaternion.identity);
            rigidBody.AddForce(transform.up.normalized * chargePower, ForceMode2D.Impulse);
        }         
        chargePower = 1;
        SoundSystemScript.instance.PlaySoundFXClip(fartSound, transform, 1f);
        animator.SetTrigger("isFlying");
    }

    public void Respawn(InputAction.CallbackContext context)
    {
        var spawnPoint = GameObject.Find("SpawnPoint");
        transform.position = spawnPoint.transform.position;
    }
    private void ChargeMeter(Transform meter, float chargePower, float maxChargePower)
    {
        float chargePercent = 1/maxChargePower;
        meter.localScale = new Vector3(chargePercent*chargePower,1, 1);
    }

    public void StartGame(InputAction.CallbackContext context)
    {
        transform.Find("FantasyCloud").gameObject.SetActive(false);
        UnityEngine.Camera.main.GetComponent<Animator>().SetTrigger("gameStarted");
        playerInputs.Player.Jump.started -= StartGame;
        MusicSystemScript.instance.PlayLevelMusic();
    }
}
