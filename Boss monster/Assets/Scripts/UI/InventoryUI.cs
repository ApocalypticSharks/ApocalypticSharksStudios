using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public void Refresh(Inventory inventory)
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        foreach (var item in inventory.items)
        {
            var data = inventory.GetItemData(item.itemId);
            var slot = Instantiate(slotPrefab, transform);
            slot.GetComponentInChildren<Image>().sprite = data.Icon;
        }
    }
}
