using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController
{
    private int collectedCoins = 0;
    public PlayerView view;
    public Action getCoin;
    
    public PlayerController()
    {
        getCoin += GetCoin;
    }
    private void GetCoin()
    {
        collectedCoins++;
    }
}
