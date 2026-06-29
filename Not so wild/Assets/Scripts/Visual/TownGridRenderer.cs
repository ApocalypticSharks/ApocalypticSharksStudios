using NotSoWild.Core;
using UnityEngine;

namespace NotSoWild.Visual
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class TownGridRenderer : MonoBehaviour
    {
        static Sprite _sharedSprite;

        [Header("Tileset")]
        [SerializeField] GroundTileset _groundTileset;

        [Header("Fallback Colors")]
        [SerializeField] Color _backgroundColor = new(0.72f, 0.62f, 0.42f, 1f);
        [SerializeField] Color _roadColor = new(0.45f, 0.36f, 0.24f, 1f);
        [SerializeField] Color _buildableColor = new(0.55f, 0.62f, 0.38f, 0.35f);
        [SerializeField] Color _gridLineColor = new(1f, 1f, 1f, 0.12f);
        [SerializeField] bool _showGridLines;

        Transform _cellsRoot;
        TownGrid _grid;
        int _tileSeed;

        public void Build(TownGrid grid, int tileSeed)
        {
            _grid = grid;
            _tileSeed = tileSeed;
            EnsureCellsRoot();
            ClearChildren(_cellsRoot);

            if (_groundTileset != null)
            {
                CreateTilesetCells();
            }
            else
            {
                CreateBackground();
                CreateFallbackCells();
            }

            if (_showGridLines)
            {
                CreateGridLines();
            }
        }

        void EnsureCellsRoot()
        {
            if (_cellsRoot != null)
            {
                return;
            }

            var root = new GameObject("GridCells");
            root.transform.SetParent(transform, false);
            _cellsRoot = root.transform;
        }

        void CreateTilesetCells()
        {
            for (int x = 0; x < _grid.Columns; x++)
            {
                for (int y = 0; y < _grid.Rows; y++)
                {
                    var sprite = _groundTileset.GetSprite(x, y, _grid.RoadCenterRow, _tileSeed);
                    if (sprite == null)
                    {
                        continue;
                    }

                    CreateTileSprite($"Tile_{x}_{y}", _grid.CellToWorldCenter(new GridCoordinates(x, y)), sprite, 0);
                }
            }
        }

        void CreateBackground()
        {
            float width = _grid.Columns * _grid.CellSize;
            float height = _grid.Rows * _grid.CellSize;
            var center = new Vector3(
                _grid.Origin.x + width * 0.5f,
                _grid.Origin.y + height * 0.5f,
                0.1f);

            CreateColoredSprite("Background", center, new Vector2(width, height), _backgroundColor, -1);
        }

        void CreateFallbackCells()
        {
            var cellScale = new Vector2(_grid.CellSize * 0.98f, _grid.CellSize * 0.98f);

            foreach (var coordinates in _grid.GetRoadCells())
            {
                CreateColoredSprite(
                    $"Road_{coordinates.X}_{coordinates.Y}",
                    _grid.CellToWorldCenter(coordinates),
                    cellScale,
                    _roadColor,
                    0);
            }

            foreach (var coordinates in _grid.GetBuildableCells())
            {
                CreateColoredSprite(
                    $"Build_{coordinates.X}_{coordinates.Y}",
                    _grid.CellToWorldCenter(coordinates),
                    cellScale,
                    _buildableColor,
                    1);
            }
        }

        void CreateGridLines()
        {
            float width = _grid.Columns * _grid.CellSize;
            float height = _grid.Rows * _grid.CellSize;
            float lineThickness = Mathf.Max(0.02f, _grid.CellSize * 0.03f);

            for (int x = 0; x <= _grid.Columns; x++)
            {
                float worldX = _grid.Origin.x + x * _grid.CellSize;
                CreateColoredSprite(
                    $"GridV_{x}",
                    new Vector3(worldX, _grid.Origin.y + height * 0.5f, -0.5f),
                    new Vector2(lineThickness, height),
                    _gridLineColor,
                    3);
            }

            for (int y = 0; y <= _grid.Rows; y++)
            {
                float worldY = _grid.Origin.y + y * _grid.CellSize;
                CreateColoredSprite(
                    $"GridH_{y}",
                    new Vector3(_grid.Origin.x + width * 0.5f, worldY, -0.5f),
                    new Vector2(width, lineThickness),
                    _gridLineColor,
                    3);
            }
        }

        void CreateTileSprite(string name, Vector3 position, Sprite sprite, int sortingOrder)
        {
            var cellObject = new GameObject(name);
            cellObject.transform.SetParent(_cellsRoot, false);
            cellObject.transform.position = position;

            var renderer = cellObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
        }

        void CreateColoredSprite(string name, Vector3 position, Vector2 size, Color color, int sortingOrder)
        {
            var cellObject = new GameObject(name);
            cellObject.transform.SetParent(_cellsRoot, false);
            cellObject.transform.position = position;

            var renderer = cellObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSharedSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            cellObject.transform.localScale = new Vector3(size.x, size.y, 1f);
        }

        static Sprite GetSharedSprite()
        {
            if (_sharedSprite != null)
            {
                return _sharedSprite;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.filterMode = FilterMode.Point;

            _sharedSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);

            return _sharedSprite;
        }

        static void ClearChildren(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Destroy(root.GetChild(i).gameObject);
            }
        }
    }
}
