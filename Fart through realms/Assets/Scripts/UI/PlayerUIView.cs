using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIView : MonoBehaviour
{
    public PlayerUIController controller;
    [SerializeField]private Text counter;

    public void UpdateCoinCounter(int collectedCoins)
    {
        counter.text = $"x {collectedCoins}";
    }
}
