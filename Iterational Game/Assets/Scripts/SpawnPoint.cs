using System.Collections;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    [SerializeField] private float respawnDelay = 10f;
    [SerializeField] private MonoBehaviour[] componentsToDisableWhileDead;

    private Health enemyHealth;
    private LootableCorpse lootableCorpse;
    private Coroutine respawnCoroutine;

    private void Awake()
    {
        if (enemy == null)
        {
            Debug.LogError("SpawnPoint has no enemy assigned", this);
            enabled = false;
            return;
        }

        enemyHealth = enemy.GetComponent<Health>();
        lootableCorpse = enemy.GetComponent<LootableCorpse>();

        if (enemyHealth == null)
        {
            Debug.LogError("SpawnPoint enemy has no Health component", enemy);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnDied += OnEnemyDied;
        }
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnDied -= OnEnemyDied;
        }
    }

    private void Start()
    {
        SetAliveState();
    }

    private void OnEnemyDied(GameObject killer)
    {
        SetCorpseState();

        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }

        respawnCoroutine = StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        enemy.transform.position = transform.position;
        enemyHealth.ResetHealth();
        lootableCorpse?.ResetLoot();
        SetAliveState();

        respawnCoroutine = null;
    }

    private void SetAliveState()
    {
        enemy.SetActive(true);
        SetDeadStateComponentsEnabled(true);
        lootableCorpse?.ResetLoot();
    }

    private void SetCorpseState()
    {
        SetDeadStateComponentsEnabled(false);
        lootableCorpse?.SetLootable(true);
    }

    private void SetDeadStateComponentsEnabled(bool isEnabled)
    {
        foreach (MonoBehaviour component in componentsToDisableWhileDead)
        {
            if (component != null)
            {
                component.enabled = isEnabled;
            }
        }
    }
}
