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
    [Tooltip("Center icon for non-Standard cards. Standard playing cards use suit in the center instead.")]
    public Sprite actionSprite;
    public Sprite RankSprite;
    public Sprite RankSecondarySprite;
    public Sprite cardBackSprite;
    public int price;

    [Header("Card Properties")]
    public CardSuit suit;
    public CardRank rank;

    [Header("Game Values")]
    public int baseValue; //    
    public bool isAce = false;

    [Header("Card Type")]
    public CardType cardType = CardType.Standard;
    public Rarity rarity = Rarity.Common;

    [Header("Special Properties")]
    public bool isCursed = false;
    public bool isBlessed = false;
    public bool isConsumable = false; //   

    [Header("Effects")]
    public List<EffectStruct> onPlayEffects;
    public List<EffectStruct> onWinEffects;
    public List<EffectStruct> onLoseEffects;
    public List<EffectStruct> onDiscardEffects;

    [Header("Meta Info")]
    public int matchstickCost = 0; //     (0 =   1)
    public int shopCost = 10; //  

    /// <summary>  :   0  1.</summary>
    public int GetMatchstickBurnCost() => matchstickCost > 0 ? matchstickCost : 1;

    public bool HasPoisonWinEffect()
    {
        if (onWinEffects == null)
            return false;
        foreach (EffectStruct e in onWinEffects)
        {
            if (e.type == EffectType.CardWinPoison)
                return true;
        }
        return false;
    }

    public bool CountsAsCoinCardForGreed()
    {
        if (ListHasCoinEffect(onPlayEffects))
            return true;
        if (ListHasCoinEffect(onWinEffects))
            return true;
        return false;
    }

    private static bool ListHasCoinEffect(List<EffectStruct> list)
    {
        if (list == null)
            return false;
        foreach (EffectStruct e in list)
        {
            if (e.type == EffectType.CardWinCoin || e.type == EffectType.MoneyBag)
                return true;
        }
        return false;
    }

    //     
    public string GetValueText()
    {
        if (rank == CardRank.Ace) return "A";
        if (rank == CardRank.Jack) return "J";
        if (rank == CardRank.Queen) return "Q";
        if (rank == CardRank.King) return "K";
        return baseValue.ToString();
    }

    //  :  
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
public enum CardType
{
    Standard,
    Action,
}
public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
