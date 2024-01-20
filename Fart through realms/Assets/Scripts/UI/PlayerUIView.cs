using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIView : MonoBehaviour
{
    public PlayerUIController controller;
    [SerializeField]private Text counter;
    [SerializeField] private Image coinSprite;

    public void UpdateCoinCounter(int collectedCoins)
    {
        counter.text = $"x {collectedCoins}";
    }

    public void SetCoinCounterImage(Sprite sprite)
    {
        coinSprite.sprite = sprite;
    }
}
