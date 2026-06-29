using System;
using NotSoWild.Core;
using UnityEngine;

namespace NotSoWild.Gameplay
{
    [Serializable]
    public struct WorkBonusEntry
    {
        public WorkRole Role;
        [Tooltip("Extra build progress or building output when staffed.")]
        public int Bonus;
    }

    [CreateAssetMenu(fileName = "Trait", menuName = "Not So Wild/Trait")]
    public sealed class TraitDefinition : ScriptableObject
    {
        public string DisplayName = "Trait";
        [TextArea] public string Description = "+0";
        public bool CannotBuild;
        public WorkBonusEntry[] WorkBonuses;
        [Tooltip("Adds to resident combat power during raids.")]
        public int DefenseBonus;
        [Tooltip("Adds to resident shooting accuracy during raids.")]
        public int AccuracyBonus;
        [Tooltip("Adds to resident attack range during raids.")]
        public float AttackRangeBonus;
        [Tooltip("Multiplies attack cooldown during raids. Lower is faster; 1 keeps the default.")]
        public float AttackCooldownMultiplier = 1f;
        [Tooltip("Extra targets fired at per attack during raids.")]
        public int ExtraTargets;
        [Tooltip("Accuracy penalty applied when this trait fires at extra targets.")]
        public int MultiTargetAccuracyPenalty;
        [Tooltip("Adds accuracy based on target distance during raids.")]
        public bool AccuracyScalesWithDistance;
        [Tooltip("Adds to close-range damage during raids.")]
        public int MeleeDamageBonus;
        public int DailyGoldBonus;
        public int ReputationBonus;
        public int HeatBonus;
        public int RaidRiskBonus;

        [Header("Resident Stats")]
        public int StartingHealthBonus;
        public int StartingMoodBonus;
        public int StartingStressBonus;
        public int DailyHealthDelta;
        public int DailyMoodDelta;
        public int DailyStressDelta;

        [Header("Special Rules")]
        public bool DoesNotUseResidentCapacity;
        public bool PreventsStressGain;
        public float RaidStressMultiplier = 1f;
        public int HospitalWorkBonus;
        public bool CannotWorkAtChurch;
        public int StressWithoutChurch;
        public float ChurchEffectMultiplier = 1f;
    }
}
