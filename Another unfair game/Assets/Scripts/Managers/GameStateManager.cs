using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    [Header("Acts and Floors")]
    public List<ActSO> acts;
    public ActSO currentAct;
    public FloorSO currentFloor;

    [Header("Game State")]
    public GameState initialGameState = GameState.Shop;
    public GameState currentGameState = GameState.Menu;

    [Header("Game Modifiers")]
    public List<UpgradeData> upgrades;
    public List<EquipmentData> equipment;

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
    public void MoveToNextGameState(GameState gameState)
    {
        currentGameState = gameState;
        UIManager.Instance.ChangePanel(gameState);
        switch (gameState)
        {
            case GameState.BattleStart:
                ShopManager.Instance.DeinitializeShop();
                BattleManager.Instance.StartNewBattle();
                break;
            case GameState.BattleEnemyTurn:
                BattleManager.Instance.EnemyTurn();
                break;
            case GameState.BattleResults:
                BattleManager.Instance.BattleResults();
                break;
            case GameState.BattleEnd:
                BattleManager.Instance.BattleEnd();
                break;
            case GameState.Shop:
                ShopManager.Instance.PrepareNewShopItems();
                break;
        }
    }

}

public enum GameState
{
    Menu,
    Wheel,
    BattleStart,
    BattlePlayerTurn,
    BattleEnemyTurn,
    BattleResults,
    BattleEnd,
    Shop,
    Treasure,
    RestSite,
    Event,
    ActTransition,
    GameOver
}
