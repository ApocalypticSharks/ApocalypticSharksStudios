using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }
    [Header("Deck References")]
    public List<CardSO> baseDeck = new(); // Стартовая колода
    [SerializeField] private List<CardSO> gameDeck = new();
    [SerializeField] private List<CardSO> shopDeck = new();
    [SerializeField] private List<CardSO> discardPile = new();

    [Header("Deck Settings")]
    [SerializeField] private int maxDeckSize = 30;
    [SerializeField] private int minDeckSize = 10;

    // События
    public System.Action<CardSO> OnCardDrawn;
    public System.Action<CardSO> OnCardPlayed;
    public System.Action<CardSO> OnCardDiscarded;
    public System.Action OnDeckShuffled;
    public System.Action OnPlayerHandChanged;

    [Header("Card Prefab")]
    [SerializeField] public GameObject cardPrefab;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        InitializeGameDeck(baseDeck);
    }

    // Инициализация колоды
    public void InitializeGameDeck(List<CardSO> baseDeck)
    {
        gameDeck.Clear();
        if (baseDeck != null)
            gameDeck.AddRange(baseDeck);
        ShuffleDeck(gameDeck);
    }

    

    // Перемешивание колоды
    public void ShuffleDeck(List<CardSO> deck)
    {
        // Fisher-Yates shuffle
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            CardSO temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }

        OnDeckShuffled?.Invoke();
        Debug.Log($"Deck shuffled. {deck.Count} cards remaining.");
    }

    // Взятие карты
    public void DrawFromGameDeck(List<GameObject> hand, Transform handContainer)
    {
        if (gameDeck.Count == 0)
        {
            // Перемешиваем сброс обратно в колоду
            ReshuffleDiscardPile();
        }

        CardSO card = gameDeck[0];
        GameObject cardInstance = Instantiate(cardPrefab, handContainer);
        cardInstance.GetComponent<CardData>().data = card;
        cardInstance.GetComponent<CardData>().Initialize();
        gameDeck.RemoveAt(0);

        //card.onPlayEffects.ForEach((effect) => effect.ApplyEffect(GameStateManager.Instance.currentGameState));
        
        hand.Add(cardInstance);

        TryApplyPoisonedCardDrawUpgrade(card, hand);

        if (PlayerManager.Instance != null && hand == PlayerManager.Instance.playerHand
            && BattleManager.Instance != null && BattleManager.Instance.dealers != null
            && BattleManager.Instance.dealers.Count > 0)
            PlayerManager.Instance.RegisterCoinCardPlayedForGreed(card);

        foreach (UpgradeData upgrade in GameStateManager.Instance.upgrades)
        {
            if (upgrade?.data?.onPlayEffects == null)
                continue;
            int lv = upgrade.EffectiveLevel;
            foreach (EffectStruct effect in upgrade.data.onPlayEffects)
            {
                EffectStruct scaled = effect;
                scaled.value = UpgradeData.ScaledEffectValue(effect.value, lv);
                scaled.ApplyEffect(GameStateManager.Instance.currentGameState);
            }
        }
    }

    public CardSO DrawFromShopDeck()
    {
        if (shopDeck.Count == 0)
        {
            ShuffleDeck(shopDeck);
        }

        CardSO card = shopDeck[0];
        shopDeck.RemoveAt(0);
        return card;
    }

    private static void TryApplyPoisonedCardDrawUpgrade(CardSO card, List<GameObject> hand)
    {
        if (card == null || PlayerManager.Instance == null || hand == null)
            return;
        if (hand == PlayerManager.Instance.playerHand)
            return;
        if (PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveOpponentDrawPoisonCardAppliesPoison) <= 0)
            return;
        if (!card.HasPoisonWinEffect())
            return;
        if (BattleManager.Instance?.dealers == null)
            return;
        Dealer owner = null;
        foreach (Dealer d in BattleManager.Instance.dealers)
        {
            if (d != null && d.dealerHand == hand)
            {
                owner = d;
                break;
            }
        }
        if (owner == null || owner.dealerHealth <= 0 || card.onWinEffects == null)
            return;
        var ctx = EffectWinContext.ForPoisonedCardsDealerDraw(card, owner);
        foreach (EffectStruct effect in card.onWinEffects)
        {
            if (effect.type == EffectType.CardWinPoison)
                effect.ApplyWinEffect(in ctx);
        }
    }

    // Восстановление колоды из сброса
    public void ReshuffleDiscardPile()
    {
        Debug.Log("Reshuffling discard pile into deck...");

        gameDeck.AddRange(discardPile);
        discardPile.Clear();

        ShuffleDeck(gameDeck);
    }

    // Сброс карты (без эффектов)
    public void DiscardCard(GameObject card, List<GameObject> hand, bool toDiscardPile = true, bool fromPlayerMatchstickBurn = false)
    {
        if (hand.Contains(card))
        {
            hand.Remove(card);
            CardData cardData = card.GetComponent<CardData>();
            CardSO cardSo = cardData != null ? cardData.data : null;

            if (fromPlayerMatchstickBurn && cardSo != null && cardSo.HasPoisonWinEffect())
            {
                int smokeStacks = PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassivePoisonAllEnemiesOnPoisonCardBurn);
                if (smokeStacks > 0 && BattleManager.Instance?.dealers != null)
                {
                    foreach (Dealer d in BattleManager.Instance.dealers)
                    {
                        if (d != null && d.dealerHealth > 0)
                            d.AddPoison(smokeStacks);
                    }
                }
            }

            if (toDiscardPile && cardSo != null)
                discardPile.Add(cardSo);

            // Применяем эффекты "при сбросе"
            if (cardSo != null)
                ApplyCardEffects(cardSo.onDiscardEffects);

            Destroy(card);
            //if (hand == GetPlayerHand())
            //    OnPlayerHandChanged?.Invoke();
        }
    }

    public void DiscradAllCards(List<GameObject> hand)
    {
        while (hand.Count > 0)
        {
            DiscardCard(hand[0], hand);
        }
    }

    public void ReturnCardToShop(CardSO card, List<CardSO> hand)
    {
        if (hand.Contains(card))
        {
            hand.Remove(card);
            shopDeck.Add(card);
            OnPlayerHandChanged?.Invoke();
        }
    }

    public void ReturnAllCardsToShop(List<CardSO> hand) 
    {
        while (hand.Count > 0)
        {
            ReturnCardToShop(hand[0], hand);
        }
    }

    // Применение эффектов карты
    private void ApplyCardEffects(List<EffectStruct> effects)
    {
        if (effects == null || effects.Count == 0) return;

        foreach (EffectStruct effect in effects)
        {
            effect.ApplyEffect(GameStateManager.Instance.currentGameState);
            Debug.Log($"Applied effect: {effect.description}");
        }
    }

    // Добавление карты в колоду (между боями)
    public void AddCardToDeck(CardSO newCard)
    {
        if (gameDeck.Count >= maxDeckSize)
        {
            Debug.LogWarning($"Deck is full! Max size: {maxDeckSize}");
            return;
        }

        gameDeck.Add(newCard);
        ShuffleDeck(gameDeck);
        Debug.Log($"Added {newCard.cardName} to deck.");
    }

    // Удаление карты из колоды (между боями)
    public bool RemoveCardFromDeck(CardSO cardToRemove)
    {
        bool removed = gameDeck.Remove(cardToRemove);

        if (removed)
        {
            Debug.Log($"Removed {cardToRemove.cardName} from deck.");
        }

        return removed;
    }

    // Геттеры для UI
    public int GetDeckCount() => gameDeck.Count;
    public int GetDiscardCount() => discardPile.Count;

    /// <summary>Whether this <see cref="CardSO"/> is already in the draw pile, discard, or the player hand (shop should not sell duplicates).</summary>
    public bool PlayerLibraryContains(CardSO card)
    {
        if (card == null)
            return false;
        foreach (CardSO c in gameDeck)
        {
            if (c == card)
                return true;
        }
        foreach (CardSO c in discardPile)
        {
            if (c == card)
                return true;
        }
        if (PlayerManager.Instance?.playerHand == null)
            return false;
        foreach (GameObject go in PlayerManager.Instance.playerHand)
        {
            if (go == null)
                continue;
            CardData cd = go.GetComponent<CardData>();
            if (cd != null && cd.data == card)
                return true;
        }
        return false;
    }

    // Подсчет очков в руке (для блекджека)
    //public int CalculateHandValue(List<CardSO> hand)
    //{
    //    int totalValue = 0;
    //    int aceCount = 0;

    //    foreach (CardSO card in hand)
    //    {
    //        if (card.rank == CardRank.Ace)
    //        {
    //            aceCount++;
    //            totalValue += 1; // Сначала считаем тузы как 1
    //        }
    //        else
    //        {
    //            totalValue += card.baseValue;
    //        }
    //    }

    //    // Пытаемся использовать тузы как 11, если это выгодно
    //    while (aceCount > 0 && totalValue + 10 <= 21)
    //    {
    //        totalValue += 10;
    //        aceCount--;
    //    }

    //    return totalValue;
    //}

    // Проверка на перебор
    //public bool IsBust(List<CardSO> hand)
    //{
    //    return CalculateHandValue(hand) > 21;
    //}

    // Проверка на блекджек
    //public bool HasBlackjack(List<CardSO> hand)
    //{
    //    return hand.Count == 2 && CalculateHandValue(hand) == 21;
    //}
}