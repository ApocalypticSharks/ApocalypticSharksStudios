using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Transform leftDoorTransform, rightDoorTransform, leftBalconyTransform, rightBalconyTransform, roofTransform;
    [SerializeField] private DojoScript dojoParams;
    private bool spawning;
    private List<IEnemyInterface> rangeEnemiesList = new List<IEnemyInterface>();
    private List<IEnemyInterface> meleeEnemiesList = new List<IEnemyInterface>();
    private List<IEnemyInterface> roofEnemiesList = new List<IEnemyInterface>();

    private void Start()
    {
        meleeEnemiesList.Add(new Farmer());
        meleeEnemiesList.Add(new Farmer());
        rangeEnemiesList.Add(new Thrower());
        rangeEnemiesList.Add(new Thrower());
        roofEnemiesList.Add(new Assassin());
    }
    void Update()
    {
        if (leftDoorTransform.childCount<=0)
        {
            SpawnMelee(leftDoorTransform);
        }
        if (rightDoorTransform.childCount <= 0)
        {
            SpawnMelee(rightDoorTransform);
        }
        if (leftBalconyTransform.childCount <= 0)
        {
            SpawnRange(leftBalconyTransform);
        }
        if (rightBalconyTransform.childCount <= 0)
        {
            SpawnRange(rightBalconyTransform);
        }
        if (roofTransform.childCount <= 0)
        {
            SpawnRoof(roofTransform);
        }
    }

    private void SpawnMelee(Transform position)
    {
        List<IEnemyInterface> deactivatedEnemies = meleeEnemiesList.Where(enemy => !enemy.enemyInstance.activeSelf).ToList();
        deactivatedEnemies[Random.Range(0, deactivatedEnemies.Count)].Activate(position);
    }

    private void SpawnRange(Transform position)
    {
        List<IEnemyInterface> deactivatedEnemies = rangeEnemiesList.Where(enemy => !enemy.enemyInstance.activeSelf).ToList();
        deactivatedEnemies[Random.Range(0, deactivatedEnemies.Count)].Activate(position);
    }

    private void SpawnRoof(Transform position)
    {
        List<IEnemyInterface> deactivatedEnemies = roofEnemiesList.Where(enemy => !enemy.enemyInstance.activeSelf).ToList();
        deactivatedEnemies[Random.Range(0, deactivatedEnemies.Count)].Activate(position);
    }
}
