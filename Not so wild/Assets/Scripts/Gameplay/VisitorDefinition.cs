using UnityEngine;
using NotSoWild.Core;

namespace NotSoWild.Gameplay
{
    public enum VisitorKind
    {
        ResidentCandidate,
        Event,
        BuilderEvent
    }

    [CreateAssetMenu(fileName = "Visitor", menuName = "Not So Wild/Visitor")]
    public sealed class VisitorDefinition : ScriptableObject
    {
        public string DisplayName = "Stranger";
        public VisitorKind Kind = VisitorKind.ResidentCandidate;
        [TextArea] public string Description;
        public UnitClassDefinition UnitClass;
        public TraitDefinition[] Traits;

        [Header("Event")]
        [TextArea] public string EventText;
        public string AcceptLabel = "Accept";
        public string RejectLabel = "Refuse";
        public int AcceptGold;
        public int AcceptReputation;
        public int AcceptHeat;
        public int RejectGold;
        public int RejectReputation;
        public int RejectHeat;
        public UnitFaction BuilderFaction;
        public BuildingDefinition OfferedBuilding;

        [Header("Resident")]
        public int AcceptReputationBonus = 1;
        public int RejectReputationPenalty = 1;
    }
}
