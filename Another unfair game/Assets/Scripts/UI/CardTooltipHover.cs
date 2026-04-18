using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to a <see cref="Graphic"/> on a playing card (e.g. FrontImage). Shows <see cref="CardSO"/> text on hover.
/// </summary>
public class CardTooltipHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private CardData _card;

    private void Awake()
    {
        _card = GetComponentInParent<CardData>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_card == null || _card.data == null || UIManager.Instance == null)
            return;
        UIManager.Instance.ShowTooltip(_card.data.cardName, _card.data.description, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance?.HideTooltip();
    }
}
