using UnityEngine;

namespace NotSoWild.Core
{
    [CreateAssetMenu(fileName = "TownGridConfig", menuName = "Not So Wild/Town Grid Config")]
    public class TownGridConfig : ScriptableObject
    {
        [Min(0.25f)]
        public float CellSize = 1f;

        [Min(1)]
        public int RoadRows = 1;

        [Min(1)]
        public int BuildDepthPerSide = 3;

        [Min(0f)]
        public float ScreenPadding = 0.1f;

        public bool UseFixedGridDimensions = true;
        public int GridColumns = 17;
        public int GridRows = 9;

        public int StartingTownHallCenterX = 8;
        public int StartingTownHallCenterY = 6;

        public GridCoordinates StartingTownHallCenter =>
            new(StartingTownHallCenterX, StartingTownHallCenterY);
    }
}
