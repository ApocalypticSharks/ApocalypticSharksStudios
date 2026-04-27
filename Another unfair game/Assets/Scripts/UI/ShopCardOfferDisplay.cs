using UnityEngine;
using UnityEngine.EventSystems;

public class ShopCardOfferDisplay : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private ShopCardOfferData offer;

    private void Awake()
    {
        if (offer == null)
            offer = GetComponent<ShopCardOfferData>() ?? GetComponentInParent<ShopCardOfferData>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (offer == null)
            offer = GetComponent<ShopCardOfferData>() ?? GetComponentInParent<ShopCardOfferData>();
        if (offer?.card == null || UIManager.Instance == null)
            return;
        UIManager.Instance.ShowTooltip(offer.card.cardName, offer.card.description, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance?.HideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (offer == null)
            offer = GetComponent<ShopCardOfferData>();
        if (GameStateManager.Instance.currentGameState != GameState.Shop || offer == null || offer.card == null)
            return;

        int cost = PassiveUpgradeBonuses.GetShopGoldPriceAfterDiscount(offer.card.shopCost);
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
