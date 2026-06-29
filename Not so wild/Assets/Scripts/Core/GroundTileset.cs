using UnityEngine;

namespace NotSoWild.Core
{
    [CreateAssetMenu(fileName = "GroundTileset", menuName = "Not So Wild/Ground Tileset")]
    public sealed class GroundTileset : ScriptableObject
    {
        [Header("North ground row (top of sheet)")]
        public Sprite GroundNorth0;
        public Sprite GroundNorth1;
        public Sprite GroundNorth2;

        [Header("Road row (center)")]
        public Sprite Road0;
        public Sprite Road1;
        public Sprite Road2;

        [Header("South ground row (bottom of sheet)")]
        public Sprite GroundSouth0;
        public Sprite GroundSouth1;
        public Sprite GroundSouth2;

        public Sprite GetSprite(int column, int row, int roadCenterRow, int seed)
        {
            if (row == roadCenterRow)
            {
                return PickRoadVariant(column, row, seed, Road0, Road1, Road2);
            }

            if (row > roadCenterRow)
            {
                return PickGroundVariant(column, row, seed, GroundNorth0, GroundNorth1, GroundNorth2);
            }

            return PickGroundVariant(column, row, seed, GroundSouth0, GroundSouth1, GroundSouth2);
        }

        static Sprite PickRoadVariant(int column, int row, int seed, Sprite road0, Sprite road1, Sprite road2)
        {
            int variant = PositiveMod(HashCell(column, row, seed), 3);
            return variant switch
            {
                0 => road0,
                1 => road1,
                _ => road2
            };
        }

        static Sprite PickGroundVariant(int column, int row, int seed, Sprite decoratedA, Sprite plain, Sprite decoratedB)
        {
            int roll = PositiveMod(HashCell(column, row, seed), 100);
            if (roll < 20)
            {
                return decoratedA;
            }

            if (roll < 65)
            {
                return plain;
            }

            return decoratedB;
        }

        static int HashCell(int column, int row, int seed)
        {
            unchecked
            {
                int hash = seed;
                hash = (hash ^ (column * 374761393)) * 668265263;
                hash = (hash ^ (row * 1274126177)) * 668265263;
                hash ^= hash >> 13;
                hash *= 1274126177;
                hash ^= hash >> 16;
                return hash;
            }
        }

        static int PositiveMod(int value, int size)
        {
            if (size <= 0)
            {
                return 0;
            }

            int result = value % size;
            return result < 0 ? result + size : result;
        }
    }
}
