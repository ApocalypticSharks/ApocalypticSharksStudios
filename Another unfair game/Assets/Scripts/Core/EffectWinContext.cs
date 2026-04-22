/// <summary>
/// Context for card <see cref="CardSO.onWinEffects"/> when the owning hand wins a showdown.
/// </summary>
public readonly struct EffectWinContext
{
    public readonly CardSO Card;
    public readonly bool PlayerWon;
    public readonly Dealer OpponentDealer;
    /// <summary>When true, <see cref="EffectType.UpgradePassivePoisonSpreadToOtherEnemiesOnCardPoison"/> does not trigger (e.g. poison applied from enemy drawing a card).</summary>
    public readonly bool SuppressPoisonFlaskSpread;

    public EffectWinContext(CardSO card, bool playerWon, Dealer opponentDealer, bool suppressPoisonFlaskSpread = false)
    {
        Card = card;
        PlayerWon = playerWon;
        OpponentDealer = opponentDealer;
        SuppressPoisonFlaskSpread = suppressPoisonFlaskSpread;
    }

    /// <summary>Poison from deck draw (Poisoned Cards upgrade): poison applies to that dealer with <see cref="SuppressPoisonFlaskSpread"/> set.</summary>
    public static EffectWinContext ForPoisonedCardsDealerDraw(CardSO card, Dealer dealer) =>
        new EffectWinContext(card, playerWon: true, opponentDealer: dealer, suppressPoisonFlaskSpread: true);
}
