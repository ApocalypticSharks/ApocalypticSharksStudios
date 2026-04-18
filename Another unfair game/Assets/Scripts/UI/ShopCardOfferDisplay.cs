using UnityEngine;
using UnityEngine.EventSystems;

public class ShopCardOfferDisplay : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private ShopCardOfferData offer;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameStateManager.Instance.currentGameState != GameState.Shop || offer.card == null)
            return;

        int cost = offer.card.shopCost;
        if (offer.isPurchased)
        {
            if (DeckManager.Instance.RemoveCardFromDeck(offer.card))
            {
                PlayerManager.Instance.GetGold(cost / 2);
                offer.isPurchased = false;
            }
        }
        else
        {
            if (PlayerManager.Instance.gold < cost)
                return;
            PlayerManager.Instance.SpendGold(cost);
            DeckManager.Instance.AddCardToDeck(offer.card);
            offer.isPurchased = true;
        }
    }
}
