using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ChestLootUI : MonoBehaviour
{
    public static ChestLootUI Instance { get; private set; }

    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private int maxSlots = 8;

    private readonly List<InventorySlotView> slots = new();
    private readonly List<Button> slotButtons = new();

    private GameObject panelRoot;
    private RectTransform slotsRoot;
    private Text titleText;
    private PlayerInteraction playerInteraction;
    private Inventory inventory;
    private ulong openChestNetworkId;

    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (slotPrefab == null && InventoryUI.Instance != null)
            slotPrefab = InventoryUI.Instance.slotPrefab;

        BuildPanel();
        Close();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!IsOpen)
            return;

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            Close();
            return;
        }

        if (openChestNetworkId == 0 || playerInteraction == null)
        {
            Close();
            return;
        }

        if (!TryGetOpenChest(out var chest))
        {
            Close();
            return;
        }

        if (!chest.IsWithinRange(playerInteraction.transform.position))
            Close();
    }

    public static void EnsureExists()
    {
        if (Instance != null)
            return;

        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return;

        canvas.gameObject.AddComponent<ChestLootUI>();
    }

    public void Open(ulong chestNetworkObjectId, PlayerInteraction interaction)
    {
        playerInteraction = interaction;
        inventory = interaction != null ? interaction.GetComponent<Inventory>() : null;
        openChestNetworkId = chestNetworkObjectId;

        if (titleText != null)
            titleText.text = "Сундук";

        panelRoot.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        openChestNetworkId = 0;
        playerInteraction = null;
        inventory = null;

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Refresh()
    {
        if (!IsOpen || inventory == null || !TryGetOpenChest(out var chest))
            return;

        EnsureSlotCount(maxSlots);

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < chest.LootItems.Count)
            {
                var loot = chest.LootItems[i];
                var data = inventory.GetItemData(loot.itemId.ToString());
                slots[i].SetItem(data != null ? data.Icon : null);
            }
            else
            {
                slots[i].SetEmpty();
            }

            slots[i].SetSelected(false);

            if (i < slotButtons.Count && slotButtons[i] != null)
                slotButtons[i].interactable = i < chest.LootItems.Count;
        }

        if (titleText != null)
            titleText.text = chest.HasLoot ? "Сундук" : "Сундук пуст";
    }

    private void BuildPanel()
    {
        panelRoot = new GameObject("ChestLootPanel", typeof(RectTransform));
        panelRoot.transform.SetParent(transform, false);

        var panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.sizeDelta = new Vector2(560f, 120f);
        panelRect.anchoredPosition = new Vector2(0f, -24f);

        var background = panelRoot.AddComponent<Image>();
        background.color = new Color(0.08f, 0.07f, 0.06f, 0.92f);
        background.raycastTarget = true;

        var titleObject = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        titleObject.transform.SetParent(panelRoot.transform, false);
        var titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(0f, 24f);
        titleRect.anchoredPosition = new Vector2(0f, -8f);
        titleText = titleObject.GetComponent<Text>();
        titleText.font = GetDefaultFont();
        titleText.text = "Сундук";
        titleText.fontSize = 14;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.86f, 0.8f, 0.62f, 1f);
        titleText.raycastTarget = false;

        var slotsObject = new GameObject("Slots", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        slotsObject.transform.SetParent(panelRoot.transform, false);
        slotsRoot = slotsObject.GetComponent<RectTransform>();
        slotsRoot.anchorMin = new Vector2(0.5f, 0f);
        slotsRoot.anchorMax = new Vector2(0.5f, 0f);
        slotsRoot.pivot = new Vector2(0.5f, 0f);
        slotsRoot.sizeDelta = new Vector2(520f, 64f);
        slotsRoot.anchoredPosition = new Vector2(0f, 16f);

        var layout = slotsObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    private void EnsureSlotCount(int count)
    {
        if (slotPrefab == null || slotsRoot == null)
            return;

        while (slots.Count < count)
        {
            int slotIndex = slots.Count;
            var slotObject = Instantiate(slotPrefab, slotsRoot);
            var slotView = slotObject.GetComponent<InventorySlotView>();
            var button = slotObject.GetComponent<Button>();
            if (button == null)
                button = slotObject.AddComponent<Button>();

            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.highlightedColor = new Color(0.75f, 0.55f, 0.15f, 0.9f);
            colors.pressedColor = new Color(0.55f, 0.4f, 0.1f, 1f);
            button.colors = colors;

            int capturedIndex = slotIndex;
            button.onClick.AddListener(() => OnSlotClicked(capturedIndex));

            if (slotView != null)
                slots.Add(slotView);
            slotButtons.Add(button);
        }
    }

    private void OnSlotClicked(int slotIndex)
    {
        if (playerInteraction == null || openChestNetworkId == 0)
            return;

        playerInteraction.RequestTakeLootFromChest(openChestNetworkId, slotIndex);
    }

    private bool TryGetOpenChest(out LootChest chest)
    {
        chest = null;
        if (openChestNetworkId == 0)
            return false;

        if (Unity.Netcode.NetworkManager.Singleton == null ||
            !Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                openChestNetworkId, out var networkObject))
            return false;

        chest = networkObject.GetComponent<LootChest>();
        return chest != null;
    }

    private static Font GetDefaultFont()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
