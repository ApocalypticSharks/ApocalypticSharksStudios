using NotSoWild.Core;
using NotSoWild.UI;
using NotSoWild.Visual;
using UnityEngine;

namespace NotSoWild.Gameplay
{
    public sealed class BuildInteractionController : MonoBehaviour
    {
        [SerializeField] Camera _camera;

        GameSession _session;
        BuildingManager _buildingManager;
        WorkforceManager _workforceManager;
        BuildingDefinition _selectedDefinition;
        BuildingView _preview;
        GridCoordinates _hoverCenter;
        bool _hasHover;
        PlacedBuildingRecord _selectedBuilding;
        ConstructionSite _selectedConstruction;
        ResidentRecord _pendingBuilder;

        public BuildingDefinition SelectedDefinition => _selectedDefinition;
        public PlacedBuildingRecord SelectedBuilding => _selectedBuilding;
        public ConstructionSite SelectedConstruction => _selectedConstruction;
        public bool IsPlacing => _selectedDefinition != null;
        public ResidentRecord PendingBuilder => _pendingBuilder;

        public void Initialize(
            GameSession session,
            BuildingManager buildingManager,
            WorkforceManager workforceManager,
            Camera camera)
        {
            _session = session;
            _buildingManager = buildingManager;
            _workforceManager = workforceManager;
            if (camera != null)
            {
                _camera = camera;
            }
        }

        void Update()
        {
            if (_session == null || _session.State == null || _camera == null)
            {
                return;
            }

            if (_session.Phase == SessionPhase.Defeat || _session.Phase == SessionPhase.Victory)
            {
                return;
            }

            if (GameUI.BlocksWorldInput)
            {
                return;
            }

            if (_session.ActiveVisitor != null &&
                _session.ActiveVisitor.IsDecisionPending &&
                (_session.ActiveVisitor.IsPointerOver || _session.ActiveVisitor.IsSelected))
            {
                return;
            }

            UpdateHoverCell();

            if (GameInputHelper.WasLeftMousePressedThisFrame)
            {
                HandlePrimaryClick();
            }

            if (GameInputHelper.WasEscapePressedThisFrame)
            {
                CancelPlacement();
                _selectedBuilding = null;
                _selectedConstruction = null;
                _pendingBuilder = null;
                _session.NotifyChanged();
            }
        }

        void UpdateHoverCell()
        {
            _hasHover = TryGetMouseCell(out _hoverCenter);
            if (!IsPlacing || _selectedDefinition == null || !_hasHover)
            {
                if (_preview != null)
                {
                    _preview.gameObject.SetActive(false);
                }

                return;
            }

            EnsurePreview();
            _preview.gameObject.SetActive(true);
            bool valid = _buildingManager.CanPlaceAt(_session.State, _selectedDefinition, _hoverCenter);
            _preview.SetPreview(valid);

            var grid = _session.Bootstrap.Grid;
            var origin = grid.GetFootprintOriginFromCenter(
                _hoverCenter,
                _selectedDefinition.Width,
                _selectedDefinition.Height);
            _preview.Setup(
                _selectedDefinition,
                grid,
                origin,
                _hoverCenter,
                _buildingManager.GetSpriteForDefinition(_selectedDefinition));
        }

        void EnsurePreview()
        {
            if (_preview != null || _selectedDefinition == null || _session.Bootstrap?.Grid == null)
            {
                return;
            }

            _preview = _buildingManager.CreatePreview(
                _selectedDefinition,
                _session.Bootstrap.Grid,
                _hoverCenter,
                transform);
        }

        void HandlePrimaryClick()
        {
            if (!_hasHover)
            {
                return;
            }

            if (IsPlacing && _selectedDefinition != null)
            {
                if (_pendingBuilder != null)
                {
                    TryConfirmPlacement(_pendingBuilder);
                }
                else if (WorkforceHelper.CountIdleWorkers(_session.State) == 1)
                {
                    foreach (var resident in _session.State.Residents)
                    {
                        if (WorkforceHelper.IsIdle(resident) && WorkforceHelper.CanBuild(resident))
                        {
                            TryConfirmPlacement(resident);
                            break;
                        }
                    }
                }
                else
                {
                    _session.State.AddLog("Choose a builder, then click the tile again.");
                    _session.NotifyChanged();
                }

                return;
            }

            var building = _session.State.FindBuildingAt(_hoverCenter);
            if (building != null)
            {
                _selectedBuilding = building;
                _selectedConstruction = null;
                _session.NotifyChanged();
                return;
            }

            var construction = _session.State.FindConstructionAt(_hoverCenter);
            if (construction != null)
            {
                _selectedBuilding = null;
                _selectedConstruction = construction;
                _session.NotifyChanged();
            }
        }

