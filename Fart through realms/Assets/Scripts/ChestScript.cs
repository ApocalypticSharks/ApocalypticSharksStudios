using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestScript : MonoBehaviour
{
    [SerializeField] Animator animator;
    private bool isOpened;
    [SerializeField] Object coin;
    public void CoinDrop(ref int collectedCoins)
    {
        if (!isOpened)
        {
            isOpened = true;
            animator.SetBool("isOpened", isOpened);
            collectedCoins++;
            Instantiate(coin, transform.position, transform.rotation);
        }
    }
}
