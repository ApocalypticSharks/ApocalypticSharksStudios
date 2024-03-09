using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;
using static UnityEngine.InputManagerEntry;

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
    public delegate void winded(Transform wind, float windForce);
    public winded getWinded;
    public delegate void beaned(float beanForce);
    public beaned getBeaned;
    private Transform playerInstance;
    
    public PlayerController(Transform instance)
    {
        playerInstance = instance;
        getCoin += GetCoin;
        toNextLevel += MoveToNextLevel;
        getSpiked += GetSpiked;
        getWinded += GetWinded;
        getBeaned += GetBeaned;
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
        playerInstance.Find("body").GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        playerInstance.Find("body").GetComponent<Rigidbody2D>().AddForce(pushDirection * spikeFoce, ForceMode2D.Impulse);
    }

    private void GetWinded(Transform wind, float windForce)
    {
        playerInstance.Find("body").GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        playerInstance.Find("body").GetComponent<Rigidbody2D>().AddForce(wind.right * windForce, ForceMode2D.Impulse);
    }

    private void GetBeaned(float beanForce)
    {
        playerInstance.Find("body").GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        playerInstance.Find("body").GetComponent<ConstantForce2D>().relativeForce = new Vector3(0, beanForce, 0);
        playerInstance.Find("body").GetComponent<Rigidbody2D>().gravityScale = 0;
    }
}
