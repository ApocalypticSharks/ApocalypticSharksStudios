using Unity.Netcode;
using UnityEngine;

public class ReliquaryCarrier : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer backRenderer;
    [SerializeField] private Sprite reliquarySprite;
    [SerializeField] private float backVisualScale = 1f;

    public NetworkVariable<bool> IsCarrying { get; } = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private WeaponScript weapon;
    private ItemData reliquaryData;

    private void Awake()
    {
        weapon = GetComponentInChildren<WeaponScript>(true);
        reliquaryData = Resources.Load<ItemData>("Items/reliquary");
        EnsureBackRenderer();
    }

    public override void OnNetworkSpawn()
    {
        IsCarrying.OnValueChanged += OnCarryingChanged;
        ApplyCarryingState(IsCarrying.Value);
    }

    public override void OnNetworkDespawn()
    {
        IsCarrying.OnValueChanged -= OnCarryingChanged;
    }

    public bool TryEquipFromWorld()
    {
        if (!IsServer || IsCarrying.Value)
            return false;

        IsCarrying.Value = true;
        return true;
    }

    public void DropVoluntary(Vector3 position)
    {
        if (!IsServer || !IsCarrying.Value)
            return;

        DropAtPosition(position);
    }

    public void DropOnDeath(Vector3 position)
    {
        if (!IsServer || !IsCarrying.Value)
            return;

        DropAtPosition(position);
    }

    private void DropAtPosition(Vector3 position)
    {
        IsCarrying.Value = false;

        if (reliquaryData == null || reliquaryData.Prefab == null)
            return;

        var dropped = Instantiate(reliquaryData.Prefab, position, Quaternion.identity);
        var networkObject = dropped.GetComponent<NetworkObject>();
        if (networkObject != null)
            networkObject.Spawn(true);
    }

    private void OnCarryingChanged(bool previous, bool current)
    {
        ApplyCarryingState(current);
    }

    private void ApplyCarryingState(bool carrying)
    {
        EnsureBackRenderer();

        if (backRenderer != null)
        {
            backRenderer.enabled = carrying;
            if (carrying)
                ApplyBackVisualScale();
        }

        if (!carrying)
            return;

        if (weapon == null)
            weapon = GetComponentInChildren<WeaponScript>(true);

        if (weapon != null)
            weapon.Holster();
    }

    private void EnsureBackRenderer()
    {
        if (backRenderer != null)
            return;

        var backObject = new GameObject("ReliquaryBack");
        backObject.transform.SetParent(transform, false);
        backObject.transform.localPosition = Vector3.zero;

        backRenderer = backObject.AddComponent<SpriteRenderer>();
        backRenderer.sortingOrder = 0;

        if (reliquarySprite == null && reliquaryData != null)
            reliquarySprite = reliquaryData.Icon;

        backRenderer.sprite = reliquarySprite;
        backRenderer.enabled = false;
    }

    private void ApplyBackVisualScale()
    {
        if (backRenderer == null || backRenderer.sprite == null)
            return;

        float scale = backVisualScale;
        if (scale <= 0f)
            scale = SpriteWorldScale.GetPickupScale(backRenderer.sprite, PickupVisualCategory.LargeItem);

        backRenderer.transform.localScale = new Vector3(scale, scale, 1f);
    }
}
