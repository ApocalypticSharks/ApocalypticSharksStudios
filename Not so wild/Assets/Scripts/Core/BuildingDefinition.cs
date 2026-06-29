using UnityEngine;
using NotSoWild.Gameplay;

namespace NotSoWild.Core
{
    [CreateAssetMenu(fileName = "BuildingDefinition", menuName = "Not So Wild/Building Definition")]
    public sealed class BuildingDefinition : ScriptableObject
    {
        public string DisplayName = "Building";
        public UnitFaction Faction = UnitFaction.Settlers;
        public Sprite Sprite;
        [Min(1)] public int Width = 3;
        [Min(1)] public int Height = 3;
        public int SortingOrder = 10;

        [Header("Construction")]
        [Min(0)] public int GoldCost = 8;
        [Min(1)] public int BuildSeconds = 20;

        [Header("Operation")]
        public bool RequiresWorker = true;
        public WorkRole WorkRole = WorkRole.General;
        [Tooltip("Applied once the building is completed, even without a worker.")]
        public int ResidentCapacityBonus;
        [Tooltip("Applied at night once the building is completed, even without a worker.")]
        public int PassiveDailyGold;
        [Tooltip("Applied only when a worker is assigned.")]
        public int StaffDailyGold;
        public int StaffDefense;
        public int StaffReputation;
        public int StaffHeat;
        public int StaffDailyHealth;
        public int StaffDailyMood;
        public int StaffDailyStress;
    }
}
