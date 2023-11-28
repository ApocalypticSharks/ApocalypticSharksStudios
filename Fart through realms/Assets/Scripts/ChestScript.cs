using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestScript : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public bool isOpened;
    [SerializeField] private Object coin;
    public void CoinDrop()
    {
        if (!isOpened)
        {
            isOpened = true;
            animator.SetBool("isOpened", isOpened);
            Instantiate(coin, transform.position, transform.rotation);
        }
    }
}
