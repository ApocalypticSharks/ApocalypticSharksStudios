using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    public int maxSlots = 8;

    public NetworkList<NetworkInventoryItem> NetworkItems { get; } = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private static Dictionary<string, ItemData> sharedItemDB;

    private void Awake()
    {
        EnsureItemDatabase();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkItems.OnListChanged += OnInventoryListChanged;

        if (IsOwner)
            RefreshOwnerUI(-1);
    }

    public override void OnNetworkDespawn()
    {
        NetworkItems.OnListChanged -= OnInventoryListChanged;
        base.OnNetworkDespawn();
    }

    private void OnInventoryListChanged(NetworkListEvent<NetworkInventoryItem> changeEvent)
    {
        if (!IsOwner)
            return;

        GetComponent<InventorySelection>()?.OnInventoryChanged();
    }

    private static void EnsureItemDatabase()
    {
        if (sharedItemDB != null)
            return;

        sharedItemDB = new Dictionary<string, ItemData>();
        foreach (ItemData item in Resources.LoadAll<ItemData>("Items"))
            sharedItemDB[item.ItemId] = item;
    }

    [Rpc(SendTo.Server)]
    public void AddItemRpc(string itemId)
    {
        TryAddItem(itemId);
    }

    public bool TryAddItem(string itemId)
    {
        if (!IsServer)
            return false;

        var data = GetItemData(itemId);
        if (data == null)
            return false;

        for (int i = 0; i < NetworkItems.Count; i++)
        {
            var existing = NetworkItems[i];
            if (existing.itemId.ToString() == itemId && existing.quantity < data.MaxStack)
            {
                existing.quantity += 1;
                NetworkItems[i] = existing;
                NotifyOwnerInventoryChangedClientRpc();
                return true;
            }
        }

        if (NetworkItems.Count >= maxSlots)
            return false;

        NetworkItems.Add(new NetworkInventoryItem(itemId, 1));
        NotifyOwnerInventoryChangedClientRpc();
        return true;
    }

    public void RemoveItem(int slotIndex, int amount = 1)
    {
        RemoveItemRpc(slotIndex, amount);
    }

    [Rpc(SendTo.Server)]
    public void RemoveItemRpc(int slotIndex, int amount = 1)
    {
        if (slotIndex < 0 || slotIndex >= NetworkItems.Count)
            return;

        var item = NetworkItems[slotIndex];
        item.quantity -= amount;

        if (item.quantity <= 0)
            NetworkItems.RemoveAt(slotIndex);
        else
            NetworkItems[slotIndex] = item;

        NotifyOwnerInventoryChangedClientRpc();
    }

    [ClientRpc]
    private void NotifyOwnerInventoryChangedClientRpc()
    {
        if (!IsOwner)
            return;

        GetComponent<InventorySelection>()?.OnInventoryChanged();
    }

    public void RefreshOwnerUI(int selectedIndex)
    {
        if (!IsOwner)
            return;

        var ui = InventoryUI.Instance != null ? InventoryUI.Instance : FindFirstObjectByType<InventoryUI>();
        if (ui != null)
            ui.Refresh(this, selectedIndex);
    }

    public void RefreshUI(int selectedIndex)
    {
        RefreshOwnerUI(selectedIndex);
    }

    public int GetItemCount() => NetworkItems.Count;

    public int CountItem(string itemId)
    {
        int count = 0;
        foreach (var item in NetworkItems)
        {
            if (item.itemId.ToString() == itemId)
                count += item.quantity;
        }

        return count;
    }

    public bool TryRemoveFromSlot(int slotIndex, int amount = 1)
    {
        if (!IsServer || amount <= 0)
            return false;

        if (slotIndex < 0 || slotIndex >= NetworkItems.Count)
            return false;

        var item = NetworkItems[slotIndex];
        item.quantity -= amount;

        if (item.quantity <= 0)
            NetworkItems.RemoveAt(slotIndex);
        else
            NetworkItems[slotIndex] = item;

        NotifyOwnerInventoryChangedClientRpc();
        return true;
    }

    public bool TryConsumeOne(string itemId)
    {
        if (!IsServer)
            return false;

        for (int i = 0; i < NetworkItems.Count; i++)
        {
            if (NetworkItems[i].itemId.ToString() != itemId)
                continue;

            var item = NetworkItems[i];
            item.quantity -= 1;

            if (item.quantity <= 0)
                NetworkItems.RemoveAt(i);
            else
                NetworkItems[i] = item;

            NotifyOwnerInventoryChangedClientRpc();
            return true;
        }

        return false;
    }

    [Rpc(SendTo.Server)]
    public void ConsumeMagazineRpc(string ammoItemId)
    {
        if (!TryConsumeOne(ammoItemId))
        {
            MagazineConsumeFailedOwnerRpc();
            return;
        }

        MagazineConsumeSucceededOwnerRpc();
    }

    [Rpc(SendTo.Owner)]
    private void MagazineConsumeSucceededOwnerRpc()
    {
        GetComponentInChildren<WeaponScript>()?.ApplyMagazineReload();
    }

    [Rpc(SendTo.Owner)]
    private void MagazineConsumeFailedOwnerRpc()
    {
        GetComponentInChildren<WeaponScript>()?.CancelMagazineReload();
    }

    public ItemData GetItemData(string itemId)
    {
        EnsureItemDatabase();
        return sharedItemDB.TryGetValue(itemId, out ItemData data) ? data : null;
    }

    public List<InventoryItem> GetItems()
    {
        var result = new List<InventoryItem>(NetworkItems.Count);
        foreach (var item in NetworkItems)
            result.Add(new InventoryItem(item.itemId.ToString(), item.quantity));
        return result;
    }
}
