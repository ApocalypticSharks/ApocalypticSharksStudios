using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    public GameObject slotPrefab;

    private readonly List<InventorySlotView> slots = new();
    private PlayerHealth playerHealth;
    private PlayerStamina playerStamina;
    private Image healthFill;
    private Image staminaFill;
    private Text healthText;
    private Text staminaText;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        RefreshStatusBars();
    }

    public void Refresh(Inventory inventory, int selectedIndex)
    {
        EnsureSlotCount(inventory.maxSlots);
        TrackPlayerStatus(inventory);
        EnsureStatusPanel();

        var items = inventory.GetItems();
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null)
                continue;

            if (i < items.Count)
            {
                var data = inventory.GetItemData(items[i].itemId);
                slots[i].SetItem(data != null ? data.Icon : null);
            }
            else
            {
                slots[i].SetEmpty();
            }

            slots[i].SetSelected(i == selectedIndex);
        }

        RefreshStatusBars();
    }

    private void EnsureSlotCount(int count)
    {
        PruneDestroyedSlots();

        while (slots.Count < count)
        {
            var slotObject = Instantiate(slotPrefab, transform);
            var slotView = slotObject.GetComponent<InventorySlotView>();
            if (slotView != null)
                slots.Add(slotView);
        }
    }

    private void PruneDestroyedSlots()
    {
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (slots[i] == null)
                slots.RemoveAt(i);
        }
    }

    private void TrackPlayerStatus(Inventory inventory)
    {
        if (inventory == null)
            return;

        playerHealth = inventory.GetComponent<PlayerHealth>();
        playerStamina = inventory.GetComponent<PlayerStamina>();
    }

    private void EnsureStatusPanel()
    {
        if (healthFill != null && staminaFill != null)
            return;

        var parent = transform.parent != null ? transform.parent : transform;
        var panel = new GameObject("StatusPanel", typeof(RectTransform));
        panel.transform.SetParent(parent, false);

        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(150f, 56f);

        var inventoryRect = transform as RectTransform;
        panelRect.anchoredPosition = inventoryRect != null
            ? inventoryRect.anchoredPosition + new Vector2(-370f, 0f)
            : new Vector2(-370f, 100f);

        healthFill = CreateStatusRow(panel.transform, "HP", new Vector2(0f, 14f), new Color(0.55f, 0.07f, 0.06f, 1f), out healthText);
        staminaFill = CreateStatusRow(panel.transform, "ST", new Vector2(0f, -14f), new Color(0.55f, 0.43f, 0.08f, 1f), out staminaText);
    }

    private Image CreateStatusRow(Transform parent, string label, Vector2 position, Color fillColor, out Text valueText)
    {
        var row = new GameObject(label, typeof(RectTransform));
        row.transform.SetParent(parent, false);

        var rowRect = row.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 0.5f);
        rowRect.anchorMax = new Vector2(0.5f, 0.5f);
        rowRect.pivot = new Vector2(0.5f, 0.5f);
        rowRect.sizeDelta = new Vector2(150f, 20f);
        rowRect.anchoredPosition = position;

        var labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelObject.transform.SetParent(row.transform, false);
        var labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(0f, 0.5f);
        labelRect.pivot = new Vector2(0f, 0.5f);
        labelRect.sizeDelta = new Vector2(28f, 20f);
        labelRect.anchoredPosition = Vector2.zero;
        var labelText = labelObject.GetComponent<Text>();
        labelText.font = GetDefaultFont();
        labelText.text = label;
        labelText.fontSize = 12;
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = new Color(0.82f, 0.78f, 0.66f, 1f);
        labelText.raycastTarget = false;

        var background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        background.transform.SetParent(row.transform, false);
        var backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0f, 0.5f);
        backgroundRect.pivot = new Vector2(0f, 0.5f);
        backgroundRect.sizeDelta = new Vector2(112f, 14f);
        backgroundRect.anchoredPosition = new Vector2(32f, 0f);
        var backgroundImage = background.GetComponent<Image>();
        backgroundImage.color = new Color(0.08f, 0.08f, 0.07f, 0.85f);
        backgroundImage.raycastTarget = false;

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fill.transform.SetParent(background.transform, false);
        var fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);
        var fillImage = fill.GetComponent<Image>();
        fillImage.color = fillColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 1f;
        fillImage.raycastTarget = false;

        var valueObject = new GameObject("Value", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        valueObject.transform.SetParent(background.transform, false);
        var valueRect = valueObject.GetComponent<RectTransform>();
        valueRect.anchorMin = Vector2.zero;
        valueRect.anchorMax = Vector2.one;
        valueRect.offsetMin = Vector2.zero;
        valueRect.offsetMax = Vector2.zero;
        valueText = valueObject.GetComponent<Text>();
        valueText.font = GetDefaultFont();
        valueText.fontSize = 10;
        valueText.alignment = TextAnchor.MiddleCenter;
        valueText.color = new Color(0.9f, 0.86f, 0.72f, 1f);
        valueText.raycastTarget = false;

        return fillImage;
    }

    private void RefreshStatusBars()
    {
        if (healthFill != null && playerHealth != null)
            SetStatusValue(healthFill, healthText, playerHealth.Value.Value, playerHealth.MaxValue);

        if (staminaFill != null && playerStamina != null)
            SetStatusValue(staminaFill, staminaText, playerStamina.Value.Value, playerStamina.MaxValue);
    }

    private void SetStatusValue(Image fill, Text text, float value, float max)
    {
        float normalized = max > 0f ? Mathf.Clamp01(value / max) : 0f;
        fill.fillAmount = normalized;

        if (text != null)
            text.text = $"{Mathf.CeilToInt(value)}/{Mathf.CeilToInt(max)}";
    }

    private Font GetDefaultFont()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
