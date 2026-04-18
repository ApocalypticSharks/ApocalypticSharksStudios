using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ShopVisitMode
{
    All,
    UpgradesOnly,
    CardsOnly
}

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("Upgrades")]
    public List<UpgradeSO> updgrades;
    public GameObject upgradePrefab;

    [Header("Cards & booster")]
    public List<CardSO> shopCardPool;
    public GameObject shopCardOfferPrefab;
    public GameObject boosterPrefab;
    public int boosterGoldCost = 40;

    [Header("Layout")]
    public Transform shopContainer;
    public Transform upgradeShopContainer;
    public Transform cardShopContainer;

    public List<GameObject> itemsForSale = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private Transform UpgradeParent => upgradeShopContainer != null ? upgradeShopContainer : shopContainer;
    private Transform CardParent => cardShopContainer != null ? cardShopContainer : shopContainer;

    public void DeinitializeShop()
    {
        foreach (GameObject item in itemsForSale)
        {
            if (item != null)
                Destroy(item);
        }
        itemsForSale.Clear();
    }

    public void PrepareNewShopItems()
    {
        ShopVisitMode mode = Random.Range(0, 2) == 0
            ? ShopVisitMode.UpgradesOnly
            : ShopVisitMode.CardsOnly;

        DeinitializeShop();

        if (mode != ShopVisitMode.CardsOnly)
            PrepareUpgradeOffers(5);
        if (mode != ShopVisitMode.UpgradesOnly)
        {
            PrepareCardOffers(2);
            PrepareBooster();
        }
    }

    private void PrepareUpgradeOffers(int count)
    {
        if (upgradePrefab == null || UpgradeParent == null || updgrades == null || updgrades.Count == 0)
            return;

        var remaining = updgrades.Where(u => u != null).Distinct().ToList();
        for (int i = 0; i < count && remaining.Count > 0; i++)
        {
            UpgradeSO picked = PickWeightedUnique(remaining, u => Mathf.Max(1, u.Rarity));
            if (picked == null)
                break;

            GameObject item = Instantiate(upgradePrefab, UpgradeParent);
            item.GetComponent<UpgradeData>().data = picked;
            itemsForSale.Add(item);
        }
    }

    private void PrepareCardOffers(int count)
    {
        if (shopCardOfferPrefab == null || CardParent == null || shopCardPool == null || shopCardPool.Count == 0)
            return;

        var remaining = shopCardPool.Where(c => c != null).Distinct().ToList();
        for (int i = 0; i < count && remaining.Count > 0; i++)
        {
            CardSO picked = PickWeightedUnique(remaining, c => CardRarityWeight(c.rarity));
            if (picked == null)
                break;

            GameObject item = Instantiate(shopCardOfferPrefab, CardParent);
            var offer = item.GetComponent<ShopCardOfferData>();
            if (offer != null)
                offer.card = picked;

            var cardVisual = item.GetComponent<CardData>();
            if (cardVisual != null)
            {
                cardVisual.data = picked;
                cardVisual.Initialize();
            }
            else
            {
                var childCard = item.GetComponentInChildren<CardData>();
                if (childCard != null)
                {
                    childCard.data = picked;
                    childCard.Initialize();
                }
            }

            itemsForSale.Add(item);
        }
    }

    private void PrepareBooster()
    {
        if (boosterPrefab == null || CardParent == null || shopCardPool == null || shopCardPool.Count == 0)
            return;

        GameObject item = Instantiate(boosterPrefab, CardParent);
        itemsForSale.Add(item);
    }

    public IReadOnlyList<CardSO> PullBoosterCards(int count)
    {
        var result = new List<CardSO>(count);
        var pool = shopCardPool.Where(c => c != null).ToList();
        if (pool.Count == 0)
            return result;

        for (int i = 0; i < count; i++)
            result.Add(PickWeightedWithReplacement(pool, c => CardRarityWeight(c.rarity)));

        return result;
    }

    private static int CardRarityWeight(Rarity r)
    {
        return r switch
        {
            Rarity.Common => 10,
            Rarity.Uncommon => 6,
            Rarity.Rare => 4,
            Rarity.Epic => 2,
            Rarity.Legendary => 1,
            _ => 5
        };
    }

    private static UpgradeSO PickWeightedUnique(List<UpgradeSO> remaining, System.Func<UpgradeSO, int> weightFn)
    {
        UpgradeSO picked = PickWeightedWithReplacement(remaining, weightFn);
        if (picked != null)
            remaining.RemoveAll(x => x == picked);
        return picked;
    }

    private static CardSO PickWeightedUnique(List<CardSO> remaining, System.Func<CardSO, int> weightFn)
    {
        CardSO picked = PickWeightedWithReplacement(remaining, weightFn);
        if (picked != null)
            remaining.RemoveAll(x => x == picked);
        return picked;
    }

    private static T PickWeightedWithReplacement<T>(IReadOnlyList<T> items, System.Func<T, int> weightFn) where T : class
    {
        if (items == null || items.Count == 0)
            return null;

        int total = 0;
        foreach (T item in items)
            total += Mathf.Max(1, weightFn(item));

        if (total <= 0)
            return items[Random.Range(0, items.Count)];

        int roll = Random.Range(0, total);
        foreach (T item in items)
        {
            roll -= Mathf.Max(1, weightFn(item));
            if (roll < 0)
                return item;
        }

        return items[items.Count - 1];
    }
}
