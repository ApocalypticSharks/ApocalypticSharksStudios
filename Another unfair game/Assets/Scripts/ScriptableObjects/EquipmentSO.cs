using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Blackjack Rogue/Equipment")]
public class EquipmentSO : ScriptableObject
{
    public List<CardSO> Cards;
    public string Name;
    public string Description;
    public EquipmentType Type;
    public string Sprite;
    public int Rarity;
    public int Cost;
}

public enum EquipmentType { Helmet, Armor, Legs, Hand, Ring, Necklace }
