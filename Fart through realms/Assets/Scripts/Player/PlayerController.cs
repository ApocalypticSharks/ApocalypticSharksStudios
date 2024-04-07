using System;
using UnityEngine;

public class PlayerController
{
    private int collectedCoins = 0;
    public bool isFlying;
    public PlayerView view;
    public Action getCoin;
    public delegate void moveToNextLevel(Vector3 doorPosition, int requiredCoinAmount, GameObject levelToActivate, GameObject levelToDeactivate, Sprite head, Sprite hand, Sprite arm, Sprite leg, Sprite foot, Sprite body, Sprite uiCoinSprite);
    public moveToNextLevel toNextLevel;
    public delegate void updateLevelUI(Sprite coinSprite);
    public updateLevelUI updateUI;
    public delegate void spiked(Vector3 pushDirection, float spikeFoce);
    public spiked getSpiked;
    public delegate void winded(Transform wind, float windForce);
    public winded getWinded;
    public delegate void beaned(params float[] forces);
    public beaned getBeaned;
    public delegate void unbeaned();
    public unbeaned getUnbeaned;
    private Transform playerInstance;
    
    public PlayerController(Transform instance)
    {
        playerInstance = instance;
        getCoin += GetCoin;
        toNextLevel += MoveToNextLevel;
        getSpiked += GetSpiked;
        getWinded += GetWinded;
        getBeaned += GetBeaned;
        getUnbeaned += GetUnbeaned;
    }
    private void GetCoin()
    {
        collectedCoins++;
    }
    private void MoveToNextLevel(Vector3 doorPosition, int requiredCoinAmount, GameObject leveltoActivate, GameObject levelToDeactivate, Sprite head, Sprite hand, Sprite arm, Sprite leg, Sprite foot, Sprite body, Sprite uiCoinSprite)
    {
        if (collectedCoins >= requiredCoinAmount)
        {
            updateUI?.Invoke(uiCoinSprite);
            leveltoActivate.SetActive(true);
            playerInstance.Find("body").position = doorPosition;
            levelToDeactivate.SetActive(false);
            playerInstance.Find("body").transform.Find("head").GetComponent<SpriteRenderer>().sprite = head;
            playerInstance.Find("body").transform.GetComponent<SpriteRenderer>().sprite = body;
            playerInstance.Find("body").transform.Find("armLeft").GetComponent<SpriteRenderer>().sprite = arm;
            playerInstance.Find("body").transform.Find("armLeft").GetChild(0).GetComponent<SpriteRenderer>().sprite = hand;
            playerInstance.Find("body").transform.Find("armRight").GetComponent<SpriteRenderer>().sprite = arm;
            playerInstance.Find("body").transform.Find("armRight").GetChild(0).GetComponent<SpriteRenderer>().sprite = hand;
            playerInstance.Find("body").transform.Find("legLeft").GetComponent<SpriteRenderer>().sprite = leg;
            playerInstance.Find("body").transform.Find("legLeft").GetChild(0).GetComponent<SpriteRenderer>().sprite = foot;
            playerInstance.Find("body").transform.Find("legRight").GetComponent<SpriteRenderer>().sprite = leg;
            playerInstance.Find("body").transform.Find("legRight").GetChild(0).GetComponent<SpriteRenderer>().sprite = foot;
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

    private void GetBeaned(params float[] forces)
    {
        foreach (var force in forces)
        {
            if (isFlying == false)
            {
                isFlying = true;
                playerInstance.Find("body").GetComponent<Rigidbody2D>().velocity = Vector3.zero;
                playerInstance.Find("body").GetComponent<ConstantForce2D>().relativeForce = new Vector3(0, force, 0);
                playerInstance.Find("body").GetComponent<Rigidbody2D>().gravityScale = 0;
                playerInstance.Find("body").GetComponent<Rigidbody2D>().drag = 1;
            }
        }        
    }

    private void GetUnbeaned()
    {
        if (isFlying == true)
        {
            isFlying = false;
            playerInstance.Find("body").GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            playerInstance.Find("body").GetComponent<ConstantForce2D>().relativeForce = Vector3.zero;
            playerInstance.Find("body").GetComponent<Rigidbody2D>().gravityScale = 1;
            playerInstance.Find("body").GetComponent<Rigidbody2D>().drag = 0;
        }
    }
}
