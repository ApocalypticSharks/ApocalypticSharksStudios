using NotSoWild.Core;
using UnityEngine;

namespace NotSoWild.Gameplay
{
    [CreateAssetMenu(fileName = "BuildingCatalog", menuName = "Not So Wild/Building Catalog")]
    public sealed class BuildingCatalog : ScriptableObject
    {
        public BuildingDefinition[] Buildings;
    }
}
