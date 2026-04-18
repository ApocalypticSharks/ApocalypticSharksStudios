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
        currentHandValue = CalculateHandValue();
        handValueText.text = currentHandValue.ToString();
    }
    public void DrawFromDealerDeck()
    {
        DeckManager.Instance.ShuffleDeck(dealerDeck);
        for (int i = 0; i < 2; i++)
        {
            Debug.Log("Dealer card drawn");
            CardSO card = dealerDeck[i];
            GameObject cardInstance = Instantiate(DeckManager.Instance.cardPrefab, delaerHandContainer);
            cardInstance.gameObject.GetComponent<CardData>().data = card;
            card.onPlayEffects.ForEach((effect) => effect.ApplyEffect(GameStateManager.Instance.currentGameState));
            dealerHand.Add(cardInstance);
        }
        currentHandValue = CalculateHandValue();
        handValueText.text = currentHandValue.ToString();
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
            if (card.GetComponent<CardData>().data.rank == CardRank.Ace)
            {
                aceCount++;
                totalValue += 1; // ������� ������� ���� ��� 1
            }
            else
            {
                totalValue += card.GetComponent<CardData>().data.baseValue;
            }
        }

        // �������� ������������ ���� ��� 11, ���� ��� �������
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
