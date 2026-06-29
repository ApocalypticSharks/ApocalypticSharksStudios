using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Spawns every ItemData from Resources/Items in a grid and test loot chests when hosting the item test scene.
/// </summary>
public class ItemTestSceneSetup : MonoBehaviour
{
    [SerializeField] private GameObject lootChestPrefab;
    [SerializeField] private Vector2 gridOrigin = new(-6.5f, 2.5f);
    [SerializeField] private Vector2 cellSpacing = new(2.4f, -2.4f);
    [SerializeField] private int columns = 4;

    private bool contentSpawned;

    private void Start()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnServerStarted += SpawnContentIfNeeded;
        if (NetworkManager.Singleton.IsServer)
            SpawnContentIfNeeded();
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnServerStarted -= SpawnContentIfNeeded;
    }

    private void SpawnContentIfNeeded()
    {
        if (contentSpawned)
            return;

        contentSpawned = true;
        SpawnGroundPickups();
        SpawnTestChests();
    }

    private void SpawnGroundPickups()
    {
        var allItems = Resources.LoadAll<ItemData>("Items");
        System.Array.Sort(allItems, (a, b) => string.CompareOrdinal(a.ItemId, b.ItemId));

        for (int i = 0; i < allItems.Length; i++)
            SpawnItemPickup(allItems[i], i);
    }

    private void SpawnItemPickup(ItemData itemData, int index)
    {
        if (itemData == null || itemData.Prefab == null)
            return;

        int column = index % columns;
        int row = index / columns;
        var position = gridOrigin + new Vector2(column * cellSpacing.x, row * cellSpacing.y);

        var pickup = Instantiate(itemData.Prefab, position, Quaternion.identity);
        pickup.name = $"Pickup_{itemData.ItemId}";

        var networkObject = pickup.GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            Destroy(pickup);
            return;
        }

        networkObject.Spawn(true);
    }

    private void SpawnTestChests()
    {
        if (lootChestPrefab == null)
            return;

        SpawnChest(new Vector2(5f, 2.5f), new[]
        {
            new ChestLootDefinition { itemId = "bandage", quantity = 3 },
            new ChestLootDefinition { itemId = "reliquary", quantity = 1 }
        });

        SpawnChest(new Vector2(7.5f, 2.5f), new[]
        {
            new ChestLootDefinition { itemId = "weapon_knife", quantity = 1 },
            new ChestLootDefinition { itemId = "weapon_smg", quantity = 1 },
            new ChestLootDefinition { itemId = "weapon_bolt_rifle", quantity = 1 }
        });

        SpawnChest(new Vector2(6.25f, 0f), new[]
        {
            new ChestLootDefinition { itemId = "smg_ammo", quantity = 2 },
            new ChestLootDefinition { itemId = "ammo_shotgun", quantity = 2 },
            new ChestLootDefinition { itemId = "ammo_rifle", quantity = 2 }
        });
    }

    private void SpawnChest(Vector2 position, ChestLootDefinition[] loot)
    {
        var chestObject = Instantiate(lootChestPrefab, position, Quaternion.identity);
        var chest = chestObject.GetComponent<LootChest>();
        var networkObject = chestObject.GetComponent<NetworkObject>();
        if (chest == null || networkObject == null)
        {
            Destroy(chestObject);
            return;
        }

        chestObject.name = "LootChest_Test";
        networkObject.Spawn(true);
        chest.SetLootContents(loot);
    }
}
