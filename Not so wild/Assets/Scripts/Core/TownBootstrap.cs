using NotSoWild.Core;
using NotSoWild.Gameplay;
using NotSoWild.Visual;
using UnityEngine;

namespace NotSoWild.Core
{
    public sealed class TownBootstrap : MonoBehaviour
    {
        [SerializeField] Camera _camera;
        [SerializeField] TownGridConfig _config;
        [SerializeField] TownGridRenderer _gridRenderer;
        [SerializeField] RoadEntryPoint _roadEntryPoint;
        [SerializeField] BuildingDefinition _startingTownHall;

        [Header("Gameplay")]
        [SerializeField] GameSessionConfig _gameSessionConfig;
        [SerializeField] VisitorCatalog _visitorCatalog;
        [SerializeField] BuildingCatalog _buildingCatalog;
        [SerializeField] GameObject _visitorPrefab;

        public TownGrid Grid { get; private set; }
        public RoadEntryPoint RoadEntry => _roadEntryPoint;
        public BuildingView TownHall { get; private set; }
        public GameSessionConfig GameSessionConfig => _gameSessionConfig;
        public VisitorCatalog VisitorCatalog => _visitorCatalog;
        public BuildingCatalog BuildingCatalog => _buildingCatalog;
        public GameObject VisitorPrefab => _visitorPrefab;

        void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            EnsureDefaultGameplayAssets();

            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<TownGridConfig>();
            }

            Grid = CreateGridFromCamera(_camera, _config);
            _gridRenderer.Build(Grid, Random.Range(int.MinValue, int.MaxValue));
            _roadEntryPoint.Initialize(Grid);
            PlaceStartingTownHall();
            EnsureGameplaySystems();
        }

        void EnsureDefaultGameplayAssets()
        {
            if (_gameSessionConfig == null)
            {
                _gameSessionConfig = Resources.Load<GameSessionConfig>("NotSoWild/DefaultGameSessionConfig");
            }

            if (_visitorCatalog == null)
            {
                _visitorCatalog = Resources.Load<VisitorCatalog>("NotSoWild/DefaultVisitorCatalog");
            }

            if (_buildingCatalog == null)
            {
                _buildingCatalog = Resources.Load<BuildingCatalog>("NotSoWild/DefaultBuildingCatalog");
            }

#if UNITY_EDITOR
            if (_gameSessionConfig == null)
            {
                _gameSessionConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<GameSessionConfig>(
                    "Assets/Resources/NotSoWild/DefaultGameSessionConfig.asset");
            }

            if (_visitorCatalog == null)
            {
                _visitorCatalog = UnityEditor.AssetDatabase.LoadAssetAtPath<VisitorCatalog>(
                    "Assets/Resources/NotSoWild/DefaultVisitorCatalog.asset");
            }

            if (_buildingCatalog == null)
            {
                _buildingCatalog = UnityEditor.AssetDatabase.LoadAssetAtPath<BuildingCatalog>(
                    "Assets/Resources/NotSoWild/DefaultBuildingCatalog.asset");
            }
#endif

#if UNITY_EDITOR
            if (_visitorPrefab == null)
            {
                _visitorPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/Prefabs/Cowboy.prefab");
            }
#endif
        }

        void EnsureGameplaySystems()
        {
            var session = GetComponent<GameSession>();
            if (session == null)
            {
                session = gameObject.AddComponent<GameSession>();
            }

            if (GetComponent<RaidBattleController>() == null)
            {
                gameObject.AddComponent<RaidBattleController>();
            }

            if (GetComponent<ResidentManager>() == null)
            {
                gameObject.AddComponent<ResidentManager>();
            }

            if (GetComponent<BuildingManager>() == null)
            {
                gameObject.AddComponent<BuildingManager>();
            }

            if (GetComponent<WorkforceManager>() == null)
            {
                gameObject.AddComponent<WorkforceManager>();
            }

            if (GetComponent<BuildInteractionController>() == null)
            {
                gameObject.AddComponent<BuildInteractionController>();
            }

            session.AssignDependencies(this, _gameSessionConfig, _visitorCatalog, _buildingCatalog, _visitorPrefab);

            if (GetComponent<NotSoWild.UI.GameUI>() == null)
            {
                gameObject.AddComponent<NotSoWild.UI.GameUI>();
            }
        }

        void PlaceStartingTownHall()
        {
            if (_startingTownHall == null || _startingTownHall.Sprite == null)
            {
                return;
            }

            var center = Grid.ResolveTownHallCenter(
                _config.StartingTownHallCenterX,
                _config.StartingTownHallCenterY,
                _startingTownHall.Width,
                _startingTownHall.Height);
            var origin = Grid.GetFootprintOriginFromCenter(
                center,
                _startingTownHall.Width,
                _startingTownHall.Height);
            if (!Grid.TryOccupyFootprint(origin, _startingTownHall.Width, _startingTownHall.Height))
            {
                Debug.LogError(
                    $"Could not place starting Town Hall at center {center} (origin {origin}).");
                return;
            }

            Debug.Log(
                $"Town Hall: center Tile_{center.X}_{center.Y}, " +
                $"world center {Grid.GetFootprintCenterWorldPosition(center)}, " +
                $"road row {Grid.RoadCenterRow}.");

            var townHallObject = new GameObject(_startingTownHall.DisplayName);
            townHallObject.transform.SetParent(transform, false);

            TownHall = townHallObject.AddComponent<BuildingView>();
            TownHall.Setup(_startingTownHall, Grid, origin, center);
        }

        public static TownGrid CreateGridFromCamera(Camera camera, TownGridConfig config)
        {
            int columns;
            int rows;

            if (config.UseFixedGridDimensions)
            {
                columns = Mathf.Max(3, config.GridColumns);
                rows = Mathf.Max(3, config.GridRows);
                if (rows % 2 == 0)
                {
                    rows += 1;
                }
            }
            else
            {
                float visibleHeight = camera.orthographicSize * 2f;
                float visibleWidth = visibleHeight * camera.aspect;

                float paddedWidth = visibleWidth - config.ScreenPadding * 2f;
                float paddedHeight = visibleHeight - config.ScreenPadding * 2f;

                columns = Mathf.Max(3, Mathf.FloorToInt(paddedWidth / config.CellSize));
                rows = Mathf.Max(3, Mathf.FloorToInt(paddedHeight / config.CellSize));

                if (rows % 2 == 0)
                {
                    rows += 1;
                }
            }

            float gridWidth = columns * config.CellSize;
            float gridHeight = rows * config.CellSize;
            var origin = new Vector2(-gridWidth * 0.5f, -gridHeight * 0.5f);

            return new TownGrid(columns, rows, config.CellSize, origin, config);
        }

        void OnValidate()
        {
            if (_gridRenderer == null)
            {
                _gridRenderer = GetComponentInChildren<TownGridRenderer>();
            }

            if (_roadEntryPoint == null)
            {
                _roadEntryPoint = GetComponentInChildren<RoadEntryPoint>();
            }
        }
    }
}
