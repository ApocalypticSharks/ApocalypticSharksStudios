using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class WorldItemPickup : NetworkBehaviour
{
    [SerializeField] private string itemId = "bandage";
    [SerializeField] private float pickupRadius = 1.25f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public string ItemId => itemId;

    private void Awake()
    {
        ApplyVisual();
    }

    public override void OnNetworkSpawn()
    {
        ApplyVisual();
    }

    private void ApplyVisual()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        var data = Resources.Load<ItemData>($"Items/{itemId}");
        if (data == null || spriteRenderer == null)
            return;

        if (data.Icon != null)
            spriteRenderer.sprite = data.Icon;

        var category = data.IsWeapon && data.WeaponData != null
            ? SpriteWorldScale.GetPickupCategory(data.WeaponData)
            : data.PickupVisualCategory;

        float scale = SpriteWorldScale.GetPickupScale(spriteRenderer.sprite, category);
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    public bool IsWithinRange(Vector3 position)
    {
        return Vector2.Distance(transform.position, position) <= pickupRadius;
    }

    public void Despawn()
    {
        if (IsServer && IsSpawned)
            NetworkObject.Despawn(true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
