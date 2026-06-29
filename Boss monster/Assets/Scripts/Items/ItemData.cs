using UnityEngine;
[CreateAssetMenu(fileName = "New ItemData", menuName = "ScriptableObjects/ItemData", order = 2)]
public class ItemData : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject prefab;
    [SerializeField] private bool stackable;
    [SerializeField] private int maxStack = 1;
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private int healAmount;
    [SerializeField] private bool isReliquary;
    [SerializeField] private PickupVisualCategory pickupVisualCategory = PickupVisualCategory.SmallItem;

    public string ItemId { get { return itemId;  } }
    public string ItemName { get { return itemName; } }
    public Sprite Icon { get { return icon; } }
    public GameObject Prefab { get { return prefab; } }
    public int MaxStack { get { return maxStack; } }
    public bool Stackable { get { return stackable; } }
    public WeaponData WeaponData { get { return weaponData; } }
    public bool IsWeapon { get { return weaponData != null; } }
    public int HealAmount { get { return healAmount; } }
    public bool IsConsumable { get { return healAmount > 0 && !isReliquary; } }
    public bool IsReliquary { get { return isReliquary; } }
    public PickupVisualCategory PickupVisualCategory { get { return pickupVisualCategory; } }
}
