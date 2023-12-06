using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    public PlayerController controller;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ChestScript chest = collision.transform.GetComponent<ChestScript>();
        DoorScript door = collision.transform.GetComponent<DoorScript>();
        if (chest != null && !chest.isOpened)
        {
            controller.getCoin?.Invoke();
            chest.CoinDrop();
        }
        if (door != null)
        {
            controller.toNextLevel?.Invoke(door.nextLevelDoor.position, door.coinAmountToOpen, door.levelToActivate, door.levelToDeactivate);
        }

    }
}
