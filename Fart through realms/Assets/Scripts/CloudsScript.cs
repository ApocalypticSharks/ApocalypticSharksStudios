using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudsScript : MonoBehaviour
{
    [SerializeField] private GameObject levelToActivate, levelToDeactivate;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        levelToActivate.SetActive(true);
        levelToDeactivate.SetActive(false);
    }
}
