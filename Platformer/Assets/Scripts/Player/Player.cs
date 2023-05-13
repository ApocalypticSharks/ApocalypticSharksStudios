using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player: MonoBehaviour
{
    [SerializeField] private GameObject playerEntity;
    private GameObject playerCore;

    public Player(bool addScripts, Vector3 spawnPoint, Quaternion rotation)
    {
        playerEntity = Instantiate(Resources.Load("Gufik"), spawnPoint, rotation) as GameObject;
        playerCore = playerEntity.transform.GetChild(0).gameObject;
        if (addScripts)
        {
            EnableScripts();
        }
    }

    public void EnableScripts()
    {
        playerCore.GetComponent<Animator>().runtimeAnimatorController = Resources.Load("Animators/DrawCharacter") as RuntimeAnimatorController;
        playerCore.GetComponent<CharacterController2D>().enabled = true;
        playerCore.GetComponent<Attack>().enabled = true;
        playerCore.GetComponent<PlayerMovement>().enabled = true;
    }
}
