using System.Linq;
using UnityEngine;

public static class PoisonUpgradeEffects
{
    public static void ApplyPoisonSpreadToOtherDealers(Dealer primaryTarget, int stacksPerOtherEnemy)
    {
        if (stacksPerOtherEnemy <= 0 || primaryTarget == null || BattleManager.Instance?.dealers == null)
            return;
        foreach (Dealer d in BattleManager.Instance.dealers)
        {
            if (d == null || d.dealerHealth <= 0 || d == primaryTarget)
                continue;
            d.AddPoison(stacksPerOtherEnemy);
        }
    }
}

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
                    CardSO src = topCard.GetComponent<CardData>()?.data;
                    PlayerDealsPhysicalDamageToDealer(targetDealer, dmg, src);
                }
                else if (phase == GameState.BattleEnemyTurn)
                {
                    PlayerManager.Instance.TakeDamage(
                        BattleManager.Instance.activeDealer.GetDealerHand().Count * effect.value,
                        ignoreShield: false,
                        attackingDealer: BattleManager.Instance.activeDealer);
                }
                break;
            case EffectType.Overcharge:
                var handValue = PlayerManager.Instance.CalculateHandValue();
                CardSO overSrc = topCard.GetComponent<CardData>()?.data;

                for (int i = 0; i < 21 - handValue; i++)
                {
                    var randomEnemy = Random.Range(0, BattleManager.Instance.dealers.Count);
                    Dealer ov = BattleManager.Instance.dealers[randomEnemy];
                    PlayerDealsPhysicalDamageToDealer(ov, 1, overSrc);
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

    /// <summary>
    /// Single entry for player-dealt physical damage: dealer HP, gold-on-hit, dirty-gear poison, shield bash.
    /// Use for strikes, Overcharge ticks, hand-value wins, and play-phase damage cards.
    /// </summary>
    public static void PlayerDealsPhysicalDamageToDealer(Dealer dealer, int amount, CardSO sourceCard = null)
    {
        if (dealer == null || amount <= 0 || dealer.dealerHealth <= 0)
            return;
        dealer.TakeDamage(amount, ignoreShield: false);
        PassiveUpgradeBonuses.OnPlayerDealtDamageToDealer(dealer, amount);
        var poisonCtx = new EffectWinContext(sourceCard, playerWon: true, opponentDealer: dealer);
        ApplyBonusPoisonOnNonPoisonDamageDealer(in poisonCtx);
        PassiveUpgradeBonuses.ApplyShieldBashAfterPhysicalDamageToDealer(dealer);
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
                    PlayerDealsPhysicalDamageToDealer(ctx.OpponentDealer, amount, ctx.Card);
                else if (!ctx.PlayerWon)
                    PlayerManager.Instance.TakeDamage(amount, ignoreShield: false, attackingDealer: ctx.OpponentDealer);
                break;
            case EffectType.CardWinShield:
                amount = ApplyPassiveShieldBonus(amount, in ctx);
                if (ctx.PlayerWon)
                {
                    int shieldApplications = 1;
                    int doubleShieldChance = Mathf.Min(100, PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveShieldDoubleBlockChance));
                    if (doubleShieldChance > 0 && Random.Range(0, 100) < doubleShieldChance)
                        shieldApplications = 2;
                    for (int s = 0; s < shieldApplications; s++)
                        PlayerManager.Instance.AddShield(amount);
                }
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
                    ApplyBonusPoisonOnNonPoisonDamageDealer(in ctx);
                }
                else if (!ctx.PlayerWon)
                    PlayerManager.Instance.TakeDamage(amount, ignoreShield: true, attackingDealer: ctx.OpponentDealer);
                break;
            case EffectType.CardWinPoison:
                amount = ApplyPassivePoisonBonuses(amount, in ctx);
                amount = ApplyPoisonCrit(amount, in ctx);
                if (ctx.PlayerWon && ctx.OpponentDealer != null)
                {
                    ctx.OpponentDealer.AddPoison(amount);
                    int spread = PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassivePoisonSpreadToOtherEnemiesOnCardPoison);
                    if (spread > 0 && !ctx.SuppressPoisonFlaskSpread)
                        PoisonUpgradeEffects.ApplyPoisonSpreadToOtherDealers(ctx.OpponentDealer, spread);
                }
                else if (!ctx.PlayerWon)
                    PlayerManager.Instance.AddPoison(amount);
                break;
            case EffectType.CardWinCoin:
                if (ctx.PlayerWon && PlayerManager.Instance != null)
                    PlayerManager.Instance.GetGold(amount);
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

    private static void ApplyBonusPoisonOnNonPoisonDamageDealer(in EffectWinContext ctx)
    {
        if (!ctx.PlayerWon || ctx.OpponentDealer == null)
            return;
        int bonus = PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveNonPoisonDamageAddsPoison);
        if (bonus > 0)
            ctx.OpponentDealer.AddPoison(bonus);
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
    UpgradePassiveGoldIncomeMultiplier,

    /// <summary>When your winning card applies CardWinPoison to a dealer, each other living dealer gains this many poison stacks (scaled per upgrade level).</summary>
    UpgradePassivePoisonSpreadToOtherEnemiesOnCardPoison,
    /// <summary>Extra full poison tick waves at round start (total waves = max(1, 1 + sum of scaled values)).</summary>
    UpgradePassiveExtraPoisonTicksPerRound,
    /// <summary>When you burn a card with a poison win effect (matchsticks), each living dealer gains this many poison stacks.</summary>
    UpgradePassivePoisonAllEnemiesOnPoisonCardBurn,
    /// <summary>When a dealer draws from the shared deck and the card has CardWinPoison, that poison applies to them as if they lost the showdown.</summary>
    UpgradePassiveOpponentDrawPoisonCardAppliesPoison,
    /// <summary>CardWinStrike / CardWinMagicStrike also add this many poison stacks when you win the hand.</summary>
    UpgradePassiveNonPoisonDamageAddsPoison,

    /// <summary>On winning the showdown, gain gold equal to this card value (subject to passive gold income bonuses).</summary>
    CardWinCoin,

    /// <summary>When you take damage from a dealer hit, that dealer takes this much physical damage (scaled per level).</summary>
    UpgradePassiveReflectDamageWhenHit,
    /// <summary>After any player physical damage to a dealer (strike, Overcharge tick, hand win, play-phase damage), deal extra physical damage equal to your current shield (owned if sum &gt; 0).</summary>
    UpgradePassiveShieldBashOnPhysicalDamage,
    /// <summary>Percent chance (max 100) to gain shield twice from your winning <see cref="CardWinShield"/> card effect.</summary>
    UpgradePassiveShieldDoubleBlockChance,
    /// <summary>When your shield is depleted to 0 by a blockable hit, each living enemy takes physical damage equal to their current hand value (gate if sum &gt; 0).</summary>
    UpgradePassiveShieldShardsWhenBroken,
    /// <summary>Shield gained at the start of each battle (scaled).</summary>
    UpgradePassiveBattleStartShield
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
