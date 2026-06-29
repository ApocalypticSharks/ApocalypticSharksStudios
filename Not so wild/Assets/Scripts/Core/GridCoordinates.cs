using System;
using UnityEngine;

namespace NotSoWild.Core
{
    [Serializable]
    public struct GridCoordinates : IEquatable<GridCoordinates>
    {
        public int X;
        public int Y;

        public GridCoordinates(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(GridCoordinates other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => obj is GridCoordinates other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(GridCoordinates a, GridCoordinates b) => a.Equals(b);

        public static bool operator !=(GridCoordinates a, GridCoordinates b) => !a.Equals(b);

        public override string ToString() => $"({X}, {Y})";

        public int ManhattanDistanceTo(GridCoordinates other) =>
            Mathf.Abs(X - other.X) + Mathf.Abs(Y - other.Y);
    }
}
