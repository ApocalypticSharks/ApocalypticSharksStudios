using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public Transform dealerEntities;
    public List<Dealer> dealers = new List<Dealer>();
    public Dealer activeDealer;
    public GameObject dealerPrefab;
    public bool isStunned;

    public int currentBattleNumber;
    public int maxBattlesCount;

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
    }

    public void StartNewBattle()
    {
        PlayerManager.Instance.shield = 0;
        PlayerManager.Instance.poisonStacks = 0;
        PlayerManager.Instance.ClearGreedRoundBonus();

        currentBattleNumber += 1;
        if (currentBattleNumber > maxBattlesCount)
        {
            StartBossBattle();
        }
        else
        {
            ApplyBattleStartShield();
            InitializeEnemies();
            InitializeStartingHands();
            GameStateManager.Instance.MoveToNextGameState(GameState.BattlePlayerTurn);
        }
    }
    public void StartBossBattle()
    {
        PlayerManager.Instance.shield = 0;
        PlayerManager.Instance.poisonStacks = 0;
        PlayerManager.Instance.ClearGreedRoundBonus();
        ApplyBattleStartShield();

        InitializeBoss();
        InitializeStartingHands();
        GameStateManager.Instance.MoveToNextGameState(GameState.BattlePlayerTurn);
    }
    public void EnemyTurn()
    {
        RevealAllDealerHoleCards();
        foreach (var dealer in dealers)
        {
            activeDealer = dealer;
            while (dealer.ShouldHit())
            {
                dealer.DrawCard();
            }
        }
        GameStateManager.Instance.MoveToNextGameState(GameState.BattleResults);
    }

    /// <summary>Show dealer hole cards before showdown or dealer draw (blackjack rules).</summary>
    public void RevealAllDealerHoleCards()
    {
        if (dealers == null)
            return;
        foreach (Dealer dealer in dealers)
        {
            if (dealer != null)
                dealer.RevealHoleCard();
        }
    }
    public void BattleResults()
    {
        int p = PlayerManager.Instance.currentHandValue;
        foreach (var dealer in dealers)
        {
            if (dealer.dealerHealth > 0)
            {
                int d = dealer.currentHandValue;
                if (p <= 21 && d > 21)
                {
                    Debug.Log($"Damage dealt to enemy: {p}");
                    EffectProcessor.PlayerDealsPhysicalDamageToDealer(dealer, p, null);
                    ApplyWinningHandCardEffects(true, dealer, PlayerManager.Instance.playerHand);
                }
                else if (p > 21 && d <= 21)
                {
                    Debug.Log($"Damage dealt to player: {d}");
                    PlayerManager.Instance.TakeDamage(d, ignoreShield: false, attackingDealer: dealer);
                    ApplyWinningHandCardEffects(false, dealer, dealer.dealerHand);
                }
                else if (p > d)
                {
                    Debug.Log($"Damage dealt to enemy: {p}");
                    EffectProcessor.PlayerDealsPhysicalDamageToDealer(dealer, p, null);
                    ApplyWinningHandCardEffects(true, dealer, PlayerManager.Instance.playerHand);
                }
                else if (p < d)
                {
                    Debug.Log($"Damage dealt to player: {d}");
                    PlayerManager.Instance.TakeDamage(d, ignoreShield: false, attackingDealer: dealer);
                    ApplyWinningHandCardEffects(false, dealer, dealer.dealerHand);
                }
            }
        }
        if (PlayerManager.Instance.playerHealth > 0
            && dealers.Any(dealer => dealer.dealerHealth > 0))
        {
            StartNextRound();
        }
        else
        {
            GameStateManager.Instance.MoveToNextGameState(GameState.BattleEnd);
        }
    }

    public void StartNextRound()
    {
        PlayerManager.Instance.ClearGreedRoundBonus();

        int extraPoisonTickWaves = PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveExtraPoisonTicksPerRound);
        int poisonTickWaves = Mathf.Max(1, 1 + extraPoisonTickWaves);
        for (int wave = 0; wave < poisonTickWaves; wave++)
        {
            PlayerManager.Instance.TickPoisonAtRoundStart();
            foreach (var dealer in dealers)
            {
                if (dealer.dealerHealth > 0)
                    dealer.TickPoisonAtRoundStart();
            }
        }

        DeckManager.Instance.DiscradAllCards(PlayerManager.Instance.playerHand);
        InitializeStartingHands();
        foreach (var dealer in dealers)
        {
            DeckManager.Instance.DiscradAllCards(dealer.dealerHand);
            if(dealer.dealerHealth > 0)
                dealer.DrawFromDealerDeck();
        }
    }
    public void BattleEnd()
    {
        PlayerManager.Instance.shield = 0;
        PlayerManager.Instance.poisonStacks = 0;
        PlayerManager.Instance.ClearGreedRoundBonus();

        DeckManager.Instance.DiscradAllCards(PlayerManager.Instance.playerHand);
        foreach (var dealer in dealers)
        {
            for (int i = 0; i < 2; i++)
            {
                DeckManager.Instance.DiscardCard(dealer.dealerHand[i], dealer.dealerHand, false);
            }
            DeckManager.Instance.DiscradAllCards(dealer.dealerHand);
            Destroy(dealer.gameObject);
        }
        DeckManager.Instance.ReshuffleDiscardPile();
        GameStateManager.Instance.MoveToNextGameState(GameState.Shop);
    }
    public void InitializeEnemies()
    {
        int randomComposition = Random.Range(0, GameStateManager.Instance.currentFloor.dealerCompositions.Count);
        foreach (var dealer in GameStateManager.Instance.currentFloor.dealerCompositions[randomComposition].dealers)
        {
            GameObject dealerInstance = Instantiate(dealerPrefab, dealerEntities);
            dealerInstance.gameObject.GetComponent<Dealer>().Initialize(dealer);
            dealers.Add(dealerInstance.gameObject.GetComponent<Dealer>());
        }
    }
    public void InitializeBoss()
    {
        GameObject dealerInstance = Instantiate(dealerPrefab, dealerEntities);
        dealerInstance.gameObject.GetComponent<Dealer>().Initialize(GameStateManager.Instance.currentFloor.bossDealer);
        dealers.Add(dealerInstance.gameObject.GetComponent<Dealer>());
    }
    public void InitializeStartingHands()
    {
        for (int i = 0; i < 2; i++)
        {
            PlayerManager.Instance.PlayerHit();
        }
    }

    private static void ApplyBattleStartShield()
    {
        int amount = PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveBattleStartShield);
        if (amount > 0 && PlayerManager.Instance != null)
            PlayerManager.Instance.AddShield(amount);
    }

    private static void ApplyWinningHandCardEffects(bool playerWon, Dealer dealer, List<GameObject> winningHand)
    {
        if (winningHand == null)
            return;
        foreach (GameObject cardGo in winningHand)
        {
            CardData cardData = cardGo.GetComponent<CardData>();
            if (cardData == null || cardData.data == null)
                continue;
            List<EffectStruct> effects = cardData.data.onWinEffects;
            if (effects == null || effects.Count == 0)
                continue;
            var ctx = new EffectWinContext(cardData.data, playerWon, dealer);
            foreach (EffectStruct effect in effects)
                effect.ApplyWinEffect(in ctx);
        }
    }
}
