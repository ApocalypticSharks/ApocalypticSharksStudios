using UnityEngine;

/// <summary>
/// Tracks lightweight card-type/suit combo state for player and enemy during a battle round.
/// </summary>
public static class ComboEngine
{
    private const int MaxComboPercentBonus = 80;
    private const int MaxSuitTransitionStreak = 5;

    private struct ComboState
    {
        public CardSO LastCard;
        public int TypeTransitionStreak;
        public int SuitTransitionStreak;
        public bool LastTransitionBridgedTypes;
    }

    private static ComboState _player;
    private static ComboState _enemy;

    public static CardType? ExpectedNextType => ExpectedTypeFor(true);
    public static int TypeTransitionStreak => _player.TypeTransitionStreak;
    public static int SuitTransitionStreak => _player.SuitTransitionStreak;
    public static int EnemyTypeTransitionStreak => _enemy.TypeTransitionStreak;
    public static int EnemySuitTransitionStreak => _enemy.SuitTransitionStreak;

    public static void ResetRoundState()
    {
        _player = default;
        _enemy = default;
        UIManager.Instance?.UpdateComboHud(_player.TypeTransitionStreak, _player.SuitTransitionStreak, ExpectedNextType);
    }

    public static void RegisterCardResolved(CardSO card, bool playerSide)
    {
        if (card == null)
            return;

        ref ComboState state = ref (playerSide ? ref _player : ref _enemy);
        bool brokeTypeCombo = false;
        state.LastTransitionBridgedTypes = false;

        if (state.LastCard != null)
        {
            state.LastTransitionBridgedTypes = state.LastCard.cardType != card.cardType;
            if (state.LastTransitionBridgedTypes)
                state.TypeTransitionStreak += 1;
            else
            {
                brokeTypeCombo = state.TypeTransitionStreak >= 2;
                state.TypeTransitionStreak = 0;
            }

            if (state.LastCard.suit != card.suit)
                state.SuitTransitionStreak = Mathf.Min(MaxSuitTransitionStreak, state.SuitTransitionStreak + 1);
            else
                state.SuitTransitionStreak = 0;
        }
        else
        {
            state.TypeTransitionStreak = 0;
            state.SuitTransitionStreak = 0;
        }

        state.LastCard = card;

        if (playerSide && brokeTypeCombo)
            TryApplySafetyValve(card);

        if (playerSide)
            UIManager.Instance?.UpdateComboHud(_player.TypeTransitionStreak, _player.SuitTransitionStreak, ExpectedNextType);
    }

    public static int GetCurrentComboPercentBonus(bool playerSide)
    {
        ref ComboState state = ref (playerSide ? ref _player : ref _enemy);
        int pct = 0;
        if (playerSide && state.LastTransitionBridgedTypes)
            pct += PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveBridgeCapacitorComboPercent);
        int perStep = playerSide ? PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveSuitResonatorComboPercentPerStep) : 0;
        if (perStep > 0 && state.SuitTransitionStreak > 0)
            pct += perStep * state.SuitTransitionStreak;
        return Mathf.Clamp(pct, 0, MaxComboPercentBonus);
    }

    public static bool LastTransitionBridgedTypesFor(bool playerSide)
    {
        return playerSide ? _player.LastTransitionBridgedTypes : _enemy.LastTransitionBridgedTypes;
    }

    private static CardType? ExpectedTypeFor(bool playerSide)
    {
        CardSO last = playerSide ? _player.LastCard : _enemy.LastCard;
        if (last == null)
            return null;
        return last.cardType == CardType.Standard ? CardType.Action : CardType.Standard;
    }

    private static void TryApplySafetyValve(CardSO currentCard)
    {
        int value = PassiveUpgradeBonuses.SumPassiveValue(EffectType.UpgradePassiveSafetyValveOnComboBreak);
        if (value <= 0 || PlayerManager.Instance == null)
            return;

        PlayerManager.Instance.AddShield(value);
        if (currentCard != null && currentCard.suit == CardSuit.Diamonds)
            PlayerManager.Instance.GetGold(value);
    }
}
