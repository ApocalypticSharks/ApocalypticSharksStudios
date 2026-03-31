using UnityEngine;

[CreateAssetMenu(fileName = "New Wheel Segment", menuName = "Blackjack Rogue/Wheel Segment")]
public class WheelSegmentData : ScriptableObject
{
    [Header("Visuals")]
    public string segmentName;
    public Sprite icon;
    public Color segmentColor = Color.white;
    [TextArea(2, 3)]
    public string description;

    [Header("Segment Type")]
    public SegmentType type;
    public Rarity rarity = Rarity.Common;

    [Header("Effects")]
    public int weight = 100; // Вероятность выпадения (1-100)
    public bool isRepeatable = true; // Может выпадать несколько раз

    [Header("Battle Segments")]
    public int difficultyModifier = 0; // Модификатор сложности

    [Header("Reward Segments")]
    public CardRewardType rewardType;
    public int rewardAmount = 1;

    [Header("Special Segments")]
    public SpecialEvent specialEvent;
    public bool isCursedSegment = false; // Проклятый сектор
    public bool isBlessedSegment = false; // Благословенный сектор
}

public enum SegmentType
{
    Battle,         // Бой с дилером
    EliteBattle,    // Бой с элитным дилером
    BossBattle,     // Босс
    Shop,           // Магазин
    Treasure,       // Сундук с наградой
    RestSite,       // Место отдыха (лечение, улучшения)
    Event,          // Случайное событие
    Curse,          // Проклятие
    Blessing,       // Благословение
    Gamble,         // Азартная игра (дополнительная рулетка)
    Mystery         // Тайный сектор
}

public enum CardRewardType
{
    RandomCard,
    CurseCard,
    BlessingCard,
    GambitCard,
    RemoveCard,
    UpgradeCard
}

public enum SpecialEvent
{
    None,
    DoubleOrNothing,    // Удвоить ставку или проиграть всё
    MatchstickFountain, // Фонтан спичек
    DeckPurification,   // Очистка колоды от проклятий
    WheelOfFortune,     // Колесо фортуны (мини-игра)
    TimeWarp,           // Пропуск хода дилера
    MirrorMatch         // Бой против своей колоды
}