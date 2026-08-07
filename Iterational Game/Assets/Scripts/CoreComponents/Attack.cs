using System;
using UnityEngine;

public class Attack : MonoBehaviour
{
    private Transform currentTarget;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int attackDamage = 10;

    public event Action<Transform> OnTargetChanged;

    public void AttackTarget()
    {
        if (currentTarget == null || DistanceToTarget() > attackRange)
        {
            return;
        }

        Health targetHealth = currentTarget.GetComponent<Health>();

        if (targetHealth == null || targetHealth.IsDead)
        {
            return;
        }

        targetHealth.TakeDamage(attackDamage, gameObject);
    }

    public void SetTarget(Transform target)
    {
        currentTarget = target;
        OnTargetChanged?.Invoke(currentTarget);
    }

    public Transform GetTarget()
    {
        return currentTarget;
    }

    private float DistanceToTarget()
    {
        return Vector2.Distance(transform.position, currentTarget.position);
    }
}
