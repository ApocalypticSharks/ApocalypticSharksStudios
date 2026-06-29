using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CultistHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 60f;

    public NetworkVariable<float> Value = new NetworkVariable<float>(60f);
    public NetworkVariable<bool> IsDead = new NetworkVariable<bool>(false);

    private CultistAI cultistAI;
    private SpriteRenderer spriteRenderer;
    private Collider2D bodyCollider;

    public override void OnNetworkSpawn()
    {
        cultistAI = GetComponent<CultistAI>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bodyCollider = GetComponent<Collider2D>();

        if (IsServer)
            Value.Value = maxHealth;

        IsDead.OnValueChanged += OnDeadStateChanged;
        OnDeadStateChanged(false, IsDead.Value);
    }

    public override void OnNetworkDespawn()
    {
        IsDead.OnValueChanged -= OnDeadStateChanged;
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer || IsDead.Value || damage <= 0)
            return;

        Value.Value = Mathf.Max(0f, Value.Value - damage);
        if (Value.Value <= 0f)
            IsDead.Value = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer || IsDead.Value)
            return;

        switch (collision.gameObject.tag)
        {
            case "bullet":
                var bullet = collision.gameObject.GetComponent<BulletScript>();
                if (bullet != null && bullet.owner.Value != EnemyIds.MeleeOwnerId)
                {
                    TakeDamage(bullet.damage.Value);
                    bullet.DestroyBulletRpc();
                }
                break;
            case "meleeHitBox":
                var melee = collision.gameObject.GetComponent<MeleeHitboxScript>();
                if (melee != null && melee.owner.Value != EnemyIds.MeleeOwnerId)
                    TakeDamage(melee.damage.Value);
                break;
        }
    }

    private void OnDeadStateChanged(bool previous, bool current)
    {
        if (!current)
            return;

        if (cultistAI != null)
            cultistAI.HandleDeath();

        if (bodyCollider != null)
            bodyCollider.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.color = new Color(0.35f, 0.35f, 0.35f, 0.6f);

        if (IsServer)
            StartCoroutine(DespawnAfterDelay(2f));
    }

    private System.Collections.IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }
}
