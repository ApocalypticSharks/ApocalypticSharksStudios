using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int collectedCoins;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GetCoin(ref collectedCoins, collision.transform.GetComponent<ChestScript>());
    }

    private void GetCoin(ref int collectedCoins, ChestScript chest)
    {
        if(chest != null)
        {
            chest.CoinDrop(ref collectedCoins);
        }
    }
}
