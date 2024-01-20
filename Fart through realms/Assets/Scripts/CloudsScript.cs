using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudsScript : MonoBehaviour
{
    [SerializeField] private GameObject levelToActivate, levelToDeactivate;
    [SerializeField] private Sprite coinSprite;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        levelToActivate.SetActive(true);
        levelToDeactivate.SetActive(false);
        var player = collision.transform.GetComponent<PlayerView>();
        if (player != null)
            player.controller.updateUI?.Invoke(coinSprite);
    }
}
