using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static PlayerController;

public class PlayerView : MonoBehaviour
{
    public PlayerController controller;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ChestScript chest = collision.transform.GetComponent<ChestScript>();
        DoorScript door = collision.transform.GetComponent<DoorScript>();
        SpikesScript spikes = collision.transform.GetComponent<SpikesScript>();
        WindScript wind = collision.transform.GetComponent<WindScript>();
        BeanCanScript beanCan = collision.transform.GetComponent<BeanCanScript>();
        if (chest != null && !chest.isOpened)
        {
            controller.getCoin?.Invoke();
            chest.CoinDrop();
        }
        if (door != null)
        {
            controller.toNextLevel?.Invoke(door.nextLevelDoor.position, door.coinAmountToOpen, door.levelToActivate, door.levelToDeactivate);
            controller.updateUI?.Invoke(door.uiCoinSprite);
        }
        if (spikes != null)
        { 
            controller.getSpiked?.Invoke(spikes.pushDirection, spikes.force);
        }
        if (wind != null)
        {
            controller.getWinded?.Invoke(collision.transform, wind.windForce);
        }
        if(beanCan != null)
        {
            controller.getBeaned?.Invoke(beanCan.beanForce);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "MovingPlatform")
        {
            transform.parent.SetParent(collision.transform, true);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "MovingPlatform")
        {
            transform.parent.parent = null;
        }
    }
}
