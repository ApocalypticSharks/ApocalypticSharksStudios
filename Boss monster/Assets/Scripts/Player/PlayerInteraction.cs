using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInteraction : NetworkBehaviour
{
    [SerializeField] private float interactRadius = 1.25f;
    [SerializeField] private Inventory inventory;
    [SerializeField] private InventorySelection inventorySelection;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private ReliquaryCarrier reliquaryCarrier;

    private ContactFilter2D pickupFilter;

    private void Awake()
    {
        if (inventory == null)
            inventory = GetComponent<Inventory>();

        if (inventorySelection == null)
            inventorySelection = GetComponent<InventorySelection>();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (reliquaryCarrier == null)
            reliquaryCarrier = GetComponent<ReliquaryCarrier>();

        pickupFilter = ContactFilter2D.noFilter;
        pickupFilter.useTriggers = true;
    }

    public void TryInteractNearby()
    {
        if (!IsOwner || IsCarryingReliquary())
            return;

        ChestLootUI.EnsureExists();

        var chest = FindClosestChest();
        var pickup = FindClosestPickup();

        if (chest != null && (pickup == null || IsCloser(chest.transform.position, pickup.transform.position)))
        {
            if (ChestLootUI.Instance != null && ChestLootUI.Instance.IsOpen)
            {
                if (chest.HasLoot)
                    RequestTakeFirstLootFromOpenChestRpc(chest.NetworkObjectId);
                else
                    ChestLootUI.Instance.Close();
            }
            else
            {
                RequestOpenChestRpc(chest.NetworkObjectId);
            }

            return;
        }

        if (pickup != null)
            RequestPickupRpc(pickup.NetworkObjectId);
    }

    public void RequestTakeLootFromChest(ulong chestNetworkObjectId, int slotIndex)
    {
        if (!IsOwner || IsCarryingReliquary())
            return;

        RequestTakeLootFromChestRpc(chestNetworkObjectId, slotIndex);
    }

    public void TryDropSelectedItem()
    {
        if (!IsOwner)
            return;

        DropItemRpc(ComputeDropPosition());
    }

    public void TryUseSelectedItem()
    {
        if (IsCarryingReliquary())
            return;

        if (inventorySelection == null || inventory == null)
            return;

        int slotIndex = inventorySelection.SelectedIndex;
        if (slotIndex < 0)
            return;

        var items = inventory.GetItems();
        if (slotIndex >= items.Count)
            return;

        var itemData = inventory.GetItemData(items[slotIndex].itemId);
        if (itemData == null || !itemData.IsConsumable)
            return;

        UseItemRpc(slotIndex);
    }

    [Rpc(SendTo.Server)]
    private void DropItemRpc(Vector3 dropPosition)
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (playerHealth == null || playerHealth.IsDead.Value)
            return;

        if (IsCarryingReliquary())
        {
            if (reliquaryCarrier == null)
                reliquaryCarrier = GetComponent<ReliquaryCarrier>();

            reliquaryCarrier?.DropVoluntary(dropPosition);
            return;
        }

        if (inventorySelection == null || inventory == null)
            return;

        int slotIndex = inventorySelection.SelectedIndex;
        if (slotIndex < 0)
            return;

        var items = inventory.GetItems();
        if (slotIndex >= items.Count)
            return;

        var itemData = inventory.GetItemData(items[slotIndex].itemId);
        if (itemData == null || itemData.Prefab == null)
            return;

        if (!SpawnWorldPickup(itemData, dropPosition))
            return;

        inventory.TryRemoveFromSlot(slotIndex);
    }

    [Rpc(SendTo.Server)]
    private void UseItemRpc(int slotIndex)
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (playerHealth == null || playerHealth.IsDead.Value)
            return;

        var items = inventory.GetItems();
        if (slotIndex < 0 || slotIndex >= items.Count)
            return;

        var itemData = inventory.GetItemData(items[slotIndex].itemId);
        if (itemData == null || !itemData.IsConsumable)
            return;

        playerHealth.Heal(itemData.HealAmount);
        inventory.TryConsumeOne(items[slotIndex].itemId);
    }

    [Rpc(SendTo.Server)]
    private void RequestOpenChestRpc(ulong chestNetworkObjectId)
    {
        if (IsCarryingReliquary())
            return;

        if (!TryGetChest(chestNetworkObjectId, out var chest))
            return;

        if (!chest.IsWithinRange(transform.position))
            return;

        if (!chest.IsOpened.Value)
            chest.IsOpened.Value = true;

        OpenChestOwnerRpc(chestNetworkObjectId);
    }

    [Rpc(SendTo.Owner)]
    private void OpenChestOwnerRpc(ulong chestNetworkObjectId)
    {
        ChestLootUI.EnsureExists();
        ChestLootUI.Instance?.Open(chestNetworkObjectId, this);
    }

    [Rpc(SendTo.Server)]
    private void RequestTakeLootFromChestRpc(ulong chestNetworkObjectId, int slotIndex)
    {
        if (IsCarryingReliquary())
            return;

        if (!TryGetChest(chestNetworkObjectId, out var chest))
            return;

        if (!chest.IsWithinRange(transform.position))
            return;

        if (reliquaryCarrier == null)
            reliquaryCarrier = GetComponent<ReliquaryCarrier>();

        if (!chest.TryTakeLoot(slotIndex, inventory, reliquaryCarrier))
            return;

        RefreshChestOwnerRpc(chestNetworkObjectId);
    }

    [Rpc(SendTo.Server)]
    private void RequestTakeFirstLootFromOpenChestRpc(ulong chestNetworkObjectId)
    {
        if (IsCarryingReliquary())
            return;

        if (!TryGetChest(chestNetworkObjectId, out var chest))
            return;

        if (!chest.IsWithinRange(transform.position) || !chest.HasLoot)
            return;

        if (reliquaryCarrier == null)
            reliquaryCarrier = GetComponent<ReliquaryCarrier>();

        if (!chest.TryTakeLoot(0, inventory, reliquaryCarrier))
            return;

        RefreshChestOwnerRpc(chestNetworkObjectId);
    }

    [Rpc(SendTo.Owner)]
    private void RefreshChestOwnerRpc(ulong chestNetworkObjectId)
    {
        if (ChestLootUI.Instance == null || !ChestLootUI.Instance.IsOpen)
            return;

        ChestLootUI.Instance.Refresh();

        if (TryGetChest(chestNetworkObjectId, out var chest) && !chest.HasLoot)
            ChestLootUI.Instance.Close();
    }

    private LootChest FindClosestChest()
    {
        var results = new List<Collider2D>(8);
        Physics2D.OverlapCircle(transform.position, interactRadius, pickupFilter, results);

        LootChest closest = null;
        float closestDistance = float.MaxValue;

        foreach (var collider in results)
        {
            var chest = collider.GetComponent<LootChest>();
            if (chest == null)
                continue;

            float distance = Vector2.Distance(transform.position, chest.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = chest;
            }
        }

        return closest;
    }

    private bool TryGetChest(ulong chestNetworkObjectId, out LootChest chest)
    {
        chest = null;
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(chestNetworkObjectId, out NetworkObject networkObject))
            return false;

        chest = networkObject.GetComponent<LootChest>();
        return chest != null;
    }

    private bool IsCloser(Vector3 first, Vector3 second)
    {
        float firstDistance = Vector2.Distance(transform.position, first);
        float secondDistance = Vector2.Distance(transform.position, second);
        return firstDistance <= secondDistance;
    }

    private WorldItemPickup FindClosestPickup()
    {
        var results = new List<Collider2D>(8);
        Physics2D.OverlapCircle(transform.position, interactRadius, pickupFilter, results);

        WorldItemPickup closest = null;
        float closestDistance = float.MaxValue;

        foreach (var collider in results)
        {
            var pickup = collider.GetComponent<WorldItemPickup>();
            if (pickup == null)
                continue;

            float distance = Vector2.Distance(transform.position, pickup.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = pickup;
            }
        }

        return closest;
    }

    [Rpc(SendTo.Server)]
    private void RequestPickupRpc(ulong networkObjectId)
    {
        if (IsCarryingReliquary())
            return;

        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
            return;

        var pickup = networkObject.GetComponent<WorldItemPickup>();
        if (pickup == null || !pickup.IsWithinRange(transform.position))
            return;

        var itemData = inventory.GetItemData(pickup.ItemId);
        if (itemData != null && itemData.IsReliquary)
        {
            if (reliquaryCarrier == null)
                reliquaryCarrier = GetComponent<ReliquaryCarrier>();

            if (reliquaryCarrier == null || !reliquaryCarrier.TryEquipFromWorld())
                return;

            pickup.Despawn();
            return;
        }

        if (!inventory.TryAddItem(pickup.ItemId))
            return;

        pickup.Despawn();
    }

    private bool IsCarryingReliquary()
    {
        if (reliquaryCarrier == null)
            reliquaryCarrier = GetComponent<ReliquaryCarrier>();

        return reliquaryCarrier != null && reliquaryCarrier.IsCarrying.Value;
    }

    private Vector3 ComputeDropPosition()
    {
        const float dropDistance = 0.65f;

        if (Camera.main != null)
        {
            var mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var direction = new Vector2(
                mouseWorld.x - transform.position.x,
                mouseWorld.y - transform.position.y);

            if (direction.sqrMagnitude > 0.0001f)
                return transform.position + (Vector3)(direction.normalized * dropDistance);
        }

        return transform.position + (Vector3)(-transform.up * dropDistance);
    }

    private static bool SpawnWorldPickup(ItemData itemData, Vector3 position)
    {
        var dropped = Instantiate(itemData.Prefab, position, Quaternion.identity);
        if (dropped == null)
            return false;

        var networkObject = dropped.GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            Destroy(dropped);
            return false;
        }

        networkObject.Spawn(true);
        return true;
    }
}
