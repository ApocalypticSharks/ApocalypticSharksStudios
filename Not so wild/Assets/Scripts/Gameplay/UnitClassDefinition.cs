using System;
using UnityEngine;

namespace NotSoWild.Gameplay
{
    public enum UnitFaction
    {
        Settlers = 0,
        Bandits = 1
    }

    public enum UnitAttackKind
    {
        Unarmed = 0,
        Melee = 1,
        Pistol = 2,
        Revolver = 3,
        Thrown = 4
    }

    [Flags]
    public enum UnitAbilityFlags
    {
        None = 0,
        MoodOnKill = 1 << 0,
        TauntNearbyEnemies = 1 << 1,
        HealMostWoundedAlly = 1 << 2,
        PreventNearbyStress = 1 << 3,
        SummonDog = 1 << 4,
        DoesNotUseResidentCapacity = 1 << 5,
        TargetLowestHealthIncludingAllies = 1 << 6,
        AreaDamageIncludingAllies = 1 << 7,
        RicochetIncludingAllies = 1 << 8,
        ConeDamageIncludingAllies = 1 << 9,
        HasteDamagedAllies = 1 << 10,
        ExecuteWeakAlly = 1 << 11
    }

    [CreateAssetMenu(fileName = "UnitClass", menuName = "Not So Wild/Unit Class")]
    public sealed class UnitClassDefinition : ScriptableObject
    {
        public string DisplayName = "Unit";
        public UnitFaction Faction = UnitFaction.Settlers;
        [Min(1)] public int Tier = 1;
        public UnitAttackKind AttackKind = UnitAttackKind.Unarmed;
        public UnitAbilityFlags Abilities;

        [Header("Combat")]
        [Min(1)] public int MaxHp = 5;
        [Min(1)] public int Attack = 1;
        [Range(5, 95)] public int Accuracy = 60;
        [Min(0.1f)] public float MoveSpeed = 2.5f;
        [Min(0.2f)] public float AttackRange = 0.75f;
        [Min(0.1f)] public float AttackCooldown = 1.1f;
        [Min(1)] public int TargetCount = 1;
        [Min(0)] public int MultiTargetAccuracyPenalty;

        [Header("Ability Tuning")]
        [Min(0.1f)] public float TauntRadius = 1.4f;
        [Min(0.1f)] public float HealRange = 0.8f;
        [Min(1)] public int HealAmount = 2;
        [Min(0.1f)] public float AreaRadius = 0.75f;
        [Min(0)] public int RicochetCount;
        [Range(0.1f, 1f)] public float RicochetDamageMultiplier = 0.75f;
        [Min(0.1f)] public float ConeRange = 1.1f;
        [Range(1f, 180f)] public float ConeAngle = 70f;
        [Range(0.1f, 1f)] public float AllyHasteMultiplier = 0.75f;
        [Min(0.1f)] public float AllyHasteSeconds = 4f;
        [Range(0.05f, 1f)] public float ExecuteHealthThreshold = 0.25f;
        [Range(0.1f, 1f)] public float ExecuteBuffMultiplier = 0.75f;
        [Min(0.1f)] public float ExecuteBuffSeconds = 5f;

        public bool HasAbility(UnitAbilityFlags ability)
        {
            return (Abilities & ability) != 0;
        }
    }
}
