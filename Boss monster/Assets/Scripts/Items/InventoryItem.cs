using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

[System.Serializable]
public class InventoryItem
{
    public string itemId;
    public int quantity;
    public InventoryItem(string itemId, int quantity = 1)
    {
        this.itemId = itemId;
        this.quantity = quantity;
    }
}
