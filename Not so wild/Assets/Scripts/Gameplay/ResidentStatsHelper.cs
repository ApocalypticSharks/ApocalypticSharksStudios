using NotSoWild.Core;
using UnityEngine;

namespace NotSoWild.Gameplay
{
    public static class ResidentStatsHelper
    {
        public static void Initialize(ResidentRecord resident)
        {
            if (resident == null)
            {
                return;
            }

            resident.Stats = new ResidentStats(80, 70, 20);
            ApplyTraitStartingBonuses(resident);
            resident.Stats.Clamp();
        }

        public static void ProcessTownTick(ResidentRecord resident, TownState state)
        {
            if (resident?.Stats == null || state == null)
            {
                return;
            }

            var stats = resident.Stats;
            int stressBefore = stats.Stress;

            switch (resident.WorkState)
            {
                case ResidentWorkState.Idle:
                    stats.Apply(0, 2, -3);
                    break;
                case ResidentWorkState.Building:
                    stats.Apply(-1, -1, 5);
                    break;
                case ResidentWorkState.Working:
                    stats.Apply(0, 0, 2);
                    ApplyWorkingRoleEffects(resident, stats);
                    break;
            }

            if (state.Heat > 0)
            {
                stats.Apply(0, -1, state.Heat / 5);
            }

            if (stats.Stress > 70)
            {
                stats.Apply(0, -3, 0);
            }

            if (stats.Mood < 30)
            {
                stats.Apply(0, 0, 2);
            }

            ApplyFaithEffects(resident, state, stats);
            ApplyTraitDailyBonuses(resident, stats);
            PreventStressGainIfNeeded(resident, stats, stressBefore);
            stats.Clamp();
        }

        public static void ApplyRaidAftermath(ResidentRecord resident, bool raidVictory)
        {
            if (resident?.Stats == null)
            {
                return;
            }

            int stressDelta;
            if (raidVictory)
            {
                stressDelta = 6;
                resident.Stats.Apply(-1, -2, ScaleRaidStress(resident, stressDelta));
            }
            else
            {
                stressDelta = 12;
                resident.Stats.Apply(-3, -6, ScaleRaidStress(resident, stressDelta));
            }

            resident.Stats.Clamp();
        }

        public static float GetWorkEfficiency(ResidentRecord resident)
        {
            if (resident?.Stats == null)
            {
                return 1f;
            }

            float moodFactor = UnityEngine.Mathf.Lerp(0.5f, 1f, resident.Stats.Mood / (float)ResidentStats.MaxValue);
            float stressPenalty = UnityEngine.Mathf.Clamp01(
                (resident.Stats.Stress - 40f) / (ResidentStats.MaxValue - 40f));
            float stressFactor = UnityEngine.Mathf.Lerp(1f, 0.6f, stressPenalty);
            return moodFactor * stressFactor;
        }

        public static string GetLeaveReason(ResidentRecord resident)
        {
            if (resident?.Stats == null || resident.Definition == null)
            {
                return "A resident left town.";
            }

            if (resident.Stats.Health <= 0)
            {
                return $"{resident.Definition.DisplayName} left — too weak to stay.";
            }

            return $"{resident.Definition.DisplayName} left town — spirits broken.";
        }

        static void ApplyWorkingRoleEffects(ResidentRecord resident, ResidentStats stats)
        {
            var definition = resident.AssignedBuilding?.Definition;
            var role = definition?.WorkRole;
            switch (role)
            {
                case WorkRole.Saloon:
                    stats.Apply(0, 2, -1);
                    break;
                case WorkRole.Sheriff:
                    stats.Apply(0, -1, 2);
                    break;
                case WorkRole.Store:
                    stats.Apply(0, 1, 0);
                    break;
                case WorkRole.Hospital:
                case WorkRole.Church:
                    stats.Apply(0, 1, -2);
                    break;
                case WorkRole.Armory:
                case WorkRole.Prospector:
                    stats.Apply(0, 0, 1);
                    break;
                case WorkRole.Housing:
                    stats.Apply(0, 2, -2);
                    break;
                default:
                    stats.Apply(0, -1, 0);
                    break;
            }

            if (definition != null)
            {
                int health = definition.StaffDailyHealth;
                int mood = definition.StaffDailyMood;
                int stress = definition.StaffDailyStress;
                if (definition.WorkRole == WorkRole.Hospital)
                {
                    health += GetHospitalWorkBonus(resident);
                }

                if (definition.WorkRole == WorkRole.Church)
                {
                    float multiplier = GetChurchEffectMultiplier(resident);
                    mood = Mathf.RoundToInt(mood * multiplier);
                    stress = Mathf.RoundToInt(stress * multiplier);
                }

                stats.Apply(health, mood, stress);
            }
        }

