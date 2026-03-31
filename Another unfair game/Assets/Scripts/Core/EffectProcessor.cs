using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public static class EffectProcessor
{
    public static void ProcessEffect(EffectStruct effect, GameState phase)
    {
        Debug.Log($"Processing effect: {effect.type} with value {effect.value}");
        var topCard = PlayerManager.Instance.playerHand[PlayerManager.Instance.playerHand.Count - 1];
        switch (effect.type)
        {
            case EffectType.DealDamageBasedOnHandCount:
                if (phase == GameState.BattlePlayerTurn)
                {
                    BattleManager.Instance.dealers.First(dealer => dealer.dealerHealth > 0).TakeDamage(PlayerManager.Instance.playerHand.Count * effect.value);
                }
                else if (phase == GameState.BattleEnemyTurn)
                {
                    PlayerManager.Instance.playerHealth -= BattleManager.Instance.activeDealer.GetDealerHand().Count * effect.value;
                }
                break;
            //If bust, deal excess hand value as damage randomly allocated between enemies
            case EffectType.Overcharge:               
                var handValue = PlayerManager.Instance.CalculateHandValue();

                for (int i = 0; i < 21 - handValue; i++)
                {
                    var randomEnemy = Random.Range(0, BattleManager.Instance.dealers.Count);
                    BattleManager.Instance.dealers[randomEnemy].TakeDamage(1);
                }
                break;               
            //When draw "Hearts" card, Heals to the value of the card
            case EffectType.HealingHeart:
                if (topCard.gameObject.GetComponent<CardData>().data.suit == CardSuit.Hearts)
                {
                    PlayerManager.Instance.HealDamage(topCard.gameObject.GetComponent<CardData>().data.baseValue);
                }
                break;
            //When card drawn, grants amount of gold equal the card value
            case EffectType.MoneyBag:
                if (topCard.gameObject.GetComponent<CardData>().data.suit == CardSuit.Diamonds)
                {
                    PlayerManager.Instance.GetGold(topCard.gameObject.GetComponent<CardData>().data.baseValue);
                }
                break;

            //case EffectType.LoseCredits:
            //    GameManager.Instance.SpendCredits(effect.value);
            //    break;

            //case EffectType.HealPlayer:
            //    // Пока нет здоровья у игрока, можно добавить
            //    break;

            //case EffectType.DrawCard:
            //    GameManager.Instance.deckManager.PlayerDrawCard();
            //    break;

            //case EffectType.DiscardCard:
            //    // Сброс случайной карты
            //    var hand = GameManager.Instance.deckManager.GetPlayerHand();
            //    if (hand.Count > 0)
            //    {
            //        GameManager.Instance.deckManager.DiscardCard(
            //            hand[Random.Range(0, hand.Count)],
            //            hand
            //        );
            //    }
            //    break;

            //case EffectType.GainMatchstick:
            //    GameManager.Instance.AddMatchsticks(effect.value);
            //    break;

            //case EffectType.LoseMatchstick:
            //        GameManager.Instance.SpendMatchstick(effect.value);
            //    break;

            //case EffectType.ModifyBet:
            //    GameManager.Instance.currentBet = Mathf.Max(1,
            //        GameManager.Instance.currentBet + effect.value);
            //    break;
        }
    }
}
public enum EffectType
{
    Overcharge, DealDamageBasedOnHandCount, HealingHeart, MoneyBag
}

[System.Serializable]
public struct EffectStruct
{
    public EffectType type;
    public int value;
    public string description;

    // Метод применения эффекта
    public void ApplyEffect(GameState phase)
    {
        EffectProcessor.ProcessEffect(this, phase);
    }
}
