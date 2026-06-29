using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct ChestLootDefinition
{
    public string itemId;
    public int quantity;
}

[RequireComponent(typeof(NetworkObject))]
public class LootChest : NetworkBehaviour
{
    [SerializeField] private float interactRadius = 1.5f;
    [SerializeField] private ChestLootDefinition[] startingLoot;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color closedTint = Color.white;
    [SerializeField] private Color openedTint = new(0.85f, 0.78f, 0.65f, 1f);
    [SerializeField] private Color emptyTint = new(0.55f, 0.52f, 0.48f, 0.85f);

    public NetworkList<NetworkInventoryItem> LootItems { get; } = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> IsOpened { get; } = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public float InteractRadius => interactRadius;

    public bool HasLoot
    {
        get
        {
            for (int i = 0; i < LootItems.Count; i++)
            {
                if (LootItems[i].quantity > 0)
                    return true;
            }

            return false;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            InitializeLoot();

        IsOpened.OnValueChanged += OnOpenedChanged;
        LootItems.OnListChanged += OnLootChanged;
        RefreshVisual();
    }

    public override void OnNetworkDespawn()
    {
        IsOpened.OnValueChanged -= OnOpenedChanged;
        LootItems.OnListChanged -= OnLootChanged;
    }

    public bool IsWithinRange(Vector3 position)
    {
        return Vector2.Distance(transform.position, position) <= interactRadius;
    }

    public bool TryTakeLoot(int slotIndex, Inventory inventory, ReliquaryCarrier reliquaryCarrier)
    {
        if (!IsServer || inventory == null || slotIndex < 0 || slotIndex >= LootItems.Count)
            return false;

        var loot = LootItems[slotIndex];
        if (loot.quantity <= 0)
            return false;

        string itemId = loot.itemId.ToString();
        var itemData = inventory.GetItemData(itemId);
        if (itemData == null)
            return false;

        if (itemData.IsReliquary)
        {
            if (reliquaryCarrier == null || reliquaryCarrier.IsCarrying.Value)
                return false;

            if (!reliquaryCarrier.TryEquipFromWorld())
                return false;
        }
        else if (!inventory.TryAddItem(itemId))
        {
            return false;
        }

        if (!IsOpened.Value)
            IsOpened.Value = true;

        loot.quantity -= 1;
        if (loot.quantity <= 0)
            LootItems.RemoveAt(slotIndex);
        else
            LootItems[slotIndex] = loot;

        return true;
    }

    public void SetLootContents(IEnumerable<ChestLootDefinition> loot)
    {
        if (!IsServer || loot == null)
            return;

        while (LootItems.Count > 0)
            LootItems.RemoveAt(LootItems.Count - 1);

        foreach (var entry in loot)
        {
            if (string.IsNullOrWhiteSpace(entry.itemId) || entry.quantity <= 0)
                continue;

            LootItems.Add(new NetworkInventoryItem(entry.itemId, entry.quantity));
        }

        IsOpened.Value = false;
        RefreshVisual();
    }

    private void InitializeLoot()
    {
        if (LootItems.Count > 0 || startingLoot == null)
            return;

        foreach (var entry in startingLoot)
        {
            if (string.IsNullOrWhiteSpace(entry.itemId) || entry.quantity <= 0)
                continue;

            LootItems.Add(new NetworkInventoryItem(entry.itemId, entry.quantity));
        }
    }

    private void OnOpenedChanged(bool previous, bool current)
    {
        RefreshVisual();
    }

    private void OnLootChanged(NetworkListEvent<NetworkInventoryItem> changeEvent)
    {
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            return;

        if (!HasLoot && IsOpened.Value)
            spriteRenderer.color = emptyTint;
        else if (IsOpened.Value)
            spriteRenderer.color = openedTint;
        else
            spriteRenderer.color = closedTint;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.9f, 0.7f, 0.2f, 0.85f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
