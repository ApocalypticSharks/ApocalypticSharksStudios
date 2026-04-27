using UnityEngine;

public class UpgradeData : MonoBehaviour
{
    public UpgradeSO data;
    /// <summary>In inventory: current level (1–<see cref="UpgradeSO.MaxLevel"/>). In shop: level after purchase.</summary>
    public int level;
    public bool isInInventory = false;

    public int EffectiveLevel =>
        Mathf.Clamp(level < 1 ? 1 : level, 1, UpgradeSO.MaxLevel);

    public static int ScaledEffectValue(int baseValue, int level)
    {
        int L = Mathf.Clamp(level < 1 ? 1 : level, 1, UpgradeSO.MaxLevel);
        return baseValue * L;
    }

    /// <summary>Gold to buy the upgrade at <paramref name="targetLevel"/> (effect values in asset are per level 1).</summary>
    public static int GetBuyPrice(UpgradeSO so, int targetLevel)
    {
        if (so == null)
            return 0;
        int L = Mathf.Clamp(targetLevel, 1, UpgradeSO.MaxLevel);
        int raw = Mathf.Max(1, so.Cost * L);
        return PassiveUpgradeBonuses.GetShopGoldPriceAfterDiscount(raw);
    }

    public static int GetSellPrice(UpgradeSO so, int currentLevel)
    {
        if (so == null)
            return 0;
        int L = Mathf.Clamp(currentLevel < 1 ? 1 : currentLevel, 1, UpgradeSO.MaxLevel);
        return Mathf.Max(0, so.Cost * L / 2);
    }

    public static int GetOwnedUpgradeLevel(UpgradeSO so)
    {
        if (so == null || GameStateManager.Instance?.upgrades == null)
            return 0;
        foreach (UpgradeData u in GameStateManager.Instance.upgrades)
        {
            if (u != null && u.data == so && u.isInInventory)
                return u.EffectiveLevel;
        }
        return 0;
    }

    public static UpgradeData FindOwnedUpgrade(UpgradeSO so)
    {
        if (so == null || GameStateManager.Instance?.upgrades == null)
            return null;
        foreach (UpgradeData u in GameStateManager.Instance.upgrades)
        {
            if (u != null && u.data == so && u.isInInventory)
                return u;
        }
        return null;
    }
}
