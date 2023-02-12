using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeSpawner : MonoBehaviour
{
    [SerializeField] Object enemy;
    public float spawnTimeLimit = 3.0f;
    public void SpawnEnemy()
    {
        StartCoroutine(Spawn());
    }
    IEnumerator Spawn()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, spawnTimeLimit));
        GameObject newEnemy = Instantiate(enemy, transform.position, Quaternion.identity) as GameObject;
        newEnemy.transform.SetParent(transform);
    }

}
