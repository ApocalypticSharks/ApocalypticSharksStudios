using System.Collections.Generic;
using UnityEngine;

namespace NotSoWild.Core
{
    public sealed class TownGrid
    {
        readonly GridCellType[,] _cells;
        readonly bool[,] _occupied;
        readonly int _roadRowMin;
        readonly int _roadRowMax;
        readonly int _buildRowMin;
        readonly int _buildRowMax;

        public int Columns { get; }
        public int Rows { get; }
        public float CellSize { get; }
        public Vector2 Origin { get; }
        public GridCoordinates EntryCell { get; }
        public Vector2 EntryDirection { get; } = Vector2.left;
        public int RoadCenterRow { get; }
        public int RoadRowMin => _roadRowMin;
        public int RoadRowMax => _roadRowMax;

        public TownGrid(int columns, int rows, float cellSize, Vector2 origin, TownGridConfig config)
        {
            Columns = columns;
            Rows = rows;
            CellSize = cellSize;
            Origin = origin;

            _cells = new GridCellType[columns, rows];
            _occupied = new bool[columns, rows];

            int roadCenterRow = rows / 2;
            RoadCenterRow = roadCenterRow;
            int roadHalf = Mathf.Max(0, (config.RoadRows - 1) / 2);
            _roadRowMin = roadCenterRow - roadHalf;
            _roadRowMax = _roadRowMin + config.RoadRows - 1;

            _buildRowMin = Mathf.Max(0, _roadRowMin - config.BuildDepthPerSide);
            _buildRowMax = Mathf.Min(rows - 1, _roadRowMax + config.BuildDepthPerSide);

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    _cells[x, y] = ResolveCellType(y);
                }
            }

