using System.Collections;
using System.Collections.Generic;
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

    public string ItemId { get { return itemId;  } }
    public string ItemName { get { return itemName; } }
    public Sprite Icon { get { return icon; } }
    public GameObject Prefab { get { return prefab; } }
    public int MaxStack { get { return maxStack; } }
    public bool Stackable { get { return stackable; } }
}
