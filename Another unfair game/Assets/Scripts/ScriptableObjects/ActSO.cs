using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActSO", menuName = "Blackjack Rogue/Act")]
public class ActSO : ScriptableObject
{
    public string Name;
    public List<FloorSO> floors;
}
