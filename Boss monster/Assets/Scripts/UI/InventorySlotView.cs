using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image icon;

    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.85f);
    [SerializeField] private Color selectedColor = new Color(0.75f, 0.55f, 0.15f, 1f);
    [SerializeField] private Color emptyColor = new Color(0.12f, 0.12f, 0.12f, 0.45f);

    private bool hasItem;

    private void Awake()
    {
        if (background == null)
            background = transform.Find("Background")?.GetComponent<Image>();
        if (icon == null)
            icon = transform.Find("Icon")?.GetComponent<Image>();
    }

    public void SetEmpty()
    {
        hasItem = false;
        if (icon != null)
        {
            icon.enabled = false;
            icon.sprite = null;
        }

        if (background != null)
            background.color = emptyColor;
    }

    public void SetItem(Sprite sprite)
    {
        hasItem = sprite != null;
        if (icon != null)
        {
            icon.enabled = hasItem;
            icon.sprite = sprite;
        }

        if (background != null)
            background.color = hasItem ? normalColor : emptyColor;
    }

    public void SetSelected(bool selected)
    {
        if (background == null)
            return;

        if (!hasItem)
        {
            background.color = emptyColor;
            return;
        }

        background.color = selected ? selectedColor : normalColor;
    }
}
