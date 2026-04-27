using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dealer : MonoBehaviour
{
    public DealerSO dealerData;
    public List<GameObject> dealerHand = new();
    public int currentHandValue;
    public Transform delaerHandContainer;
    public List<CardSO> dealerDeck;
    public int dealerHealth;
    public int shield;
    public int poisonStacks;
    public bool isActive;
    [Header("UI")]
    public TMP_Text handValueText;

    [Header("Behavior")]
    [SerializeField] private int standValue = 17;

    private const int HoleCardIndex = 1;
    private bool holeCardRevealed;

    // �������
    public System.Action OnDealerTurnStart;
    public System.Action OnDealerTurnEnd;
    public System.Action<bool> OnDealerHandChanged;
    public void Initialize(DealerSO dealerData)
    {
        this.dealerData = dealerData;
        dealerHealth = dealerData.dealerHealth;
        shield = 0;
        poisonStacks = 0;
        gameObject.GetComponent<Image>().sprite = dealerData.sprite;
        dealerDeck = dealerData.dealerDeck;
        DrawFromDealerDeck();
    }

    public void DrawCard()
    {
        DeckManager.Instance.DrawFromGameDeck(dealerHand, delaerHandContainer);
        RefreshHandValueUI();
    }

    public void DrawFromDealerDeck()
    {
        holeCardRevealed = false;
        DeckManager.Instance.ShuffleDeck(dealerDeck);
        for (int i = 0; i < 2; i++)
        {
            Debug.Log("Dealer card drawn");
            CardSO card = dealerDeck[i];
            GameObject cardInstance = Instantiate(DeckManager.Instance.cardPrefab, delaerHandContainer);
            CardData cardData = cardInstance.GetComponent<CardData>();
            cardData.data = card;
            cardData.Initialize();
            if (i == HoleCardIndex)
                cardData.SetFaceUp(false);
            card.onPlayEffects.ForEach((effect) => effect.ApplyEffect(GameStateManager.Instance.currentGameState));
            dealerHand.Add(cardInstance);
        }
        RefreshHandValueUI();
    }

    /// <summary>Flip the hole card (e.g. before dealer plays or at showdown). Safe to call multiple times.</summary>
    public void RevealHoleCard()
    {
        if (holeCardRevealed || dealerHand.Count <= HoleCardIndex)
            return;
        CardData hole = dealerHand[HoleCardIndex].GetComponent<CardData>();
        if (hole != null)
            hole.SetFaceUp(true);
        holeCardRevealed = true;
        RefreshHandValueUI();
    }

    public void RefreshHandValueUI()
    {
        currentHandValue = CalculateHandValue();
        if (handValueText == null)
            return;
        if (!holeCardRevealed && dealerHand.Count >= 2)
            handValueText.text = $"Значение руки {CalculateVisibleHandValue()} + ?";
        else
            handValueText.text = currentHandValue.ToString();
    }

    /// <summary>Blackjack total of face-up cards only (hidden hole card excluded).</summary>
    private int CalculateVisibleHandValue()
    {
        int totalValue = 0;
        int aceCount = 0;

        foreach (GameObject card in dealerHand)
        {
            CardData cd = card.GetComponent<CardData>();
            if (cd == null || !cd.IsFaceUp || cd.data == null)
                continue;

            if (cd.data.rank == CardRank.Ace)
            {
                aceCount++;
                totalValue += PassiveUpgradeBonuses.GetBlackjackCardContribution(cd.data, forPlayerHand: false);
            }
            else
            {
                totalValue += PassiveUpgradeBonuses.GetBlackjackCardContribution(cd.data, forPlayerHand: false);
            }
        }

        while (aceCount > 0 && totalValue + 10 <= 21)
        {
            totalValue += 10;
            aceCount--;
        }

        return totalValue;
    }
    public bool ShouldHit()
    {
        int handValue = CalculateHandValue();

        // ������� �������: �������� �� 17
        if (handValue < standValue) return true;

        // ������ 17 (��� + 6 = 17, �� ����� ������� ��� 7)
        //if (handValue == standValue && HasSoftSeventeen())
        //{
        //    return beforeDrawCardRules.Any(rule => rule.ruleType == RuleType.MustHitOnSoft17);
        //}

        return false;
    }

    public int CalculateHandValue()
    {
        int totalValue = 0;
        int aceCount = 0;

        foreach (GameObject card in dealerHand)
        {
            CardSO d = card.GetComponent<CardData>().data;
            if (d.rank == CardRank.Ace)
            {
                aceCount++;
                totalValue += PassiveUpgradeBonuses.GetBlackjackCardContribution(d, forPlayerHand: false);
            }
            else
            {
                totalValue += PassiveUpgradeBonuses.GetBlackjackCardContribution(d, forPlayerHand: false);
            }
        }

        while (aceCount > 0 && totalValue + 10 <= 21)
        {
            totalValue += 10;
            aceCount--;
        }

        return totalValue;
    }

    public List<GameObject> GetDealerHand() => dealerHand;

    // ��������� ����� (����� ����� ���������)
    public void TakeDamage(int damage, bool ignoreShield = false)
    {
        if (!ignoreShield && shield > 0)
        {
            int absorbed = Mathf.Min(shield, damage);
            shield -= absorbed;
            damage -= absorbed;
        }
        dealerHealth -= damage;
        dealerHealth = Mathf.Max(dealerHealth, 0);

        if (dealerHealth <= 0)
        {
            Debug.Log($"Dealer {dealerData.dealerName} defeated!");
            // ����� ��������, ������� � ����������
        }
    }

    // ���������� ����������� ������
    private int ApplySpecialRules(int currentScore, List<DealerRule> rules)
    {
        foreach (var rule in rules)
        {
            currentScore = rule.ApplyRule(currentScore, dealerHand);
        }
        return currentScore;
    }

    public void AddShield(int amount)
    {
        shield += Mathf.Max(0, amount);
    }

    public void HealDamage(int amount)
    {
        dealerHealth += Mathf.Max(0, amount);
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

    private bool HasSoftSeventeen()
    {
        int handValue = 0;
        bool hasAce = false;

        foreach (var card in dealerHand)
        {
            if (card.GetComponent<CardData>().data.isAce) hasAce = true;
            handValue += card.GetComponent<CardData>().data.GetValue(false); // ������� ���� ��� 1
        }

        return hasAce && handValue == 7;
    }
}

// ������� ������
[System.Serializable]
public class DealerRule
{
    public string ruleName;
    public RuleType ruleType;
    public int value;

    public int ApplyRule(int currentScore, List<GameObject> hand)
    {
        switch (ruleType)
        {
            case RuleType.StandAtValue:
                return currentScore; // �������� standValue ����� ����������

            case RuleType.IgnoreAceHigh:
                // ������� ���� ������ ��� 1
                int newScore = 0;
                foreach (var card in hand)
                {
                    if (card.GetComponent<CardData>().data.rank == CardRank.Ace)
                        newScore += 1;
                    else
                        newScore += card.GetComponent<CardData>().data.baseValue;
                }
                return newScore;

                // ... ������ �������
        }
        return currentScore;
    }
}

public enum RuleType
{
    StandAtValue,    // ��������������� �� ������������ ��������
    IgnoreAceHigh,   // �� ������� ���� ��� 11
    MustHitOnSoft17, // �������� �� "������" 17
    DoubleOnAny,     // ��������� �� ����� �����
    NoSplit,         // �� ��������� �����
}
