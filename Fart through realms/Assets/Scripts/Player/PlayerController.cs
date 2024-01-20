using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;

public class PlayerController
{
    private int collectedCoins = 0;
    public PlayerView view;
    public Action getCoin;
    public delegate void moveToNextLevel(Vector3 doorPosition, int requiredCoinAmount, GameObject levelToActivate, GameObject levelToDeactivate);
    public moveToNextLevel toNextLevel;
    public delegate void updateLevelUI(Sprite coinSprite);
    public updateLevelUI updateUI;
    public delegate void spiked(Vector3 pushDirection, float spikeFoce);
    public spiked getSpiked;
    private Transform playerInstance;
    
    public PlayerController(Transform instance)
    {
        playerInstance = instance;
        getCoin += GetCoin;
        toNextLevel += MoveToNextLevel;
        getSpiked += GetSpiked;
    }
    private void GetCoin()
    {
        collectedCoins++;
    }
    private void MoveToNextLevel(Vector3 doorPosition, int requiredCoinAmount, GameObject leveltoActivate, GameObject levelToDeactivate)
    {
        if (collectedCoins >= requiredCoinAmount)
        {
            leveltoActivate.SetActive(true);
            playerInstance.Find("body").position = doorPosition;
            levelToDeactivate.SetActive(false);
        }
    }

    private void GetSpiked(Vector3 pushDirection, float spikeFoce)
    {
        playerInstance.Find("body").GetComponent<Rigidbody2D>().AddForce(pushDirection * spikeFoce, ForceMode2D.Impulse);
    }
}