        public void BeginPlacement(BuildingDefinition definition)
        {
            _selectedDefinition = definition;
            _selectedBuilding = null;
            _selectedConstruction = null;
            _pendingBuilder = null;
            DestroyPreview();
            _session?.NotifyChanged();
        }

        public void CancelPlacement()
        {
            _selectedDefinition = null;
            _pendingBuilder = null;
            DestroyPreview();
            _session?.NotifyChanged();
        }

        public void SelectBuilderForPlacement(ResidentRecord builder)
        {
            if (!WorkforceHelper.IsIdle(builder) || !WorkforceHelper.CanBuild(builder))
            {
                return;
            }

            _pendingBuilder = builder;
            _session?.NotifyChanged();
        }

        public void TryConfirmPlacement(ResidentRecord builder)
        {
            if (!IsPlacing || !_hasHover || builder == null)
            {
                return;
            }

            if (_buildingManager.TryStartConstruction(_session.State, _selectedDefinition, _hoverCenter, builder))
            {
                CancelPlacement();
                _session.NotifyChanged();
            }
        }

        public void ClearSelectedBuilding()
        {
            _selectedBuilding = null;
            _selectedConstruction = null;
            _session?.NotifyChanged();
        }

        public void AssignBuilderToSelectedConstruction(ResidentRecord builder)
        {
            if (_selectedConstruction == null || builder == null)
            {
                return;
            }

            if (_buildingManager.TryAssignBuilder(_session.State, _selectedConstruction, builder))
            {
                _session.NotifyChanged();
            }
        }

        public void CancelSelectedConstruction()
        {
            if (_selectedConstruction == null)
            {
                return;
            }

            if (_buildingManager.CancelConstruction(_session.State, _selectedConstruction))
            {
                _selectedConstruction = null;
                _session.NotifyChanged();
            }
        }

        public void AssignWorkerToSelectedBuilding(ResidentRecord worker)
        {
            if (_selectedBuilding == null || worker == null)
            {
                return;
            }

            if (_workforceManager.TryAssignWorker(_session.State, _selectedBuilding, worker))
            {
                _session.NotifyChanged();
            }
        }

        public void UnassignFromSelectedBuilding()
        {
            if (_selectedBuilding == null)
            {
                return;
            }

            if (_workforceManager.TryUnassignBuilding(_session.State, _selectedBuilding))
            {
                _session.NotifyChanged();
            }
        }

        public void UnassignWorker(ResidentRecord worker)
        {
            if (worker == null)
            {
                return;
            }

            if (_workforceManager.TryUnassignWorker(_session.State, worker))
            {
                _session.NotifyChanged();
            }
        }

        public bool IsHoverValidForPlacement()
        {
            if (!IsPlacing || !_hasHover || _selectedDefinition == null)
            {
                return false;
            }

            return _buildingManager.CanPlaceAt(_session.State, _selectedDefinition, _hoverCenter);
        }

        public bool TryGetHoverCenter(out GridCoordinates center)
        {
            center = _hoverCenter;
            return _hasHover && IsPlacing;
        }

        bool TryGetMouseCell(out GridCoordinates coordinates)
        {
            coordinates = default;
            if (_session.Bootstrap?.Grid == null)
            {
                return false;
            }

            var world = _camera.ScreenToWorldPoint(GameInputHelper.MousePosition);
            return _session.Bootstrap.Grid.TryWorldToCell(world, out coordinates);
        }

        void DestroyPreview()
        {
            if (_preview != null)
            {
                Destroy(_preview.gameObject);
                _preview = null;
            }
        }

        void OnDestroy()
        {
            DestroyPreview();
        }
    }
}
