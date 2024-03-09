using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindScript : MonoBehaviour
{
    [SerializeField] private BoxCollider2D collider; 
    [SerializeField]private float blowingCooldown;
    private float timer;
    public float windForce;
    [SerializeField] private ParticleSystem particleSystem;

    private void Awake()
    {
        timer = blowingCooldown;
        collider.enabled = false;
    }
    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0 && !collider.enabled)
        {
            makeBlow();
            Debug.Log("Start Blowing");
        }
    }

    private void makeBlow()
    {
        collider.enabled = true;
        particleSystem.Play();
    }

    private void OnParticleSystemStopped()
    {
        collider.enabled = false;
        timer = blowingCooldown;
        Debug.Log("Blow Ended");
    }

}
