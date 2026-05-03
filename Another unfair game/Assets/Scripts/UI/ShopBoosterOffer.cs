using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopBoosterOffer : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private bool opened;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameStateManager.Instance.currentGameState != GameState.Shop || opened)
            return;
        if (ShopManager.Instance == null)
            return;

        int cost = PassiveUpgradeBonuses.GetShopGoldPriceAfterDiscount(ShopManager.Instance.boosterGoldCost);
        if (PlayerManager.Instance.gold < cost)
            return;

        PlayerManager.Instance.SpendGold(cost);
        opened = true;

        IReadOnlyList<CardSO> pulled = ShopManager.Instance.PullBoosterCards(3);
        foreach (CardSO card in pulled)
        {
            if (card == null || DeckManager.Instance == null)
                continue;
            if (DeckManager.Instance.PlayerLibraryContains(card))
            {
                int compensation = Mathf.CeilToInt(card.shopCost * 0.5f);
                if (compensation > 0 && PlayerManager.Instance != null)
                    PlayerManager.Instance.GetGold(compensation);
                continue;
            }
            DeckManager.Instance.AddCardToDeck(card);
        }

        Destroy(gameObject);
    }
}
