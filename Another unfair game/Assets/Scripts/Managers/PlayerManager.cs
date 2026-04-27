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
    [Header("Matchsticks")]
    [Tooltip("Сжигание карт ПКМ в бою")]
    public int matchsticks = 3;

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

    /// <summary>Bonus added to each physical damage packet this blackjack round (Greed upgrade).</summary>
    public int GreedPhysicalDamageBonusThisRound { get; private set; }

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

    public void TakeDamage(int damage, bool ignoreShield = false, Dealer attackingDealer = null)
    {
        int initialHit = Mathf.Max(0, damage);
        int incoming = initialHit;
        int shieldBefore = shield;

        if (!ignoreShield && shield > 0 && incoming > 0)
        {
            int absorbed = Mathf.Min(shield, incoming);
            shield -= absorbed;
            incoming -= absorbed;
        }

        playerHealth -= incoming;

        if (!ignoreShield && shieldBefore > 0 && shield == 0)
            PassiveUpgradeBonuses.OnPlayerShieldBrokenShieldShards();

        if (!ignoreShield && attackingDealer != null && attackingDealer.dealerHealth > 0 && initialHit > 0)
        {
            int thorn = PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveReflectDamageWhenHit);
            if (thorn > 0)
            {
                attackingDealer.TakeDamage(thorn, ignoreShield: false);
                PassiveUpgradeBonuses.OnPlayerDealtDamageToDealer(attackingDealer, thorn);
            }
        }
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

    public bool TrySpendMatchsticks(int amount)
    {
        if (amount <= 0 || matchsticks < amount)
            return false;
        matchsticks -= amount;
        UIManager.Instance?.ChangeMatchsticksValue();
        return true;
    }

    public void PlayerHit()
    {
        DeckManager.Instance.DrawFromGameDeck(playerHand, playerHandContainer);
        RefreshHandStateAfterModify();
    }

    /// <summary>После добора или сброса карты: пересчёт очков, 21 / буст.</summary>
    public void RefreshHandStateAfterModify()
    {
        currentHandValue = CalculateHandValue();
        handValue.text = currentHandValue.ToString();
        if (currentHandValue == 21)
        {
            if (playerHand.Count == 2)
            {
                BattleManager.Instance?.RevealAllDealerHoleCards();
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

    public void ClearGreedRoundBonus()
    {
        GreedPhysicalDamageBonusThisRound = 0;
    }

    public void RegisterCoinCardPlayedForGreed(CardSO card)
    {
        if (card == null || PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveGreedCoinCardPhysicalDamage) <= 0)
            return;
        if (!card.CountsAsCoinCardForGreed())
            return;
        GreedPhysicalDamageBonusThisRound += Mathf.Max(1, card.baseValue);
    }

    public int CalculateHandValue()
    {
        int totalValue = 0;
        int aceCount = 0;

        foreach (GameObject card in playerHand)
        {
            CardSO data = card.GetComponent<CardData>().data;
            if (data.rank == CardRank.Ace)
            {
                aceCount++;
                totalValue += PassiveUpgradeBonuses.GetBlackjackCardContribution(data, forPlayerHand: true);
            }
            else
            {
                totalValue += PassiveUpgradeBonuses.GetBlackjackCardContribution(data, forPlayerHand: true);
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
                if (upgrade?.data?.onBustEffects == null)
                    continue;
                int lv = upgrade.EffectiveLevel;
                foreach (var effect in upgrade.data.onBustEffects)
                {
                    EffectStruct scaled = effect;
                    scaled.value = UpgradeData.ScaledEffectValue(effect.value, lv);
                    scaled.ApplyEffect(GameStateManager.Instance.currentGameState);
                }
            }
            BattleManager.Instance?.RevealAllDealerHoleCards();
            GameStateManager.Instance.MoveToNextGameState (GameState.BattleResults);
        }
    }
}
