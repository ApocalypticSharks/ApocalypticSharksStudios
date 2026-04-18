using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public Transform playerHandContainer;
    public int playerHealth;
    public int gold;

    [Header("UI")]
    public TMP_Text handValue;

    [Header("Buttons")]
    public Button hitButton;
    public Button standButton;
    public Button startNewBattle;

    public List<GameObject> playerHand;
    public int currentHandValue;

    public int shield;
    public int poisonStacks;

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
        hitButton.onClick.AddListener(() => PlayerHit());
        standButton.onClick.AddListener(() => PlayerStand());
        startNewBattle.onClick.AddListener(() => BattleManager.Instance.StartNewBattle());
    }

    public void TakeDamage(int damage, bool ignoreShield = false)
    {
        if (!ignoreShield && shield > 0)
        {
            int absorbed = Mathf.Min(shield, damage);
            shield -= absorbed;
            damage -= absorbed;
        }
        playerHealth -= damage;
    }

    public void AddShield(int amount)
    {
        shield += Mathf.Max(0, amount);
    }

    public void AddPoison(int amount)
    {
        poisonStacks += Mathf.Max(0, amount);
    }

    public void TickPoisonAtRoundStart()
    {
        if (poisonStacks <= 0)
            return;
        int dmg = poisonStacks;
        poisonStacks = Mathf.Max(0, poisonStacks - 1);
        TakeDamage(dmg, ignoreShield: false);
    }

    public void HealDamage(int amount)
    {
        playerHealth += amount;
    }

    public void GetGold(int amount)
    {
        if (amount <= 0)
        {
            gold += amount;
            UIManager.Instance.ChangeGoldValue();
            return;
        }

        int bonusPct = PassiveUpgradeBonuses.GetTotalGoldIncomeBonusPercent();
        int gained = bonusPct > 0
            ? Mathf.CeilToInt(amount * (100f + bonusPct) / 100f)
            : amount;
        gold += gained;
        UIManager.Instance.ChangeGoldValue();
    }

    public void SpendGold(int amount)
    {
        gold -= amount;
        UIManager.Instance.ChangeGoldValue();
    }
    public void PlayerHit()
    {
        DeckManager.Instance.DrawFromGameDeck(playerHand, playerHandContainer);
        currentHandValue = CalculateHandValue();
        handValue.text = currentHandValue.ToString();
        if (currentHandValue == 21)
        {
            if (playerHand.Count == 2)
            {
                GameStateManager.Instance.MoveToNextGameState(GameState.BattleResults);
            }
            else
            {
                PlayerStand();
            }
        }
        isBust(currentHandValue);
    }

    public void PlayerStand()
    {
        GameStateManager.Instance.MoveToNextGameState(GameState.BattleEnemyTurn);
    }

    public int CalculateHandValue()
    {
        int totalValue = 0;
        int aceCount = 0;

        foreach (GameObject card in playerHand)
        {
            if (card.GetComponent<CardData>().data.rank == CardRank.Ace)
            {
                aceCount++;
                totalValue += 1; // ??????? ??????? ???? ??? 1
            }
            else
            {
                totalValue += card.GetComponent<CardData>().data.baseValue;
            }
        }

        // ???????? ???????????? ???? ??? 11, ???? ??? ???????
        while (aceCount > 0 && totalValue + 10 <= 21)
        {
            totalValue += 10;
            aceCount--;
        }

        return totalValue;
    }

    public void isBust(int handValue)
    {
        if (handValue > 21)
        {
            foreach (var upgrade in GameStateManager.Instance.upgrades)
            {
                foreach (var effect in upgrade.data.onBustEffects)
                {
                    effect.ApplyEffect(GameStateManager.Instance.currentGameState);
                }
            }
            GameStateManager.Instance.MoveToNextGameState (GameState.BattleResults);
        }
    }
}
