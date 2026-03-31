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
        currentBattleNumber += 1;
        if (currentBattleNumber > maxBattlesCount)
        {
            StartBossBattle();
        }
        else 
        { 
            InitializeEnemies();
            InitializeStartingHands();
            GameStateManager.Instance.MoveToNextGameState(GameState.BattlePlayerTurn);
        }
    }
    public void StartBossBattle()
    {
        InitializeBoss();
        InitializeStartingHands();
        GameStateManager.Instance.MoveToNextGameState(GameState.BattlePlayerTurn);
    }
    public void EnemyTurn()
    {
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
    public void BattleResults()
    {
        foreach (var dealer in dealers)
        {
            if (dealer.dealerHealth > 0)
            {                
                if (PlayerManager.Instance.currentHandValue <= 21 && dealer.currentHandValue > 21)
                {
                    Debug.Log($"Damage dealt to enemy: {PlayerManager.Instance.currentHandValue}");
                    dealer.TakeDamage(PlayerManager.Instance.currentHandValue);
                }
                else if (PlayerManager.Instance.currentHandValue > 21 && dealer.currentHandValue <= 21)
                {
                    Debug.Log($"Damage dealt to player: {dealer.currentHandValue}");
                    PlayerManager.Instance.TakeDamage(dealer.currentHandValue);
                }
                else if (PlayerManager.Instance.currentHandValue > dealer.currentHandValue)
                {
                    Debug.Log($"Damage dealt to enemy: {PlayerManager.Instance.currentHandValue}");
                    dealer.TakeDamage(PlayerManager.Instance.currentHandValue);
                }
                else if (PlayerManager.Instance.currentHandValue < dealer.currentHandValue)
                {
                    Debug.Log($"Damage dealt to player: {dealer.currentHandValue}");
                    PlayerManager.Instance.TakeDamage(dealer.currentHandValue);
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
}
