using NotSoWild.Core;

namespace NotSoWild.Gameplay
{
    public static class WorkforceHelper
    {
        public static bool CanBuild(ResidentRecord resident)
        {
            if (resident?.Definition?.Traits == null)
            {
                return true;
            }

            foreach (var trait in resident.Definition.Traits)
            {
                if (trait != null && trait.CannotBuild)
                {
                    return false;
                }
            }

            return true;
        }

        public static int GetWorkBonus(ResidentRecord resident, WorkRole role)
        {
            if (resident?.Definition?.Traits == null)
            {
                return 0;
            }

            int bonus = 0;
            foreach (var trait in resident.Definition.Traits)
            {
                if (trait?.WorkBonuses == null)
                {
                    continue;
                }

                foreach (var entry in trait.WorkBonuses)
                {
                    if (entry.Role == role)
                    {
                        bonus += entry.Bonus;
                    }
                }
            }

            return bonus;
        }

        public static float GetBuildSpeedMultiplier(ResidentRecord builder)
        {
            return (1f + GetWorkBonus(builder, WorkRole.Construction)) *
                   ResidentStatsHelper.GetWorkEfficiency(builder);
        }

        public static bool IsIdle(ResidentRecord resident) =>
            resident != null && resident.WorkState == ResidentWorkState.Idle;

        public static bool CanWorkAt(ResidentRecord resident, BuildingDefinition building)
        {
            if (resident == null || building == null)
            {
                return false;
            }

            if (building.WorkRole == WorkRole.Church &&
                TownState.HasTrait(resident, trait => trait.CannotWorkAtChurch))
            {
                return false;
            }

            return true;
        }

        public static bool CanStartConstruction(TownState state)
        {
            if (state == null)
            {
                return false;
            }

            foreach (var resident in state.Residents)
            {
                if (IsIdle(resident) && CanBuild(resident))
                {
                    return true;
                }
            }

            return false;
        }

        public static int CountIdleWorkers(TownState state)
        {
            int count = 0;
            if (state == null)
            {
                return count;
            }

            foreach (var resident in state.Residents)
            {
                if (IsIdle(resident) && CanBuild(resident))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