            EntryCell = new GridCoordinates(columns - 1, roadCenterRow);
        }

        GridCellType ResolveCellType(int row)
        {
            if (row >= _roadRowMin && row <= _roadRowMax)
            {
                return GridCellType.Road;
            }

            if (row >= _buildRowMin && row <= _buildRowMax)
            {
                return GridCellType.Buildable;
            }

            return GridCellType.Empty;
        }

        public bool IsInside(GridCoordinates coordinates) =>
            coordinates.X >= 0 &&
            coordinates.X < Columns &&
            coordinates.Y >= 0 &&
            coordinates.Y < Rows;

        public GridCellType GetCellType(GridCoordinates coordinates)
        {
            if (!IsInside(coordinates))
            {
                return GridCellType.Empty;
            }

            return _cells[coordinates.X, coordinates.Y];
        }

        public bool IsRoad(GridCoordinates coordinates) =>
            GetCellType(coordinates) == GridCellType.Road;

        public bool IsBuildable(GridCoordinates coordinates) =>
            GetCellType(coordinates) == GridCellType.Buildable;

        public bool IsOccupied(GridCoordinates coordinates) =>
            IsInside(coordinates) && _occupied[coordinates.X, coordinates.Y];

        public GridCoordinates GetFootprintOriginFromCenter(GridCoordinates center, int width, int height) =>
            new GridCoordinates(center.X - (width - 1) / 2, center.Y - (height - 1) / 2);

        public Vector3 GetFootprintCenterWorldPosition(GridCoordinates center)
        {
            var world = CellToWorldCenter(center);
            return new Vector3(world.x, world.y, -0.1f);
        }

        public Vector3 AlignBuildingToFootprintCenter(
            GridCoordinates center,
            Sprite sprite,
            int footprintWidth,
            int footprintHeight)
        {
            var footprintCenter = CellToWorldCenter(center);
            var footprintSize = GetFootprintWorldSize(footprintWidth, footprintHeight);
            var rect = sprite.rect;
            var pivotNormalized = new Vector2(
                sprite.pivot.x / rect.width,
                sprite.pivot.y / rect.height);
            var pivotToFootprintCenter = new Vector2(
                (0.5f - pivotNormalized.x) * footprintSize.x,
                (0.5f - pivotNormalized.y) * footprintSize.y);

            return new Vector3(
                footprintCenter.x - pivotToFootprintCenter.x,
                footprintCenter.y - pivotToFootprintCenter.y,
                -0.1f);
        }

        public Vector2 GetFootprintWorldSize(int footprintWidth, int footprintHeight) =>
            new Vector2(footprintWidth * CellSize, footprintHeight * CellSize);

        public GridCoordinates ResolveTownHallCenter(
            int preferredCenterX,
            int preferredCenterY,
            int footprintWidth,
            int footprintHeight)
        {
            int centerX = Mathf.Clamp(
                preferredCenterX,
                (footprintWidth - 1) / 2,
                Columns - 1 - (footprintWidth - 1) / 2);

            int preferredY = Mathf.Clamp(
                preferredCenterY,
                (footprintHeight - 1) / 2,
                Rows - 1 - (footprintHeight - 1) / 2);

            if (TryCenter(centerX, preferredY, footprintWidth, footprintHeight, out var center))
            {
                return center;
            }

            int northCenterY = _roadRowMax + 1 + (footprintHeight - 1) / 2;
            northCenterY = Mathf.Clamp(
                northCenterY,
                (footprintHeight - 1) / 2,
                Rows - 1 - (footprintHeight - 1) / 2);

            if (TryCenter(centerX, northCenterY, footprintWidth, footprintHeight, out center))
            {
                return center;
            }

            return ResolveStartingTownHallCenter(
                preferredCenterX,
                preferredCenterY,
                footprintWidth,
                footprintHeight);
        }

        public IEnumerable<GridCoordinates> GetFootprintCells(
            GridCoordinates center,
            int footprintWidth,
            int footprintHeight)
        {
            var origin = GetFootprintOriginFromCenter(center, footprintWidth, footprintHeight);
            for (int x = origin.X; x < origin.X + footprintWidth; x++)
            {
                for (int y = origin.Y; y < origin.Y + footprintHeight; y++)
                {
                    yield return new GridCoordinates(x, y);
                }
            }
        }

        public GridCoordinates ResolveStartingTownHallCenter(
            int preferredCenterX,
            int preferredCenterY,
            int footprintWidth,
            int footprintHeight)
        {
            int centerX = Mathf.Clamp(
                preferredCenterX,
                (footprintWidth - 1) / 2,
                Columns - 1 - (footprintWidth - 1) / 2);
            int centerY = Mathf.Clamp(
                preferredCenterY,
                (footprintHeight - 1) / 2,
                Rows - 1 - (footprintHeight - 1) / 2);

            if (TryCenter(centerX, centerY, footprintWidth, footprintHeight, out var resolved))
            {
                return resolved;
            }

            for (int radius = 1; radius <= Mathf.Max(Columns, Rows); radius++)
            {
                for (int dy = radius; dy >= -radius; dy--)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius)
                        {
                            continue;
                        }

                        if (TryCenter(centerX + dx, centerY + dy, footprintWidth, footprintHeight, out resolved))
                        {
                            return resolved;
                        }
                    }
                }
            }

            return new GridCoordinates(centerX, centerY);
        }

        bool TryCenter(
            int centerX,
            int centerY,
            int footprintWidth,
            int footprintHeight,
            out GridCoordinates center)
        {
            center = new GridCoordinates(centerX, centerY);
            if (centerX < (footprintWidth - 1) / 2 ||
                centerY < (footprintHeight - 1) / 2 ||
                centerX > Columns - 1 - (footprintWidth - 1) / 2 ||
                centerY > Rows - 1 - (footprintHeight - 1) / 2)
            {
                return false;
            }

            var origin = GetFootprintOriginFromCenter(center, footprintWidth, footprintHeight);
            return CanOccupyFootprint(origin, footprintWidth, footprintHeight);
        }

        public GridCoordinates GetDefaultTownHallCenter(int footprintWidth, int footprintHeight, int preferredCenterX)
        {
            return ResolveStartingTownHallCenter(
                preferredCenterX,
                RoadCenterRow + 1 + (footprintHeight - 1) / 2,
                footprintWidth,
                footprintHeight);
        }

        public bool CanOccupyFootprint(GridCoordinates origin, int width, int height, bool allowRoad = false)
        {
            if (origin.X < 0 || origin.Y < 0 || origin.X + width > Columns || origin.Y + height > Rows)
            {
                return false;
            }

            for (int x = origin.X; x < origin.X + width; x++)
            {
                for (int y = origin.Y; y < origin.Y + height; y++)
                {
                    var coordinates = new GridCoordinates(x, y);
                    if (IsOccupied(coordinates))
                    {
                        return false;
                    }

                    if (allowRoad)
                    {
                        if (!IsRoad(coordinates) && !IsBuildable(coordinates))
                        {
                            return false;
                        }
                    }
                    else if (!IsBuildable(coordinates))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool FootprintTouchesRoad(GridCoordinates origin, int width, int height)
        {
            for (int x = origin.X; x < origin.X + width; x++)
            {
                for (int y = origin.Y; y < origin.Y + height; y++)
                {
                    var coordinates = new GridCoordinates(x, y);
                    if (IsRoadNeighbor(coordinates))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool IsRoadNeighbor(GridCoordinates coordinates)
        {
            var neighbors = new[]
            {
                new GridCoordinates(coordinates.X - 1, coordinates.Y),
                new GridCoordinates(coordinates.X + 1, coordinates.Y),
                new GridCoordinates(coordinates.X, coordinates.Y - 1),
                new GridCoordinates(coordinates.X, coordinates.Y + 1)
            };

            foreach (var neighbor in neighbors)
            {
                if (IsInside(neighbor) && IsRoad(neighbor))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanPlaceBuilding(GridCoordinates center, int width, int height)
        {
            var origin = GetFootprintOriginFromCenter(center, width, height);
            return CanOccupyFootprint(origin, width, height) &&
                   FootprintTouchesRoad(origin, width, height);
        }

        public Vector3 GetBuildingWorkAnchorWorld(GridCoordinates center, int width, int height)
        {
            var origin = GetFootprintOriginFromCenter(center, width, height);
            GridCoordinates roadCell = default;
            bool found = false;
            int bestDistance = int.MaxValue;

            for (int x = origin.X; x < origin.X + width; x++)
            {
                for (int y = origin.Y; y < origin.Y + height; y++)
                {
                    var cell = new GridCoordinates(x, y);
                    if (!IsRoadNeighbor(cell))
                    {
                        continue;
                    }

                    int distance = Mathf.Abs(x - center.X) + Mathf.Abs(y - center.Y);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        roadCell = cell;
                        found = true;
                    }
                }
            }

            if (!found)
            {
                return CellToWorldCenter(center);
            }

            float anchorX = CellToWorldCenter(roadCell).x;
            float roadY = CellToWorldCenter(new GridCoordinates(0, RoadCenterRow)).y;
            return new Vector3(anchorX, roadY, -0.04f);
        }

        public bool TryOccupyFootprint(GridCoordinates origin, int width, int height, bool allowRoad = false)
        {
            if (!CanOccupyFootprint(origin, width, height, allowRoad))
            {
                return false;
            }

            for (int x = origin.X; x < origin.X + width; x++)
            {
                for (int y = origin.Y; y < origin.Y + height; y++)
                {
                    _occupied[x, y] = true;
                }
            }

            return true;
        }

        public void ReleaseFootprint(GridCoordinates origin, int width, int height)
        {
            int minX = Mathf.Max(0, origin.X);
            int minY = Mathf.Max(0, origin.Y);
            int maxX = Mathf.Min(Columns, origin.X + width);
            int maxY = Mathf.Min(Rows, origin.Y + height);

            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    _occupied[x, y] = false;
                }
            }
        }

        public Vector3 FootprintBottomCenterToWorld(GridCoordinates origin, int width, int height)
        {
            float x = Origin.x + (origin.X + width * 0.5f) * CellSize;
            float y = Origin.y + origin.Y * CellSize;
            return new Vector3(x, y, -0.1f);
        }

        public Vector3 FootprintBottomCenterToWorld(GridCoordinates origin, int width) =>
            FootprintBottomCenterToWorld(origin, width, width);

        public Vector3 CellToWorldCenter(GridCoordinates coordinates)
        {
            float x = Origin.x + (coordinates.X + 0.5f) * CellSize;
            float y = Origin.y + (coordinates.Y + 0.5f) * CellSize;
            return new Vector3(x, y, 0f);
        }

        public bool TryWorldToCell(Vector3 worldPosition, out GridCoordinates coordinates)
        {
            float localX = worldPosition.x - Origin.x;
            float localY = worldPosition.y - Origin.y;

            if (localX < 0f || localY < 0f)
            {
                coordinates = default;
                return false;
            }

            int x = Mathf.FloorToInt(localX / CellSize);
            int y = Mathf.FloorToInt(localY / CellSize);
            coordinates = new GridCoordinates(x, y);
            return IsInside(coordinates);
        }

        public Vector3 EntryWorldPosition => CellToWorldCenter(EntryCell);

        public void GetRoadPatrolBounds(out float minX, out float maxX, out float minY, out float maxY)
        {
            minX = CellToWorldCenter(new GridCoordinates(1, RoadCenterRow)).x;
            maxX = CellToWorldCenter(new GridCoordinates(Columns - 2, RoadCenterRow)).x;
            minY = CellToWorldCenter(new GridCoordinates(0, _roadRowMin)).y;
            maxY = CellToWorldCenter(new GridCoordinates(0, _roadRowMax)).y;
        }

        public IEnumerable<GridCoordinates> GetRoadCells()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    var coordinates = new GridCoordinates(x, y);
                    if (IsRoad(coordinates))
                    {
                        yield return coordinates;
                    }
                }
            }
        }

        public IEnumerable<GridCoordinates> GetBuildableCells()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    var coordinates = new GridCoordinates(x, y);
                    if (IsBuildable(coordinates))
                    {
                        yield return coordinates;
                    }
                }
            }
        }
    }
}
