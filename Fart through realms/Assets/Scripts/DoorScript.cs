using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [SerializeField] public GameObject levelToActivate;
    [SerializeField] public GameObject levelToDeactivate;
    [SerializeField] public Transform nextLevelDoor;
    [SerializeField] public int coinAmountToOpen;
    [SerializeField] public Sprite uiCoinSprite;
    [SerializeField] public Sprite head, hand, arm, leg, foot, body;
}
