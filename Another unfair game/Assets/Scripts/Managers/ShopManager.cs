using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    public List<UpgradeSO> updgrades;
    public List<EquipmentSO> equipments;
    public Transform shopContainer;
    public GameObject upgradePrefab;
    public GameObject equipmentPrefab;
    public List<GameObject> itemsForSale;

    private List<UpgradeSO> _upgradesWithRarity = new List<UpgradeSO>();
    private List<EquipmentSO> _equipmentsWithRarity = new List<EquipmentSO>();
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
        InitializeShop();
    }
    public void InitializeShop()
    {
        foreach (UpgradeSO upgrade in updgrades)
        {
            for (int i = 0; i < upgrade.Rarity; i++)
            {
                _upgradesWithRarity.Add(upgrade);
            }
        }
        foreach (EquipmentSO equipment in equipments)
        {
            for (int i = 0; i < equipment.Rarity; i++)
            {
                _equipmentsWithRarity.Add(equipment);
            }
        }
    }
    public void DeinitializeShop()
    {
        while (itemsForSale.Any())
        {
            Destroy(itemsForSale[0].gameObject);
        }
    }
    public void PrepareNewShopItems()
    {
        for (int i = 0; i < 3; i++)
        {
            PrepareUpgrade();
            PrepareEquipment();
        }
    }

    private void PrepareUpgrade()
    {
        var randomItemIndex = Random.Range(0, _upgradesWithRarity.Count);
        var item = Instantiate(upgradePrefab, shopContainer);
        item.GetComponent<UpgradeData>().data = _upgradesWithRarity[randomItemIndex];
        itemsForSale.Add(item);
    }
    private void PrepareEquipment()
    {
        var randomItemIndex = Random.Range(0, _equipmentsWithRarity.Count);
        var item = Instantiate(equipmentPrefab, shopContainer);
        item.GetComponent<EquipmentData>().data = _equipmentsWithRarity[randomItemIndex];
        itemsForSale.Add(item);
    }
}
