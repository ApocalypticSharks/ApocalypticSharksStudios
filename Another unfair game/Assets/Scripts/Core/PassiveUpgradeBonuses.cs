using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Passive modifiers stored in <see cref="UpgradeSO.onWinEffects"/> on owned upgrades.
/// </summary>
public static class PassiveUpgradeBonuses
{
    public static bool IsRoyalOrAce(CardSO card)
    {
        if (card == null)
            return false;
        return card.rank == CardRank.Jack
            || card.rank == CardRank.Queen
            || card.rank == CardRank.King
            || card.rank == CardRank.Ace;
    }

    public static int SumPassiveValue(EffectType passiveType)
    {
        int sum = 0;
        foreach (EffectStruct e in EnumeratePassiveEffects())
        {
            if (e.type == passiveType)
                sum += e.value;
        }
        return sum;
    }

    public static int GetTotalGoldIncomeBonusPercent()
    {
        return SumPassiveValue(EffectType.UpgradePassiveGoldIncomeMultiplier);
    }

    /// <summary>
    /// When the player deals damage to a dealer (hand value or card effects). Rolls Thief Hood.
    /// </summary>
    public static void OnPlayerDealtDamageToDealer(Dealer dealer, int damage)
    {
        if (damage <= 0 || dealer == null || GameStateManager.Instance == null)
            return;

        int chancePercent = SumPassiveValue(EffectType.UpgradePassiveGoldOnDamageChance);
        if (chancePercent <= 0)
            return;

        chancePercent = Mathf.Min(100, chancePercent);
        if (Random.Range(0, 100) < chancePercent)
            PlayerManager.Instance.GetGold(5);
    }

    /// <summary>After physical damage to a dealer (strikes, Overcharge, hand wins, play-phase damage, etc.).</summary>
    public static void ApplyShieldBashAfterPhysicalDamageToDealer(Dealer dealer)
    {
        if (dealer == null || dealer.dealerHealth <= 0 || PlayerManager.Instance == null)
            return;
        if (SumPassiveValue(EffectType.UpgradePassiveShieldBashOnPhysicalDamage) <= 0)
            return;
        int s = PlayerManager.Instance.shield;
        if (s <= 0)
            return;
        dealer.TakeDamage(s, ignoreShield: false);
        OnPlayerDealtDamageToDealer(dealer, s);
    }

    /// <summary>Player shield just dropped to 0 from blocking a hit.</summary>
    public static void OnPlayerShieldBrokenShieldShards()
    {
        if (SumPassiveValue(EffectType.UpgradePassiveShieldShardsWhenBroken) <= 0)
            return;
        if (BattleManager.Instance?.dealers == null)
            return;
        foreach (Dealer d in BattleManager.Instance.dealers)
        {
            if (d == null || d.dealerHealth <= 0)
                continue;
            int dmg = Mathf.Max(0, d.currentHandValue);
            if (dmg <= 0)
                continue;
            d.TakeDamage(dmg, ignoreShield: false);
            OnPlayerDealtDamageToDealer(d, dmg);
        }
    }

    private static IEnumerable<EffectStruct> EnumeratePassiveEffects()
    {
        List<UpgradeData> upgrades = GameStateManager.Instance != null
            ? GameStateManager.Instance.upgrades
            : null;
        if (upgrades == null)
            yield break;

        foreach (UpgradeData upgrade in upgrades)
        {
            if (upgrade == null || upgrade.data == null || upgrade.data.onWinEffects == null)
                continue;
            int lv = upgrade.EffectiveLevel;
            foreach (EffectStruct e in upgrade.data.onWinEffects)
            {
                EffectStruct scaled = e;
                scaled.value = UpgradeData.ScaledEffectValue(e.value, lv);
                yield return scaled;
            }
        }
    }
}
