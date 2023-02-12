using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyInterface
{
    public GameObject enemyInstance { get; set; }
    public void Activate(Transform position)
    {
    }
}
