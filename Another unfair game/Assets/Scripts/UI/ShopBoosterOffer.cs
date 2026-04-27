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
            if (card != null)
                DeckManager.Instance.AddCardToDeck(card);
        }

        Destroy(gameObject);
    }
}
