using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "New Card", menuName = "Blackjack Rogue/Card")]
public class CardSO : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName;
    [TextArea(2, 3)]
    public string description;
    public Sprite cardSprite;
    public Sprite suitSprite;
    public Sprite RankSprite;
    public Sprite RankSecondarySprite;
    public Sprite cardBackSprite;
    public int price;

    [Header("Card Properties")]
    public CardSuit suit;
    public CardRank rank;

    [Header("Game Values")]
    public int baseValue; // Основное значение для блекджека
    public bool isAce = false;

    [Header("Card Type")]
    public CardType cardType = CardType.Standard;
    public Rarity rarity = Rarity.Common;

    [Header("Special Properties")]
    public bool isCursed = false;
    public bool isBlessed = false;
    public bool isConsumable = false; // Сгорает после использования

    [Header("Effects")]
    public List<EffectStruct> onPlayEffects;
    public List<EffectStruct> onWinEffects;
    public List<EffectStruct> onLoseEffects;
    public List<EffectStruct> onDiscardEffects;

    [Header("Meta Info")]
    public int matchstickCost = 0; // Стоимость в спичках для сжигания
    public int shopCost = 10; // Стоимость в магазине

    // Быстрое получение значения для отображения
    public string GetValueText()
    {
        if (rank == CardRank.Ace) return "A";
        if (rank == CardRank.Jack) return "J";
        if (rank == CardRank.Queen) return "Q";
        if (rank == CardRank.King) return "K";
        return baseValue.ToString();
    }

    // Для туза: выбор значения
    public int GetValue(bool useHighAce = true)
    {
        if (isAce) return useHighAce ? 11 : 1;
        return baseValue;
    }
}

// Enums
public enum CardSuit { Hearts, Diamonds, Clubs, Spades, Special }
public enum CardRank
{
    Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
    Jack, Queen, King, Ace, Joker
}
public enum CardType { Standard, Curse, Blessing, Gambit, Relic }
public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }