using System.Collections.Generic;
using NotSoWild.Core;
using UnityEngine;

namespace NotSoWild.Gameplay
{
    public sealed class ResidentRecord
    {
        public VisitorDefinition Definition;
        public ResidentAgent Agent;
        public ResidentStats Stats = new();
        public ResidentWorkState WorkState = ResidentWorkState.Idle;
        public ConstructionSite ConstructionSite;
        public PlacedBuildingRecord AssignedBuilding;
        public WeaponType EquippedWeapon;

        public ResidentRecord(VisitorDefinition definition)
        {
            Definition = definition;
        }
    }

    public sealed class TownState
    {
        public int Gold;
        public int Reputation;
        public int Defense;
        public int Heat;
        public int MaxResidents;
        public float ElapsedTimeSeconds;
        public float TargetTimeSeconds;
        public int RaidCount;
        public int Pistols;
        public int Rifles;
        public int Shotguns;
        public readonly List<ResidentRecord> Residents = new();
        public readonly List<PlacedBuildingRecord> Buildings = new();
        public readonly List<ConstructionSite> ConstructionSites = new();
        public readonly List<string> Log = new();

        int _nextConstructionId = 1;

        public bool IsResidentCapReached => GetResidentCapacityUsage() >= GetResidentCapacity();

        public int AllocateConstructionId() => _nextConstructionId++;

        public void AddLog(string message)
        {
            Log.Insert(0, message);
            if (Log.Count > 12)
            {
                Log.RemoveAt(Log.Count - 1);
            }
        }

        public PlacedBuildingRecord FindBuildingAt(GridCoordinates cell)
        {
            foreach (var building in Buildings)
            {
                if (building?.Definition == null)
                {
                    continue;
                }

                var origin = building.Origin;
                int width = building.Definition.Width;
                int height = building.Definition.Height;
                if (cell.X >= origin.X && cell.X < origin.X + width &&
                    cell.Y >= origin.Y && cell.Y < origin.Y + height)
                {
                    return building;
                }
            }

            return null;
        }

