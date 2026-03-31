// DealerAI.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class DealerAI : MonoBehaviour
{
    //[Header("Dealer Info")]
    //public Dealer[] dealers = new Dealer[3];
    //public List<Transform> dealerEntities;

    //private void Awake()
    //{
    //    dealers[0] = new Dealer();
    //    dealers[1] = new Dealer();
    //    dealers[2] = new Dealer();
    //    AssignDealersToEnemies();
    //}

    //public Dealer currentDealer;
    //public async Task Initialize(List<DealerCompositionSO> floorDelaers)
    //{
    //    int randomComposition = Random.Range(0,floorDelaers.Count);
    //    for (int i = 0; i < floorDelaers[randomComposition].dealers.Count; i++)
    //    {
    //        dealers[i].Initialize(floorDelaers[randomComposition].dealers[i]);
    //    }
    //}

    //public void AssignDealersToEnemies()
    //{
    //    if (dealerEntities == null)
    //    {
    //        Debug.LogError("Enemies parent GameObject is not assigned!");
    //        return;
    //    }

    //    for (int i = 0; i < dealers.Length && i < dealerEntities.Count; i++)
    //    {
    //        Dealer dealer = dealers[i];
    //        Transform enemyTransform = dealerEntities[i];

    //        dealer.entity = enemyTransform;
    //        Debug.Log($"Assigning dealer {dealer} to enemy {enemyTransform.name}");
    //    }
    //}
}