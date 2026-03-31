using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DealerCompositionSO", menuName = "Blackjack Rogue/Dealer Composition")]
public class DealerCompositionSO : ScriptableObject
{
    public List<DealerSO> dealers;
}
