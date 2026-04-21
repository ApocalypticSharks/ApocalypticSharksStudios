using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// На карте в руке игрока: ПКМ — сжечь карту за спички (см. <see cref="CardSO.GetMatchstickBurnCost"/>).
/// Вешается на тот же объект, что и <see cref="CardTooltipHover"/> (например FrontImage).
/// </summary>
public class PlayerHandCardBurn : MonoBehaviour, IPointerClickHandler
{
    private CardData _card;

    private void Awake()
    {
        _card = GetComponentInParent<CardData>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
            return;

        if (_card == null)
            _card = GetComponentInParent<CardData>();
        if (_card == null || !_card.IsFaceUp || _card.data == null)
            return;
        if (GameStateManager.Instance == null || PlayerManager.Instance == null || DeckManager.Instance == null)
            return;
        if (GameStateManager.Instance.currentGameState != GameState.BattlePlayerTurn)
            return;

        GameObject cardGo = _card.gameObject;
        if (!PlayerManager.Instance.playerHand.Contains(cardGo))
            return;

        int cost = _card.data.GetMatchstickBurnCost();
        if (!PlayerManager.Instance.TrySpendMatchsticks(cost))
            return;

        UIManager.Instance?.HideTooltip();
        DeckManager.Instance.DiscardCard(cardGo, PlayerManager.Instance.playerHand);
        PlayerManager.Instance.RefreshHandStateAfterModify();
    }
}
