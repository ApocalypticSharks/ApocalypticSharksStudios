using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject BattlePanel;
    public GameObject ShopPanel;
    public GameObject StateSwitchPanel;
    public Transform ActiveUpgradeContainer;
    public TMP_Text GoldAmount;
    [SerializeField] private TMP_Text matchsticksText;

    [Header("Tooltip")]
    [SerializeField] private Vector2 tooltipScreenOffset = new Vector2(18f, -18f);
    [SerializeField] private float tooltipMaxWidth = 300f;

    private RectTransform _tooltipRoot;
    private TMP_Text _tooltipTitle;
    private TMP_Text _tooltipBody;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ChangeMatchsticksValue();
    }

    public void ChangePanel(GameState gameState)
    {
        HideTooltip();
        BattlePanel.SetActive(new GameState[] { GameState.BattleStart,
            GameState.BattlePlayerTurn,
            GameState.BattleEnemyTurn,
            GameState.BattleResults,
            GameState.BattleEnd }.Contains(gameState));
        ShopPanel.SetActive(gameState == GameState.Shop);
        StateSwitchPanel.SetActive(false);
    }

    public void ChangeGoldValue()
    {
        GoldAmount.text = PlayerManager.Instance.gold.ToString();
    }

    public void ChangeMatchsticksValue()
    {
        if (matchsticksText != null && PlayerManager.Instance != null)
            matchsticksText.text = PlayerManager.Instance.matchsticks.ToString();
    }

    public void ShowTooltip(string title, string description, Vector2 screenPosition)
    {
        EnsureTooltipBuilt();
        if (_tooltipRoot == null || _tooltipTitle == null || _tooltipBody == null)
            return;

        _tooltipTitle.text = string.IsNullOrEmpty(title) ? " " : title;
        _tooltipBody.text = string.IsNullOrEmpty(description) ? " " : description;
        _tooltipRoot.gameObject.SetActive(true);
        _tooltipRoot.SetAsLastSibling();

        LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRoot);
        PositionTooltip(screenPosition);
    }

    public void HideTooltip()
    {
        if (_tooltipRoot != null)
            _tooltipRoot.gameObject.SetActive(false);
    }

    private void PositionTooltip(Vector2 screenPointerPosition)
    {
        Canvas canvas = _tooltipRoot.GetComponentInParent<Canvas>();
        if (canvas == null)
            return;

        RectTransform canvasRect = canvas.transform as RectTransform;
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        Vector2 sp = screenPointerPosition + tooltipScreenOffset;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, sp, cam, out Vector2 localPoint);

        _tooltipRoot.anchorMin = _tooltipRoot.anchorMax = new Vector2(0.5f, 0.5f);
        _tooltipRoot.pivot = new Vector2(0f, 1f);
        _tooltipRoot.anchoredPosition = localPoint;

        LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRoot);

        float halfW = canvasRect.rect.width * 0.5f;
        float halfH = canvasRect.rect.height * 0.5f;
        Vector2 pos = _tooltipRoot.anchoredPosition;
        Vector2 sz = _tooltipRoot.rect.size;
        pos.x = Mathf.Clamp(pos.x, -halfW + 8f, halfW - sz.x - 8f);
        pos.y = Mathf.Clamp(pos.y, -halfH + 8f, halfH - sz.y - 8f);
        _tooltipRoot.anchoredPosition = pos;
    }

    private void EnsureTooltipBuilt()
    {
        if (_tooltipRoot != null)
            return;

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
            return;

        var rootGo = new GameObject("TooltipPanel", typeof(RectTransform));
        rootGo.transform.SetParent(canvas.transform, false);
        _tooltipRoot = rootGo.GetComponent<RectTransform>();

        var bg = rootGo.AddComponent<Image>();
        bg.color = new Color(0.07f, 0.08f, 0.1f, 0.95f);
        bg.raycastTarget = false;

        var vert = rootGo.AddComponent<VerticalLayoutGroup>();
        vert.padding = new RectOffset(12, 12, 10, 12);
        vert.spacing = 6;
        vert.childAlignment = TextAnchor.UpperLeft;
        vert.childControlWidth = true;
        vert.childControlHeight = true;
        vert.childForceExpandWidth = true;
        vert.childForceExpandHeight = false;

        var fitter = rootGo.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _tooltipTitle = CreateTooltipTextLine("TooltipTitle", rootGo.transform, 17f, FontStyles.Bold);
        _tooltipBody = CreateTooltipTextLine("TooltipBody", rootGo.transform, 14f, FontStyles.Normal);

        var leRoot = rootGo.AddComponent<LayoutElement>();
        leRoot.preferredWidth = tooltipMaxWidth;
        leRoot.minWidth = 120f;

        _tooltipTitle.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tooltipMaxWidth - 24f);
        _tooltipBody.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tooltipMaxWidth - 24f);

        rootGo.SetActive(false);
    }

    private static TMP_Text CreateTooltipTextLine(string name, Transform parent, float fontSize, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = Color.white;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;

        var le = go.AddComponent<LayoutElement>();
        le.minWidth = 80f;
        le.preferredWidth = -1f;
        le.flexibleWidth = 1f;
        return tmp;
    }
}
