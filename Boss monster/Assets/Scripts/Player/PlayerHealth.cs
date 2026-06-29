using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    public NetworkVariable<float> Value = new NetworkVariable<float>(100f);
    public NetworkVariable<bool> IsDead = new NetworkVariable<bool>(false);
    public float MaxValue => maxHealth;

    public override void OnNetworkSpawn()
    {
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
        if (!IsServer || IsDead.Value)
            return;

        Value.Value = Mathf.Max(0f, Value.Value - damage);
        if (Value.Value <= 0f)
            IsDead.Value = true;
    }

    public void Heal(float amount)
    {
        if (!IsServer || IsDead.Value || amount <= 0f)
            return;

        Value.Value = Mathf.Min(maxHealth, Value.Value + amount);
    }

    private void OnDeadStateChanged(bool previous, bool current)
    {
        if (!current)
            return;

        var player = GetComponent<PlayerScript>();
        if (player != null)
            player.HandleDeath();
    }
}