        static void ApplyFaithEffects(ResidentRecord resident, TownState state, ResidentStats stats)
        {
            if (state == null || state.HasOperationalChurch())
            {
                return;
            }

            if (resident.Definition?.Traits == null)
            {
                return;
            }

            foreach (var trait in resident.Definition.Traits)
            {
                if (trait != null && trait.StressWithoutChurch != 0)
                {
                    stats.Apply(0, 0, trait.StressWithoutChurch);
                }
            }
        }

        static int GetHospitalWorkBonus(ResidentRecord resident)
        {
            int total = 0;
            if (resident.Definition?.Traits == null)
            {
                return total;
            }

            foreach (var trait in resident.Definition.Traits)
            {
                if (trait != null)
                {
                    total += trait.HospitalWorkBonus;
                }
            }

            return total;
        }

        static float GetChurchEffectMultiplier(ResidentRecord resident)
        {
            float total = 1f;
            if (resident.Definition?.Traits == null)
            {
                return total;
            }

            foreach (var trait in resident.Definition.Traits)
            {
                if (trait != null && trait.ChurchEffectMultiplier > 0f)
                {
                    total *= trait.ChurchEffectMultiplier;
                }
            }

            return total;
        }

        static int ScaleRaidStress(ResidentRecord resident, int stressDelta)
        {
            if (PreventsStressGain(resident))
            {
                return Mathf.Min(0, stressDelta);
            }

            float multiplier = 1f;
            if (resident.Definition?.Traits != null)
            {
                foreach (var trait in resident.Definition.Traits)
                {
                    if (trait != null && trait.RaidStressMultiplier > 0f)
                    {
                        multiplier *= trait.RaidStressMultiplier;
                    }
                }
            }

            return Mathf.RoundToInt(stressDelta * multiplier);
        }

        static void PreventStressGainIfNeeded(ResidentRecord resident, ResidentStats stats, int stressBefore)
        {
            if (PreventsStressGain(resident) && stats.Stress > stressBefore)
            {
                stats.Stress = stressBefore;
            }
        }

        static bool PreventsStressGain(ResidentRecord resident)
        {
            var unitClass = resident?.Definition?.UnitClass;
            if (unitClass != null && unitClass.HasAbility(UnitAbilityFlags.PreventNearbyStress))
            {
                return true;
            }

            return TownState.HasTrait(resident, trait => trait.PreventsStressGain);
        }

        static void ApplyTraitStartingBonuses(ResidentRecord resident)
        {
            if (resident.Definition?.Traits == null)
            {
                return;
            }

            foreach (var trait in resident.Definition.Traits)
            {
                if (trait == null)
                {
                    continue;
                }

                resident.Stats.Apply(
                    trait.StartingHealthBonus,
                    trait.StartingMoodBonus,
                    trait.StartingStressBonus);
            }
        }

        static void ApplyTraitDailyBonuses(ResidentRecord resident, ResidentStats stats)
        {
            if (resident.Definition?.Traits == null)
            {
                return;
            }

            foreach (var trait in resident.Definition.Traits)
            {
                if (trait == null)
                {
                    continue;
                }

                stats.Apply(
                    trait.DailyHealthDelta,
                    trait.DailyMoodDelta,
                    trait.DailyStressDelta);
            }
        }
    }
}
