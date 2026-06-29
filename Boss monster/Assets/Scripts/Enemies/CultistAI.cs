using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CultistAI : NetworkBehaviour
{
    private enum State
    {
        Idle,
        Chase,
        Attack
    }

    [Header("Detection & combat")]
    [SerializeField] private float detectionRadius = 6f;
    [SerializeField] private float attackRange = 0.55f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private int meleeDamage = 18;
    [SerializeField] private float loseTargetMultiplier = 1.35f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.4f;
    [SerializeField] private float lookAhead = 0.45f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("References")]
    [SerializeField] private Transform meleePoint;
    [SerializeField] private GameObject meleeHitboxPrefab;

    private static readonly float[] AvoidanceAngles = { 25f, -25f, 50f, -50f, 75f, -75f, 110f, -110f };

    private Rigidbody2D rb;
    private CultistHealth health;
    private State state = State.Idle;
    private Transform target;
    private float nextAttackTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<CultistHealth>();
    }

    private void FixedUpdate()
    {
        if (!IsServer || health == null || health.IsDead.Value)
            return;

        UpdateTarget();
        switch (state)
        {
            case State.Idle:
                break;
            case State.Chase:
                ChaseTarget();
                break;
            case State.Attack:
                TryAttack();
                break;
        }
    }

    public void HandleDeath()
    {
        state = State.Idle;
        target = null;
        rb.linearVelocity = Vector2.zero;
    }

    private void UpdateTarget()
    {
        if (target != null && !IsValidTarget(target))
            target = null;

        if (target == null)
            target = FindNearestPlayer();

        if (target == null)
        {
            state = State.Idle;
            return;
        }

        float distance = Vector2.Distance(transform.position, target.position);
        float loseRadius = detectionRadius * loseTargetMultiplier;

        if (distance > loseRadius)
        {
            target = null;
            state = State.Idle;
            return;
        }

        state = distance <= attackRange ? State.Attack : State.Chase;
    }

    private Transform FindNearestPlayer()
    {
        Transform nearest = null;
        float nearestSqr = detectionRadius * detectionRadius;
        var players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);

        foreach (var player in players)
        {
            if (!IsValidTarget(player.transform))
                continue;

            float sqr = ((Vector2)player.transform.position - rb.position).sqrMagnitude;
            if (sqr <= nearestSqr)
            {
                nearestSqr = sqr;
                nearest = player.transform;
            }
        }

        return nearest;
    }

    private bool IsValidTarget(Transform candidate)
    {
        if (candidate == null)
            return false;

        var player = candidate.GetComponent<PlayerScript>();
        if (player == null || player.IsDead)
            return false;

        var playerHealth = candidate.GetComponent<PlayerHealth>();
        return playerHealth == null || !playerHealth.IsDead.Value;
    }

    private void ChaseTarget()
    {
        Vector2 toTarget = (Vector2)target.position - rb.position;
        if (toTarget.sqrMagnitude < 0.0001f)
            return;

        Vector2 direction = GetSteeredDirection(toTarget.normalized);
        if (direction.sqrMagnitude < 0.0001f)
            return;

        FaceDirection(direction);
        Vector2 next = rb.position + direction * (moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(next);
    }

    private void TryAttack()
    {
        if (target == null)
            return;

        Vector2 toTarget = (Vector2)target.position - rb.position;
        if (toTarget.sqrMagnitude > 0.0001f)
            FaceDirection(toTarget.normalized);

        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackCooldown;
        SpawnMeleeHitbox();
    }

    private void FaceDirection(Vector2 direction)
    {
        transform.up = -direction;
    }

    private Vector2 GetSteeredDirection(Vector2 desired)
    {
        if (!IsDirectionBlocked(desired))
            return desired;

        float bestScore = float.MinValue;
        Vector2 bestDirection = Vector2.zero;

        foreach (float angle in AvoidanceAngles)
        {
            Vector2 candidate = Rotate(desired, angle);
            if (IsDirectionBlocked(candidate))
                continue;

            float score = Vector2.Dot(candidate, desired);
            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = candidate;
            }
        }

        return bestDirection;
    }

    private bool IsDirectionBlocked(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return true;

        var hit = Physics2D.CircleCast(
            rb.position,
            0.14f,
            direction.normalized,
            lookAhead,
            obstacleMask);

        return hit.collider != null;
    }

    private static Vector2 Rotate(Vector2 vector, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos);
    }

    private void SpawnMeleeHitbox()
    {
        if (meleeHitboxPrefab == null)
            return;

        Vector3 spawnPos = meleePoint != null ? meleePoint.position : transform.position;
        Quaternion spawnRot = meleePoint != null ? meleePoint.rotation : transform.rotation;

        var hitbox = Instantiate(meleeHitboxPrefab, spawnPos, spawnRot);
        var networkObject = hitbox.GetComponent<NetworkObject>();
        networkObject.Spawn();

        var melee = hitbox.GetComponent<MeleeHitboxScript>();
        melee.owner.Value = EnemyIds.MeleeOwnerId;
        melee.damage.Value = meleeDamage;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}
