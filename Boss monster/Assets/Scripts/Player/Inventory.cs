using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    public int maxSlots = 8;

    [SerializeField]
    public List<InventoryItem> items = new List<InventoryItem>();
    private Dictionary<string, ItemData> itemDB;

    private void Start()
    {
        if (IsOwner)
        {
            itemDB = new Dictionary<string, ItemData>();
            foreach (ItemData item in Resources.LoadAll<ItemData>("Items"))
                itemDB[item.ItemId] = item;
        }
    }
    [Rpc(SendTo.Server)]
    public void AddItemRpc(string itemId)
    {
        InventoryItem existing = items.Find(i => i.itemId == itemId && i.quantity < GetItemData(i.itemId).MaxStack);
        if (existing != null)
        {
            existing.quantity += 1;
        }
        else if (items.Count < maxSlots)
        {
            items.Add(new InventoryItem(itemId, 1));
        }
        UpdateInventoryRpc();
    }
    public void RemoveItem(int slotIndex, int amount = 1)
    {
        if (slotIndex >= items.Count) return;

        InventoryItem item = items[slotIndex];
        item.quantity -= amount;

        if (item.quantity <= 0)
            items.RemoveAt(slotIndex);

        UpdateInventoryRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateInventoryRpc()
    {
        FindObjectOfType<InventoryUI>().Refresh(this);
    }

    private void SelectItem()
    {
        
    }
    public ItemData GetItemData(string itemId)
    {
        return itemDB.TryGetValue(itemId, out ItemData data) ? data : null;
    }

    public List<InventoryItem> GetItems() => items;
}
