using UnityEngine;

public class LootableCorpse : MonoBehaviour, IInteractable
{
    private bool isLootable;
    private bool isLooted;
    private RewardComponent rewardComponent;

    private void Awake()
    {
        rewardComponent = GetComponent<RewardComponent>();
    }

    public void Interact(GameObject interactor)
    {
        if (!isLootable)
        {
            return;
        }

        if (isLooted)
        {
            Debug.Log("Corpse already looted");
            return;
        }

        isLooted = true;
        Loot loot = rewardComponent != null ? rewardComponent.GetLoot() : null;

        if (loot == null)
        {
            Debug.Log("Corpse has no loot");
            return;
        }

        foreach (Item item in loot.GetItems())
        {
            Debug.Log("Looted item: " + item.GetItemName());
        }
    }

    public void SetLootable(bool lootable)
    {
        isLootable = lootable;
    }

    public void ResetLoot()
    {
        isLooted = false;
        isLootable = false;
    }
}
