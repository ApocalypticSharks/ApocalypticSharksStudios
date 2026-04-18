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
                    var targetDealer = BattleManager.Instance.dealers.First(dealer => dealer.dealerHealth > 0);
                    int dmg = PlayerManager.Instance.playerHand.Count * effect.value;
                    targetDealer.TakeDamage(dmg);
                    PassiveUpgradeBonuses.OnPlayerDealtDamageToDealer(targetDealer, dmg);
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
                    Dealer ov = BattleManager.Instance.dealers[randomEnemy];
                    ov.TakeDamage(1);
                    PassiveUpgradeBonuses.OnPlayerDealtDamageToDealer(ov, 1);
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
                amount = ApplyPassiveStrikeAndMagicBonuses(EffectType.CardWinStrike, amount, in ctx);
                amount = ApplyStrikeCrit(amount, in ctx);
                if (ctx.PlayerWon && ctx.OpponentDealer != null)
                {
                    ctx.OpponentDealer.TakeDamage(amount, ignoreShield: false);
                    PassiveUpgradeBonuses.OnPlayerDealtDamageToDealer(ctx.OpponentDealer, amount);
                }
                else if (!ctx.PlayerWon)
                    PlayerManager.Instance.TakeDamage(amount, ignoreShield: false);
                break;
            case EffectType.CardWinShield:
                amount = ApplyPassiveShieldBonus(amount, in ctx);
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
                amount = ApplyPassiveStrikeAndMagicBonuses(EffectType.CardWinMagicStrike, amount, in ctx);
                amount = ApplyStrikeCrit(amount, in ctx);
                if (ctx.PlayerWon && ctx.OpponentDealer != null)
                {
                    ctx.OpponentDealer.TakeDamage(amount, ignoreShield: true);
                    PassiveUpgradeBonuses.OnPlayerDealtDamageToDealer(ctx.OpponentDealer, amount);
                }
                else if (!ctx.PlayerWon)
                    PlayerManager.Instance.TakeDamage(amount, ignoreShield: true);
                break;
            case EffectType.CardWinPoison:
                amount = ApplyPassivePoisonBonuses(amount, in ctx);
                amount = ApplyPoisonCrit(amount, in ctx);
                if (ctx.PlayerWon && ctx.OpponentDealer != null)
                    ctx.OpponentDealer.AddPoison(amount);
                else if (!ctx.PlayerWon)
                    PlayerManager.Instance.AddPoison(amount);
                break;
        }
    }

    private static int ApplyPassiveStrikeAndMagicBonuses(EffectType winEffectType, int amount, in EffectWinContext ctx)
    {
        if (!ctx.PlayerWon || ctx.OpponentDealer == null || ctx.Card == null)
            return amount;

        if (winEffectType == EffectType.CardWinMagicStrike)
            amount += PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveMagicStrikeDamageBonus);

        if (winEffectType == EffectType.CardWinStrike)
        {
            bool royal = PassiveUpgradeBonuses.IsRoyalOrAce(ctx.Card);
            if (!royal)
                amount += PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveStrikeDamageBonusNonRoyal);
            else
                amount += PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveStrikeDamageBonusRoyal);
        }

        return amount;
    }

    private static int ApplyPassiveShieldBonus(int amount, in EffectWinContext ctx)
    {
        if (!ctx.PlayerWon)
            return amount;
        return amount + PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveShieldBonus);
    }

    private static int ApplyPassivePoisonBonuses(int amount, in EffectWinContext ctx)
    {
        if (!ctx.PlayerWon || ctx.OpponentDealer == null || ctx.Card == null)
            return amount;

        bool royal = PassiveUpgradeBonuses.IsRoyalOrAce(ctx.Card);
        if (!royal)
            amount += PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassivePoisonBonusNonRoyal);
        else
            amount += PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassivePoisonBonusRoyal);

        return amount;
    }

    private static int ApplyStrikeCrit(int amount, in EffectWinContext ctx)
    {
        if (!ctx.PlayerWon || ctx.OpponentDealer == null)
            return amount;

        int chance = PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveCritChance);
        chance = Mathf.Min(100, chance);
        if (chance > 0 && Random.Range(0, 100) < chance)
            return amount * 2;
        return amount;
    }

    private static int ApplyPoisonCrit(int amount, in EffectWinContext ctx)
    {
        if (!ctx.PlayerWon || ctx.OpponentDealer == null)
            return amount;

        int chance = PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassivePoisonCritChance);
        chance = Mathf.Min(100, chance);
        if (chance > 0 && Random.Range(0, 100) < chance)
            return amount * 2;
        return amount;
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
    CardWinPoison,

    UpgradePassiveMagicStrikeDamageBonus,
    UpgradePassiveStrikeDamageBonusNonRoyal,
    UpgradePassiveStrikeDamageBonusRoyal,
    UpgradePassiveShieldBonus,
    UpgradePassiveCritChance,
    UpgradePassivePoisonBonusNonRoyal,
    UpgradePassivePoisonBonusRoyal,
    UpgradePassivePoisonCritChance,
    /// <summary>value = chance percent to gain 5 gold when dealing damage to an enemy.</summary>
    UpgradePassiveGoldOnDamageChance,
    /// <summary>value = percent added to all gold income (multiple items stack).</summary>
    UpgradePassiveGoldIncomeMultiplier
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
