using UnityEngine;

namespace NotSoWild.Gameplay
{
    [CreateAssetMenu(fileName = "GameSessionConfig", menuName = "Not So Wild/Game Session Config")]
    public sealed class GameSessionConfig : ScriptableObject
    {
        public int StartingGold = 20;
        public int StartingReputation = 10;
        public int StartingDefense = 2;
        public VisitorDefinition StartingMayor;
        public int MaxResidents = 5;
        [Min(30f)] public float TargetTimeSeconds = 420f;
        [Min(5f)] public float TownTickIntervalSeconds = 60f;
        public float VisitorSpawnIntervalMin = 8f;
        public float VisitorSpawnIntervalMax = 22f;
        public float VisitorMoveSpeed = 2.5f;
        public float ResidentPatrolSpeed = 1.4f;
        [Min(1f)]
        public float DecisionTimeLimit = 15f;
        [Min(1f)] public float RaidIntervalMinSeconds = 15f;
        [Min(1f)] public float RaidIntervalMaxSeconds = 30f;
        public int BaseRaidPower = 4;
        public int RaidPowerPerMinute = 1;
        public int RaidPowerPerAttack = 2;
        public UnitClassDefinition RaidBanditClass;
        public UnitClassDefinition RaidBanditLeaderClass;
        [Min(1)] public int FirstLeaderRaid = 3;
        public int RaidFailureGoldLoss = 12;
        public int RaidFailureReputationLoss = 4;
        public int RaidSuccessReputationGain = 2;
    }
}