        public bool HasOperationalArmory()
        {
            foreach (var building in Buildings)
            {
                if (building?.Definition != null &&
                    building.IsOperational &&
                    building.Definition.WorkRole == WorkRole.Armory)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasOperationalChurch()
        {
            foreach (var building in Buildings)
            {
                if (building?.Definition != null &&
                    building.IsOperational &&
                    building.Definition.WorkRole == WorkRole.Church)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetWeaponCount(WeaponType weapon)
        {
            return weapon switch
            {
                WeaponType.Pistol => Pistols,
                WeaponType.Rifle => Rifles,
                WeaponType.Shotgun => Shotguns,
                _ => 0
            };
        }

        public void AddWeapon(WeaponType weapon, int amount)
        {
            switch (weapon)
            {
                case WeaponType.Pistol:
                    Pistols = Mathf.Max(0, Pistols + amount);
                    break;
                case WeaponType.Rifle:
                    Rifles = Mathf.Max(0, Rifles + amount);
                    break;
                case WeaponType.Shotgun:
                    Shotguns = Mathf.Max(0, Shotguns + amount);
                    break;
            }
        }

        public bool TryEquipWeapon(ResidentRecord resident, WeaponType weapon)
        {
            if (resident == null || weapon == WeaponType.None || GetWeaponCount(weapon) <= 0)
            {
                return false;
            }

            if (resident.EquippedWeapon != WeaponType.None)
            {
                AddWeapon(resident.EquippedWeapon, 1);
            }

            AddWeapon(weapon, -1);
            resident.EquippedWeapon = weapon;
            return true;
        }

        public void UnequipWeapon(ResidentRecord resident)
        {
            if (resident == null || resident.EquippedWeapon == WeaponType.None)
            {
                return;
            }

            AddWeapon(resident.EquippedWeapon, 1);
            resident.EquippedWeapon = WeaponType.None;
        }

        public ConstructionSite FindConstructionAt(GridCoordinates cell)
        {
            foreach (var site in ConstructionSites)
            {
                if (site?.Definition == null)
                {
                    continue;
                }

                var origin = site.Origin;
                int width = site.Definition.Width;
                int height = site.Definition.Height;
                if (cell.X >= origin.X && cell.X < origin.X + width &&
                    cell.Y >= origin.Y && cell.Y < origin.Y + height)
                {
                    return site;
                }
            }

            return null;
        }

        public int GetResidentCombatPower(ResidentRecord resident)
        {
            var unitClass = resident?.Definition?.UnitClass;
            int total = unitClass != null ? unitClass.Attack : 1;
            if (unitClass == null)
            {
                total += GetWeaponAttackBonus(resident?.EquippedWeapon ?? WeaponType.None);
            }

            if (resident.Definition?.Traits == null)
            {
                return total;
            }

            foreach (var trait in resident.Definition.Traits)
            {
                if (trait != null)
                {
                    total += trait.DefenseBonus;
                }
            }

            return total;
        }

        public int GetResidentHealthBonus(ResidentRecord resident)
        {
            var unitClass = resident?.Definition?.UnitClass;
            if (unitClass != null)
            {
                return 0;
            }

            int total = 0;
            if (resident?.Definition?.Traits == null)
            {
                return total;
            }

            foreach (var trait in resident.Definition.Traits)
            {
                if (trait != null)
                {
                    total += trait.StartingHealthBonus;
                }
            }

            return total;
        }

        public int GetResidentAccuracy(ResidentRecord resident)
        {
            var unitClass = resident?.Definition?.UnitClass;
            int total = unitClass != null ? unitClass.Accuracy : 60;
            if (unitClass == null)
            {
                total += GetWeaponAccuracyBonus(resident?.EquippedWeapon ?? WeaponType.None);
            }

            if (resident?.Definition?.Traits != null)
            {
                foreach (var trait in resident.Definition.Traits)
                {
                    if (trait != null)
                    {
                        total += trait.AccuracyBonus;
                    }
                }
            }

            if (resident?.Stats != null)
            {
                total -= Mathf.RoundToInt(resident.Stats.Stress * 0.35f);
            }

            return Mathf.Clamp(total, 10, 95);
        }

        public float GetResidentAttackRange(ResidentRecord resident)
        {
            var unitClass = resident?.Definition?.UnitClass;
            float total = unitClass != null ? unitClass.AttackRange : CombatUnit.DefaultAttackRange;
            if (resident?.Definition?.Traits != null)
            {
                foreach (var trait in resident.Definition.Traits)
                {
                    if (trait != null)
                    {
                        total += trait.AttackRangeBonus;
                    }
                }
            }

            return Mathf.Clamp(total, 1.2f, 7.5f);
        }

        public float GetResidentAttackCooldown(ResidentRecord resident)
        {
            var unitClass = resident?.Definition?.UnitClass;
            if (unitClass != null)
            {
                return Mathf.Clamp(unitClass.AttackCooldown, 0.25f, 3f);
            }

            float multiplier = 1f;
            if (resident?.Definition?.Traits != null)
            {
                foreach (var trait in resident.Definition.Traits)
                {
                    if (trait != null && trait.AttackCooldownMultiplier > 0f)
                    {
                        multiplier *= trait.AttackCooldownMultiplier;
                    }
                }
            }

            return Mathf.Clamp(CombatUnit.DefaultAttackCooldown * multiplier, 0.25f, 3f);
        }

        public int GetResidentTargetCount(ResidentRecord resident)
        {
            var unitClass = resident?.Definition?.UnitClass;
            if (unitClass != null)
            {
                return Mathf.Clamp(unitClass.TargetCount, 1, 5);
            }

            int total = 1;
            if (resident?.Definition?.Traits != null)
            {
                foreach (var trait in resident.Definition.Traits)
                {
                    if (trait != null)
                    {
                        total += trait.ExtraTargets;
                    }
                }
            }

            return Mathf.Clamp(total, 1, 3);
        }

        public int GetResidentMultiTargetAccuracyPenalty(ResidentRecord resident)
        {
            var unitClass = resident?.Definition?.UnitClass;
            if (unitClass != null)
            {
                return Mathf.Max(0, unitClass.MultiTargetAccuracyPenalty);
            }

            int total = 0;
            if (resident?.Definition?.Traits != null)
            {
                foreach (var trait in resident.Definition.Traits)
                {
                    if (trait != null)
                    {
                        total += trait.MultiTargetAccuracyPenalty;
                    }
                }
            }

            return Mathf.Max(0, total);
        }

        public bool ResidentAccuracyScalesWithDistance(ResidentRecord resident)
        {
            if (resident?.Definition?.UnitClass != null)
            {
                return false;
            }

            if (resident?.Definition?.Traits == null)
            {
                return false;
            }

            foreach (var trait in resident.Definition.Traits)
            {
                if (trait != null && trait.AccuracyScalesWithDistance)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetResidentMeleeDamageBonus(ResidentRecord resident)
        {
            if (resident?.Definition?.UnitClass != null)
            {
                return 0;
            }

            int total = 0;
            if (resident?.Definition?.Traits != null)
            {
                foreach (var trait in resident.Definition.Traits)
                {
                    if (trait != null)
                    {
                        total += trait.MeleeDamageBonus;
                    }
                }
            }

            return total;
        }

        public int GetTotalCombatPower()
        {
            int total = GetTownDefensePower();
            foreach (var resident in Residents)
            {
                total += GetResidentCombatPower(resident);
            }

            return total;
        }

        public int GetTownDefensePower()
        {
            int total = Defense;
            foreach (var building in Buildings)
            {
                if (!building.IsOperational || building.Definition == null)
                {
                    continue;
                }

                total += building.Definition.StaffDefense;
                total += WorkforceHelper.GetWorkBonus(building.Worker, building.Definition.WorkRole);
            }

            return total;
        }

        public int GetRaidRiskBonus()
        {
            int total = Heat / 3;
            foreach (var resident in Residents)
            {
                if (resident.Definition?.Traits == null)
                {
                    continue;
                }

                foreach (var trait in resident.Definition.Traits)
                {
                    if (trait != null)
                    {
                        total += trait.RaidRiskBonus;
                    }
                }
            }

            foreach (var building in Buildings)
            {
                if (!building.IsOperational || building.Definition == null)
                {
                    continue;
                }

                total += building.Definition.StaffHeat;
            }

            return total;
        }

        public int GetDailyGoldIncome()
        {
            return GetResidentTraitIncome() + GetOperationalBuildingIncome();
        }

        public int GetResidentCapacity()
        {
            int total = MaxResidents;
            foreach (var building in Buildings)
            {
                if (building?.Definition != null && building.IsOperational)
                {
                    total += building.Definition.ResidentCapacityBonus;
                }
            }

            return Mathf.Max(0, total);
        }

        public int GetResidentCapacityUsage()
        {
            int total = 0;
            foreach (var resident in Residents)
            {
                var unitClass = resident?.Definition?.UnitClass;
                bool doesNotUseCapacity = unitClass != null
                    ? unitClass.HasAbility(UnitAbilityFlags.DoesNotUseResidentCapacity)
                    : HasTrait(resident, trait => trait.DoesNotUseResidentCapacity);
                if (!doesNotUseCapacity)
                {
                    total++;
                }
            }

            return total;
        }

        public static bool HasTrait(ResidentRecord resident, System.Func<TraitDefinition, bool> predicate)
        {
            if (resident?.Definition?.Traits == null || predicate == null)
            {
                return false;
            }

            foreach (var trait in resident.Definition.Traits)
            {
                if (trait != null && predicate(trait))
                {
                    return true;
                }
            }

            return false;
        }

        public static int GetWeaponCost(WeaponType weapon)
        {
            return weapon switch
            {
                WeaponType.Pistol => 4,
                WeaponType.Rifle => 7,
                WeaponType.Shotgun => 6,
                _ => 0
            };
        }

        public static int GetWeaponAttackBonus(WeaponType weapon)
        {
            return weapon switch
            {
                WeaponType.Pistol => 1,
                WeaponType.Rifle => 2,
                WeaponType.Shotgun => 3,
                _ => 0
            };
        }

        public static int GetWeaponAccuracyBonus(WeaponType weapon)
        {
            return weapon switch
            {
                WeaponType.Pistol => 5,
                WeaponType.Rifle => 15,
                WeaponType.Shotgun => -10,
                _ => 0
            };
        }

        public int GetResidentTraitIncome()
        {
            int total = 0;
            foreach (var resident in Residents)
            {
                if (resident.Definition?.Traits == null)
                {
                    continue;
                }

                foreach (var trait in resident.Definition.Traits)
                {
                    if (trait != null)
                    {
                        total += trait.DailyGoldBonus;
                    }
                }
            }

            return total;
        }

        public int GetOperationalBuildingIncome()
        {
            int total = 0;
            foreach (var building in Buildings)
            {
                if (!building.IsOperational || building.Definition == null)
                {
                    continue;
                }

                float efficiency = ResidentStatsHelper.GetWorkEfficiency(building.Worker);
                int output = building.Definition.PassiveDailyGold;
                if (building.Worker != null)
                {
                    output += building.Definition.StaffDailyGold +
                              WorkforceHelper.GetWorkBonus(building.Worker, building.Definition.WorkRole);
                }

                total += Mathf.Max(0, Mathf.RoundToInt(output * efficiency));
            }

            return total;
        }
    }
}
