// GameManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Unity.Mathematics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //public static GameManager Instance { get; private set; }

    //[Header("Game State")]
    //public GameState initialGameState = GameState.Shop;
    //public GameState currentGameState = GameState.Menu;
    //public BattleState currentBattleState;
    //public BattlePhase currentBattlePhase;

    //[Header("Act Structure")]
    //public List<ActSO> acts = new();
    //private ActSO currentAct;

    //[Header("Player Stats")]
    //public int playerCredits = 100;
    //public int playerMatchsticks = 3;
    //public int currentBet = 5;
    //public float damageModifier = 1f;

    //[Header("Wheel System")]
    //public WheelSegmentData currentSelectedSegment;
    //public FloorSO currentFloor;
    //public int floorsPerAct = 4;

    //[Header("References")]
    //public DeckManager deckManager;
    //public UIManager uiManager;
    //public DealerAI dealerAI;

    //// События
    //public event Action<int> OnCreditsChanged;
    //public event Action<int> OnMatchsticksChanged;
    //public event Action<GameState> OnGameStateChanged;

    //private void Awake()
    //{
    //    // Singleton pattern
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //        DontDestroyOnLoad(gameObject);
    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //    }
    //}

    //private void Start()
    //{
    //    InitializeGame();
    //}

    //public void InitializeGame()
    //{
    //    playerCredits = 100;
    //    playerMatchsticks = 3;
    //    currentBet = 5;
        
    //    currentAct = acts[0];
    //    currentFloor = currentAct.floors[0];

    //    ChangeGameState(initialGameState);
    //}

    //// Начать новый бой
    //public async Task StartNewBattle()
    //{
    //    await dealerAI.Initialize(currentFloor.dealerCompositions);
    //    DiscardBothHands();
    //    deckManager.ReshuffleDiscardPile();
    //    // Раздача начальных карт
    //    for (int i = 0; i < 2; i++)
    //    {
    //        deckManager.PlayerDrawCard();
    //        dealerAI.DrawInitialCards();
    //    }

    //    ChangeBattlePhase(BattlePhase.PlayerTurn);

    //    uiManager.UpdateBattleUI();
    //}

    //public void StartNewBattleRound()
    //{
    //    DiscardBothHands();
    //    for (int i = 0; i < 2; i++)
    //    {
    //        deckManager.PlayerDrawCard();
    //        dealerAI.DrawInitialCards();
    //    }

    //    ChangeBattlePhase(BattlePhase.PlayerTurn);

    //    uiManager.UpdateBattleUI();
    //}

    //// Игрок берет карту
    //public void PlayerHit()
    //{
    //    if (currentBattlePhase != BattlePhase.PlayerTurn) return;

    //    deckManager.PlayerDrawCard();
    //    int handValue = deckManager.CalculateHandValue(deckManager.GetPlayerHand());

    //    if (handValue > 21)
    //    {
    //        PlayerBust();
    //    }
    //    else if (handValue == 21)
    //    {
    //        // Автоматически переходим к дилеру
    //        PlayerStand();
    //    }

    //    uiManager.UpdateBattleUI();
    //}

    //// Игрок останавливается
    //public void PlayerStand()
    //{
    //    ChangeBattlePhase(BattlePhase.DealerTurn);
    //    StartCoroutine(DealerTurnRoutine());
    //}

    //private IEnumerator DealerTurnRoutine()
    //{
    //    yield return new WaitForSeconds(0.5f);
    //    foreach(Dealer dealer in dealerAI.dealers)
    //    {
    //        dealerAI.currentDealer = dealer;
    //        // Дилер берет карты по правилам
    //        while (dealer.ShouldHit())
    //        {
    //            dealer.DrawCard(deckManager.DrawFromGameDeck());
    //            yield return new WaitForSeconds(1f);
    //        }
    //    }
    //    ChangeBattlePhase(BattlePhase.Result);
    //    DetermineWinner();
    //}

    //// Игрок перебирает
    //private void PlayerBust()
    //{
    //    Debug.Log("Player busts!");

    //    // Проигрыш ставки
    //    ChangePlayerCredits(-currentBet);
    //    uiManager.ShowBattleResult("Bust! You lose " + currentBet + " credits.");
    //}

    //// Определение победителя
    //private void DetermineWinner()
    //{
    //    foreach (var dealer in dealerAI.dealers)
    //    {
    //        int playerScore = deckManager.CalculateHandValue(deckManager.GetPlayerHand());
    //        int dealerScore = deckManager.CalculateHandValue(dealer.GetDealerHand());

    //        bool playerBust = deckManager.IsBust(deckManager.GetPlayerHand());
    //        bool dealerBust = deckManager.IsBust(dealer.GetDealerHand());

    //        if (playerBust)
    //        {
    //            // Уже обработано в PlayerBust
    //            return;
    //        }

    //        if (dealerBust || playerScore > dealerScore)
    //        {
    //            PlayerWin(playerScore, dealerScore, dealer);
    //        }
    //        else if (playerScore < dealerScore)
    //        {
    //            PlayerLose(playerScore, dealerScore, dealer);
    //        }
    //        else
    //        {
    //            // Ничья
    //            Draw(playerScore);
    //        }
    //    }
    //}

    //public void ChangePlayerCredits(int amount)
    //{
    //    playerCredits += amount;
    //    OnCreditsChanged?.Invoke(playerCredits);
    //}

    //private void PlayerWin(int playerScore, int dealerScore, Dealer dealer)
    //{
    //    int winAmount = currentBet;
    //    ChangePlayerCredits(winAmount);
    //    int damageToDeal = (int)Math.Round(deckManager.CalculateHandValue(deckManager.GetPlayerHand()) * damageModifier);
    //    dealer.TakeDamage(damageToDeal);
    //    uiManager.ShowBattleResult($"Win! {playerScore} vs {dealerScore}. +{winAmount} credits");

    //    // Эффекты при победе
    //    ApplyWinEffects();
    //}

    //private void PlayerLose(int playerScore, int dealerScore, Dealer dealer)
    //{
    //    ChangePlayerCredits(-deckManager.CalculateHandValue(dealer.GetDealerHand()));
    //    uiManager.ShowBattleResult($"Lose! {playerScore} vs {dealerScore}. -{currentBet} credits");
    //}

    //private void Draw(int score)
    //{
    //    uiManager.ShowBattleResult($"Push! {score} vs {score}. Bet returned");
    //    // Ставка возвращается, ничего не меняется
    //}

    //public bool SpendMatchstick(int burnCost)
    //{
    //        playerMatchsticks -= burnCost;
    //        OnMatchsticksChanged?.Invoke(playerMatchsticks);
    //        return true;
    //}
}