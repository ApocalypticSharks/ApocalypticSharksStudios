using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIController
{
    private int collectedCoins = 0;
    public PlayerUIView view;

    public PlayerUIController()
    {
    }
    public void CountCoins()
    {
        collectedCoins++;
        view.UpdateCoinCounter(collectedCoins);
    }
    public void SetCoinCounterImage(Sprite sprite)
    {
        view.SetCoinCounterImage(sprite);
    }
}
