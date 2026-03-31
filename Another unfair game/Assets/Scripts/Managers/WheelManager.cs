using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WheelManager : MonoBehaviour
{
    public static WheelManager Instance { get; private set; }

    [Header("Wheel Segments")]
    [SerializeField]
    private List<WheelSegmentData> segmentDataList;
    [SerializeField]
    private int segmentCount;
    [SerializeField] 
    public List<WheelSegmentData> currentWheelSegments = new();

    // —обыти€
    public Action onWheelSpinned;
    public Action onWheelInitialized;

    public List<int> selectedSegments;
    public WheelUI wheelUI;

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

    public void Initialize()
    {
        if (wheelUI.wheelSegments.All(s => s.GetComponent<WheelDisplay>().isCompleted) || !currentWheelSegments.Any())
        {            
            GenerateWheelSegments();
            onWheelInitialized?.Invoke();
        }
    }

    public void WheelSpin()
    {
        selectedSegments.Clear();
        int randomIndex = GetRandomAvailableSegment();
        int leftIndex = randomIndex - 1 < 0 ? segmentCount - 1 : randomIndex - 1;
        int rightIndex = randomIndex + 1 == segmentCount ? 0 : randomIndex + 1;

        selectedSegments.Add(randomIndex);
        selectedSegments.Add(leftIndex);
        selectedSegments.Add(rightIndex);

        onWheelSpinned?.Invoke();
    }

    public int GetRandomAvailableSegment()
    {
        int randomIndex = UnityEngine.Random.Range(0, segmentCount);
        if (wheelUI.wheelSegments[randomIndex].GetComponent<WheelDisplay>().isCompleted)
        {
            GetRandomAvailableSegment();
        }
        return randomIndex;
    }

    // √енераци€ секторов дл€ текущего уровн€
    public void GenerateWheelSegments()
    {
        currentWheelSegments.Clear();

        // √енерируем сектора на основе весов
        for (int i = 0; i < segmentCount; i++)
        {
            WheelSegmentData segment = GetWeightedRandomSegment();
            currentWheelSegments.Add(segment);
        }

        // √арантируем хот€ бы один боевой сектор и один не боевой
        EnsureSegmentVariety();

        Debug.Log($"Generated wheel with {currentWheelSegments.Count} segments");
    }

    private WheelSegmentData GetWeightedRandomSegment()
    {
        // —обираем все доступные сектора с учетом весов
        List<WheelSegmentData> weightedList = new();

        foreach (var segment in segmentDataList)
        {
            // ѕровер€ем, можно ли повтор€ть
            if (!segment.isRepeatable && currentWheelSegments.Contains(segment))
                continue;

            // ƒобавл€ем сектор столько раз, сколько его вес
            for (int i = 0; i < segment.weight; i++)
            {
                weightedList.Add(segment);
            }
        }

        if (weightedList.Count == 0)
        {
            Debug.LogWarning("No segments available! Adding default battle segment.");
            return CreateDefaultSegment();
        }

        return weightedList[UnityEngine.Random.Range(0, weightedList.Count)];
    }
    private void EnsureSegmentVariety()
    {
        bool hasBattle = false;
        bool hasNonBattle = false;

        foreach (var segment in currentWheelSegments)
        {
            if (segment.type == SegmentType.Battle ||
                segment.type == SegmentType.EliteBattle ||
                segment.type == SegmentType.BossBattle)
            {
                hasBattle = true;
            }
            else
            {
                hasNonBattle = true;
            }

            if (hasBattle && hasNonBattle) break;
        }

        // ≈сли нет боевых секторов - замен€ем случайный
        if (!hasBattle)
        {
            int replaceIndex = UnityEngine.Random.Range(0, currentWheelSegments.Count);
            currentWheelSegments[replaceIndex] = GetSegmentByType(SegmentType.Battle);
        }

        // ≈сли нет не боевых секторов - замен€ем случайный
        if (!hasNonBattle)
        {
            int replaceIndex = UnityEngine.Random.Range(0, currentWheelSegments.Count);
            SegmentType nonBattleType = UnityEngine.Random.value > 0.5f ? SegmentType.Shop : SegmentType.Treasure;
            currentWheelSegments[replaceIndex] = GetSegmentByType(nonBattleType);
        }
    }

    private WheelSegmentData GetSegmentByType(SegmentType type)
    {
        foreach (var segment in segmentDataList)
        {
            if (segment.type == type)
                return segment;
        }

        return CreateDefaultSegment();
    }

    private WheelSegmentData CreateDefaultSegment()
    {
        WheelSegmentData defaultSegment = ScriptableObject.CreateInstance<WheelSegmentData>();
        defaultSegment.segmentName = "Battle";
        defaultSegment.type = SegmentType.Battle;
        defaultSegment.segmentColor = Color.red;
        defaultSegment.weight = 100;
        return defaultSegment;
    }
}
