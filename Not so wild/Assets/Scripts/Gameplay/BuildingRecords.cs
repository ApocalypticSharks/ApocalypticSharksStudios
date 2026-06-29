using NotSoWild.Core;
using NotSoWild.Visual;

namespace NotSoWild.Gameplay
{
    public enum ResidentWorkState
    {
        Idle,
        Building,
        Working
    }

    public sealed class ConstructionSite
    {
        public int Id;
        public BuildingDefinition Definition;
        public GridCoordinates Center;
        public GridCoordinates Origin;
        public ResidentRecord Builder;
        public float Progress;
        public BuildingView View;

        public float RequiredSeconds => Definition != null ? Definition.BuildSeconds : 1f;
        public bool IsComplete => Progress >= RequiredSeconds;
    }

    public sealed class PlacedBuildingRecord
    {
        public BuildingDefinition Definition;
        public GridCoordinates Center;
        public GridCoordinates Origin;
        public BuildingView View;
        public ResidentRecord Worker;

        public bool IsOperational => Definition != null && (!Definition.RequiresWorker || Worker != null);
    }
}
