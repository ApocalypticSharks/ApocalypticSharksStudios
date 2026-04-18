using System.Linq;
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
                    PlayerManager.Instance.TakeDamage(BattleManager.Instance.activeDealer.GetDealerHand().Count * effect.value);
                }
                break;
            case EffectType.Overcharge:
                var handValue = PlayerManager.Instance.CalculateHandValue();

                for (int i = 0; i < 21 - handValue; i++)
                {
                    var randomEnemy = Random.Range(0, BattleManager.Instance.dealers.Count);
                    BattleManager.Instance.dealers[randomEnemy].TakeDamage(1);
                }
                break;
            case EffectType.HealingHeart:
                if (topCard.gameObject.GetComponent<CardData>().data.suit == CardSuit.Hearts)
                {
                    PlayerManager.Instance.HealDamage(topCard.gameObject.GetComponent<CardData>().data.baseValue);
                }
                break;
            case EffectType.MoneyBag:
                if (topCard.gameObject.GetComponent<CardData>().data.suit == CardSuit.Diamonds)
                {
                    PlayerManager.Instance.GetGold(topCard.gameObject.GetComponent<CardData>().data.baseValue);
                }
                break;
        }
    }

    public static void ProcessWinEffect(EffectStruct effect, in EffectWinContext ctx)
    {
        int amount = ctx.Card != null ? Mathf.Max(1, ctx.Card.baseValue) : Mathf.Max(1, effect.value);

        switch (effect.type)
        {
            case EffectType.CardWinStrike:
                if (ctx.PlayerWon && ctx.OpponentDealer != null)
                    ctx.OpponentDealer.TakeDamage(amount, ignoreShield: false);
                else if (!ctx.PlayerWon)
                    PlayerManager.Instance.TakeDamage(amount, ignoreShield: false);
                break;
            case EffectType.CardWinShield:
                if (ctx.PlayerWon)
                    PlayerManager.Instance.AddShield(amount);
                else if (ctx.OpponentDealer != null)
                    ctx.OpponentDealer.AddShield(amount);
                break;
            case EffectType.CardWinHeal:
                if (ctx.PlayerWon)
                    PlayerManager.Instance.HealDamage(amount);
                else if (ctx.OpponentDealer != null)
                    ctx.OpponentDealer.HealDamage(amount);
                break;
            case EffectType.CardWinMagicStrike:
                if (ctx.PlayerWon && ctx.OpponentDealer != null)
                    ctx.OpponentDealer.TakeDamage(amount, ignoreShield: true);
                else if (!ctx.PlayerWon)
                    PlayerManager.Instance.TakeDamage(amount, ignoreShield: true);
                break;
            case EffectType.CardWinPoison:
                if (ctx.PlayerWon && ctx.OpponentDealer != null)
                    ctx.OpponentDealer.AddPoison(amount);
                else if (!ctx.PlayerWon)
                    PlayerManager.Instance.AddPoison(amount);
                break;
        }
    }
}

public enum EffectType
{
    Overcharge,
    DealDamageBasedOnHandCount,
    HealingHeart,
    MoneyBag,
    CardWinStrike,
    CardWinShield,
    CardWinHeal,
    CardWinMagicStrike,
    CardWinPoison
}

[System.Serializable]
public struct EffectStruct
{
    public EffectType type;
    public int value;
    public string description;

    public void ApplyEffect(GameState phase)
    {
        EffectProcessor.ProcessEffect(this, phase);
    }

    public void ApplyWinEffect(in EffectWinContext ctx)
    {
        EffectProcessor.ProcessWinEffect(this, in ctx);
    }
}
