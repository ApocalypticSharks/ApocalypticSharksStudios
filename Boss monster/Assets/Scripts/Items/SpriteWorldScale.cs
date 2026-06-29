using UnityEngine;

public enum PickupVisualCategory
{
    SmallItem = 0,
    LargeItem = 1,
    WeaponMelee = 2,
    WeaponSmg = 3,
    WeaponRifle = 4
}

public static class SpriteWorldScale
{
    public const float PixelsPerUnit = 256f;
    public const float PlayerBodyHeightPx = 96f;

    public static float PlayerBodyHeightWorld => PlayerBodyHeightPx / PixelsPerUnit;

    public static float GetPickupWorldSize(PickupVisualCategory category)
    {
        switch (category)
        {
            case PickupVisualCategory.LargeItem:
                return PlayerBodyHeightWorld * 0.66f;
            case PickupVisualCategory.WeaponMelee:
                return PlayerBodyHeightWorld * 0.40f;
            case PickupVisualCategory.WeaponSmg:
                return PlayerBodyHeightWorld * 0.70f;
            case PickupVisualCategory.WeaponRifle:
                return PlayerBodyHeightWorld * 0.88f;
            default:
                return PlayerBodyHeightWorld * 0.50f;
        }
    }

    public static float GetDefaultEquippedLengthRatio(WeaponData data)
    {
        if (data == null)
            return 1f;

        if (data.IsMelee)
            return 0.38f;

        if (data.Sprite != null && data.Sprite.rect.width >= 220f)
            return 1.05f;

        return 0.82f;
    }

    public static float GetUniformScale(Sprite sprite, float targetWorldSize)
    {
        if (sprite == null || targetWorldSize <= 0f)
            return 1f;

        float nativeSize = Mathf.Max(sprite.rect.width, sprite.rect.height) / PixelsPerUnit;
        if (nativeSize <= 0.0001f)
            return 1f;

        return targetWorldSize / nativeSize;
    }

    public static float GetPickupScale(Sprite sprite, PickupVisualCategory category)
    {
        return GetUniformScale(sprite, GetPickupWorldSize(category));
    }

    public static PickupVisualCategory GetPickupCategory(WeaponData data)
    {
        if (data == null)
            return PickupVisualCategory.SmallItem;

        if (data.IsMelee)
            return PickupVisualCategory.WeaponMelee;

        if (data.Sprite != null && data.Sprite.rect.width >= 220f)
            return PickupVisualCategory.WeaponRifle;

        return PickupVisualCategory.WeaponSmg;
    }

    public static float GetEquippedWeaponScale(WeaponData data)
    {
        if (data == null || data.Sprite == null)
            return 1f;

        float ratio = data.EquippedLengthRatio > 0f
            ? data.EquippedLengthRatio
            : GetDefaultEquippedLengthRatio(data);

        return GetUniformScale(data.Sprite, PlayerBodyHeightWorld * ratio);
    }
}
