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
    private static bool _hasLastPlayerResolvedWinEffect;
    private static EffectType _lastPlayerResolvedWinEffectType;
    private static int _lastPlayerResolvedWinEffectAmount;

    public static void ResetPlayerWinEffectMemory()
    {
        _hasLastPlayerResolvedWinEffect = false;
        _lastPlayerResolvedWinEffectType = default;
        _lastPlayerResolvedWinEffectAmount = 0;
    }

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
        int total = amount;
        if (PlayerManager.Instance != null)
            total += PlayerManager.Instance.GreedPhysicalDamageBonusThisRound;
        dealer.TakeDamage(total, ignoreShield: false);
        PassiveUpgradeBonuses.OnPlayerDealtDamageToDealer(dealer, total);
        var poisonCtx = new EffectWinContext(sourceCard, playerWon: true, opponentDealer: dealer);
        ApplyBonusPoisonOnNonPoisonDamageDealer(in poisonCtx);
        PassiveUpgradeBonuses.ApplyShieldBashAfterPhysicalDamageToDealer(dealer);
    }

    public static void ProcessWinEffect(EffectStruct effect, in EffectWinContext ctx)
    {
        int amount = ctx.Card != null ? Mathf.Max(1, ctx.Card.baseValue) : Mathf.Max(1, effect.value);
        amount = ApplyComboMagnitudeBonus(amount, in ctx);

        switch (effect.type)
        {
            case EffectType.CardWinStrike:
                amount = ApplyPassiveStrikeAndMagicBonuses(EffectType.CardWinStrike, amount, in ctx);
                amount = ApplyStrikeCrit(amount, in ctx);
                if (ctx.PlayerWon && ctx.OpponentDealer != null)
                {
                    PlayerDealsPhysicalDamageToDealer(ctx.OpponentDealer, amount, ctx.Card);
                    RememberPlayerResolvedWinEffect(effect.type, amount);
                }
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
                    RememberPlayerResolvedWinEffect(effect.type, amount * shieldApplications);
                }
                else if (ctx.OpponentDealer != null)
                    ctx.OpponentDealer.AddShield(amount);
                break;
            case EffectType.CardWinHeal:
                if (ctx.PlayerWon)
                {
                    PlayerManager.Instance.HealDamage(amount);
                    RememberPlayerResolvedWinEffect(effect.type, amount);
                }
                else if (ctx.OpponentDealer != null)
                    ctx.OpponentDealer.HealDamage(amount);
                break;
            case EffectType.CardWinMagicStrike:
                amount = ApplyPassiveStrikeAndMagicBonuses(EffectType.CardWinMagicStrike, amount, in ctx);
                amount = ApplyStrikeCrit(amount, in ctx);
                if (ctx.PlayerWon && ctx.OpponentDealer != null)
                {
                    Dealer target = ctx.OpponentDealer;
                    target.TakeDamage(amount, ignoreShield: true);
                    PassiveUpgradeBonuses.OnPlayerDealtDamageToDealer(target, amount);
                    RememberPlayerResolvedWinEffect(effect.type, amount);
                    ApplyBonusPoisonOnNonPoisonDamageDealer(in ctx);

                    if (PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveMagicStrikeDamagesShieldHalf) > 0)
                    {
                        int toShield = Mathf.FloorToInt(amount / 2f);
                        if (toShield > 0)
                            target.DamageShieldOnly(toShield);
                    }

                    int magPoisonMult = PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveMagicStrikeBonusDamagePerPoisonStack);
                    if (magPoisonMult > 0 && target.poisonStacks > 0)
                    {
                        int extraMagic = target.poisonStacks * magPoisonMult;
                        target.TakeDamage(extraMagic, ignoreShield: true);
                        PassiveUpgradeBonuses.OnPlayerDealtDamageToDealer(target, extraMagic);
                    }

                    if (PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveMagicStrikeSplashesToOtherEnemies) > 0
                        && BattleManager.Instance?.dealers != null)
                    {
                        foreach (Dealer other in BattleManager.Instance.dealers)
                        {
                            if (other == null || other == target || other.dealerHealth <= 0)
                                continue;
                            PassiveUpgradeBonuses.NotifyMagicRadiationZeroHit(other);
                        }
                    }
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
                    RememberPlayerResolvedWinEffect(effect.type, amount);
                    int spread = PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassivePoisonSpreadToOtherEnemiesOnCardPoison);
                    if (spread > 0 && !ctx.SuppressPoisonFlaskSpread)
                        PoisonUpgradeEffects.ApplyPoisonSpreadToOtherDealers(ctx.OpponentDealer, spread);
                }
                else if (!ctx.PlayerWon)
                    PlayerManager.Instance.AddPoison(amount);
                break;
            case EffectType.CardWinCoin:
                if (ctx.PlayerWon && PlayerManager.Instance != null)
                {
                    PlayerManager.Instance.GetGold(amount);
                    RememberPlayerResolvedWinEffect(effect.type, amount);
                }
                break;
            case EffectType.CardWinIgnoreNextHarmfulEffect:
                if (!ctx.PlayerWon && ctx.OpponentDealer != null)
                    ctx.OpponentDealer.GrantIgnoreNextHarmfulEffect();
                break;
            case EffectType.CardWinLink:
                if (ctx.PlayerWon && ctx.OpponentDealer != null)
                {
                    if (ComboEngine.LastTransitionBridgedTypesFor(true))
                    {
                        PlayerDealsPhysicalDamageToDealer(ctx.OpponentDealer, amount, ctx.Card);
                        RememberPlayerResolvedWinEffect(EffectType.CardWinStrike, amount);
                    }
                    else
                    {
                        int shield = Mathf.Max(1, Mathf.FloorToInt(amount * 0.5f));
                        PlayerManager.Instance?.AddShield(shield);
                        RememberPlayerResolvedWinEffect(EffectType.CardWinShield, shield);
                    }
                }
                else if (!ctx.PlayerWon)
                {
                    if (ComboEngine.LastTransitionBridgedTypesFor(false))
                        PlayerManager.Instance.TakeDamage(amount, ignoreShield: false, attackingDealer: ctx.OpponentDealer);
                    else if (ctx.OpponentDealer != null)
                        ctx.OpponentDealer.AddShield(Mathf.Max(1, Mathf.FloorToInt(amount * 0.5f)));
                }
                break;
            case EffectType.CardWinEcho:
                if (!ctx.PlayerWon || !_hasLastPlayerResolvedWinEffect)
                    break;
                int echoedAmount = Mathf.Max(1, Mathf.CeilToInt(_lastPlayerResolvedWinEffectAmount * 0.5f));
                ApplyEchoedPlayerEffect(_lastPlayerResolvedWinEffectType, echoedAmount, in ctx);
                break;
        }
    }

    private static int ApplyComboMagnitudeBonus(int amount, in EffectWinContext ctx)
    {
        if (amount <= 0)
            return amount;
        int pct = ComboEngine.GetCurrentComboPercentBonus(ctx.PlayerWon);
        if (pct <= 0)
            return amount;
        return Mathf.Max(1, Mathf.CeilToInt(amount * (100f + pct) / 100f));
    }

    private static void RememberPlayerResolvedWinEffect(EffectType type, int amount)
    {
        if (amount <= 0)
            return;
        if (type == EffectType.CardWinEcho)
            return;
        _hasLastPlayerResolvedWinEffect = true;
        _lastPlayerResolvedWinEffectType = type;
        _lastPlayerResolvedWinEffectAmount = amount;
    }

    private static void ApplyEchoedPlayerEffect(EffectType sourceType, int amount, in EffectWinContext ctx)
    {
        if (amount <= 0)
            return;
        switch (sourceType)
        {
            case EffectType.CardWinStrike:
            case EffectType.CardWinMagicStrike:
            case EffectType.CardWinLink:
                if (ctx.OpponentDealer != null)
                {
                    PlayerDealsPhysicalDamageToDealer(ctx.OpponentDealer, amount, ctx.Card);
                    RememberPlayerResolvedWinEffect(EffectType.CardWinStrike, amount);
                }
                break;
            case EffectType.CardWinPoison:
                if (ctx.OpponentDealer != null)
                {
                    ctx.OpponentDealer.AddPoison(amount);
                    RememberPlayerResolvedWinEffect(EffectType.CardWinPoison, amount);
                }
                break;
            case EffectType.CardWinShield:
                PlayerManager.Instance?.AddShield(amount);
                RememberPlayerResolvedWinEffect(EffectType.CardWinShield, amount);
                break;
            case EffectType.CardWinHeal:
                PlayerManager.Instance?.HealDamage(amount);
                RememberPlayerResolvedWinEffect(EffectType.CardWinHeal, amount);
                break;
            case EffectType.CardWinCoin:
                PlayerManager.Instance?.GetGold(amount);
                RememberPlayerResolvedWinEffect(EffectType.CardWinCoin, amount);
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
    UpgradePassiveBattleStartShield,

    /// <summary>Player cards count this much lower per card for blackjack total; dealer cards this much higher (scaled per level).</summary>
    UpgradePassiveOverweight,
    /// <summary>Shop gold prices reduced by this many percent (stacking), final price rounded up, min 1 gold.</summary>
    UpgradePassiveShopDiscountPercent,
    /// <summary>Each coin-themed card drawn to your hand this round adds its base value to all physical damage until the round ends.</summary>
    UpgradePassiveGreedCoinCardPhysicalDamage,

    /// <summary>When your CardWinMagicStrike wins, floor(magicDamage/2) is also applied to the target shield only.</summary>
    UpgradePassiveMagicStrikeDamagesShieldHalf,
    /// <summary>When your CardWinMagicStrike wins, extra magic damage = target poison stacks times this passive sum (scaled per level).</summary>
    UpgradePassiveMagicStrikeBonusDamagePerPoisonStack,
    /// <summary>When your CardWinMagicStrike wins, each other living enemy receives a zero-damage &quot;hit&quot; that still runs on-hit procs (e.g. gold chance).</summary>
    UpgradePassiveMagicStrikeSplashesToOtherEnemies,

    /// <summary>Card repeats half of the last resolved player card win effect in this showdown.</summary>
    CardWinEcho,
    /// <summary>On win: if previous resolved card had a different type, deal damage; otherwise gain shield.</summary>
    CardWinLink,
    /// <summary>If last transition was Standard &lt;-&gt; Action, grants combo bonus percent.</summary>
    UpgradePassiveBridgeCapacitorComboPercent,
    /// <summary>Per suit transition streak step, grants additional combo bonus percent (capped globally).</summary>
    UpgradePassiveSuitResonatorComboPercentPerStep,
    /// <summary>When type-combo breaks, gain immediate safety shield (and diamond draw adds same gold).</summary>
    UpgradePassiveSafetyValveOnComboBreak,
    /// <summary>On win (enemy): ignore next incoming harmful effect (damage, poison, shield-only damage).</summary>
    CardWinIgnoreNextHarmfulEffect
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
