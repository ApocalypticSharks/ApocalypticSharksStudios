using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Loot", menuName = "Scriptable Objects/Loot")]
public class Loot : ScriptableObject
{
    [SerializeField] private List<Item> items;

    public List<Item> GetItems()
    {
        return items;
    }
}
