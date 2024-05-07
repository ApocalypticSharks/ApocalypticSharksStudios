using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using static PlayerController;

public class PlayerView : MonoBehaviour
{
    public PlayerController controller;
    public GameObject particles;

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
            controller.toNextLevel?.Invoke(door.LevelMusic, door.nextLevelDoor.position, door.coinAmountToOpen, door.levelToActivate, door.levelToDeactivate, door.head, door.hand, door.arm, door.leg, door.foot, door.body, door.uiCoinSprite);
        }
        if (spikes != null)
        { 
            controller.getSpiked?.Invoke(spikes.pushDirection, spikes.force);
        }
        if (wind != null)
        {
            controller.getWinded?.Invoke(collision.transform, wind.windForce);
        }
        if(beanCan != null && !controller.isFlying)
        {
            particles.SetActive(true);
            controller.getBeaned?.Invoke(beanCan.beanForce);
            transform.Find("head").GetComponent<SpriteRenderer>().sprite = beanCan.head;
        }
        if (controller.isFlying && beanCan == null)
        {
            particles.SetActive(false);
            controller.getUnbeaned?.Invoke();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (controller.isFlying)
        {
            particles.SetActive(false);
            controller.getUnbeaned?.Invoke();
        }

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
