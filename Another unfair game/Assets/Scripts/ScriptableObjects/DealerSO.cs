using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New dealer", menuName = "Blackjack Rogue/Dealer")]
public class DealerSO : ScriptableObject
{
    [Header("Dealer Info")]
    public string dealerName;
    [TextArea(2, 3)]
    public string description;
    public Sprite sprite;

    [Header("Dealer Stats")]
    public int dealerHealth;
    public float damageModifier;
    public int might;

    [Header("Dealer Deck")]
    public List<CardSO> dealerDeck;
}