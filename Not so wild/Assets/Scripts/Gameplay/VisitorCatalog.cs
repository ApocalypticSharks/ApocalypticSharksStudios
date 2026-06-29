using UnityEngine;

namespace NotSoWild.Gameplay
{
    [CreateAssetMenu(fileName = "VisitorCatalog", menuName = "Not So Wild/Visitor Catalog")]
    public sealed class VisitorCatalog : ScriptableObject
    {
        public VisitorDefinition[] ResidentCandidates;
        public VisitorDefinition[] EventVisitors;
    }
}
