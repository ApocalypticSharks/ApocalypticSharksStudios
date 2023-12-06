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
    private Transform playerInstance;
    
    public PlayerController(Transform instance)
    {
        playerInstance = instance;
        getCoin += GetCoin;
        toNextLevel += MoveToNextLevel;
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
}
