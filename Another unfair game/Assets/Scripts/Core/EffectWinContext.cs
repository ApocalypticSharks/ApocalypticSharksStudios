/// <summary>
/// Context for card <see cref="CardSO.onWinEffects"/> when the owning hand wins a showdown.
/// </summary>
public readonly struct EffectWinContext
{
    public readonly CardSO Card;
    public readonly bool PlayerWon;
    public readonly Dealer OpponentDealer;

    public EffectWinContext(CardSO card, bool playerWon, Dealer opponentDealer)
    {
        Card = card;
        PlayerWon = playerWon;
        OpponentDealer = opponentDealer;
    }
}
