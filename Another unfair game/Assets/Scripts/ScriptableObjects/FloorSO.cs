using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FloorSO", menuName = "Blackjack Rogue/Floor")]
public class FloorSO : ScriptableObject
{
    public string Name;
    public int baseEnemyHealth = 100;
    public int baseEnemyDifficulty = 0;
    public FloorSO nextFloor;
    public List<DealerCompositionSO> dealerCompositions;
    public DealerSO bossDealer;
}
