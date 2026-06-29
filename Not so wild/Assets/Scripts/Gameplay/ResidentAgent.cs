using System.Collections.Generic;
using NotSoWild.Core;
using UnityEngine;

namespace NotSoWild.Gameplay
{
    public sealed class ResidentAgent : MonoBehaviour
    {
        const float WorkArriveDistance = 0.08f;
        const float PathWaypointDistance = 0.08f;
        const float SeparationRadius = 0.26f;
        const float SeparationStrength = 0.75f;
        const float StatusIndicatorIntervalSeconds = 15f;
        const float MinPatrolWaitSeconds = 1f;
        const float MaxPatrolWaitSeconds = 4f;

        enum PatrolPhase
        {
            Walking,
            Waiting
        }

        TownGrid _grid;
        ResidentRecord _record;
        VisitorDefinition _definition;
        float _minX;
        float _maxX;
        float _minY;
        float _maxY;
        float _speed = 1.4f;
        bool _patrolling;
        Vector3 _workTarget;
        bool _hasWorkTarget;
        PatrolPhase _patrolPhase;
        Vector3 _patrolTarget;
        float _patrolWaitRemaining;
        readonly List<Vector3> _path = new();
        int _pathIndex;
        ResidentStatusIndicatorController _statusIndicators;
        ResidentAnimationController _animationController;
        ResidentHealthBarController _healthBar;
        StatusSnapshot _lastStatusSnapshot;
        float _statusIndicatorTimer;
        bool _hasStatusSnapshot;

        public VisitorDefinition Definition => _definition;
        public ResidentRecord Record => _record;

        public void Initialize(
            TownGrid grid,
            VisitorDefinition definition,
            Vector3 startPosition,
            float walkSpeed,
            ResidentRecord record = null)
        {
            _grid = grid;
            _definition = definition;
            _record = record;
            _speed = walkSpeed;
            grid.GetRoadPatrolBounds(out _minX, out _maxX, out _minY, out _maxY);

            var position = startPosition;
            position.y = Mathf.Clamp(position.y, _minY, _maxY);
            position.z = -0.04f;
            transform.position = position;

            if (definition != null)
            {
                name = definition.DisplayName;
            }

            EnsureStatusIndicators();
            EnsureAnimationController();
            EnsureHealthBar();
            ForceShowStatusIndicators();
            ApplyWorkState();
        }

        public void BindRecord(ResidentRecord record)
        {
            _record = record;
            ForceShowStatusIndicators();
            ApplyWorkState();
        }

        public void ApplyWorkState()
        {
            _patrolling = false;
            _hasWorkTarget = false;

            if (_record == null || _grid == null)
            {
                _patrolling = true;
                return;
            }

            switch (_record.WorkState)
            {
                case ResidentWorkState.Idle:
                    _patrolling = true;
                    break;
                case ResidentWorkState.Building:
                    if (_record.ConstructionSite?.Definition != null)
                    {
                        var site = _record.ConstructionSite;
                        SetWorkTarget(_grid.GetBuildingWorkAnchorWorld(
                            site.Center,
                            site.Definition.Width,
                            site.Definition.Height));
                    }
                    else
                    {
                        _patrolling = true;
                    }

                    break;
                case ResidentWorkState.Working:
                    if (_record.AssignedBuilding?.Definition != null)
                    {
                        var building = _record.AssignedBuilding;
                        SetWorkTarget(_grid.GetBuildingWorkAnchorWorld(
                            building.Center,
                            building.Definition.Width,
                            building.Definition.Height));
                    }
                    else
                    {
                        _patrolling = true;
                    }

                    break;
            }

            if (_patrolling)
            {
                StartPatrol();
            }
            else if (_hasWorkTarget)
            {
                UpdateFacingToward(_workTarget.x - transform.position.x);
            }
        }

        void SetWorkTarget(Vector3 target)
        {
            _workTarget = target;
            _workTarget.z = -0.04f;
            _hasWorkTarget = true;
            BuildPathTo(_workTarget);
        }

        void Update()
        {
            UpdateStatusIndicators();

            if (_grid == null)
            {
                SetWalking(false);
                return;
            }

            if (_patrolling)
            {
                UpdatePatrol();
                return;
            }

            if (!_hasWorkTarget)
            {
                SetWalking(false);
                return;
            }

            var position = transform.position;
            if (Vector2.Distance(position, _workTarget) <= WorkArriveDistance)
            {
                position.x = _workTarget.x;
                position.y = _workTarget.y;
                transform.position = position;
                UpdateFacingToward(_workTarget.x - position.x);
                SetWalking(false);
                return;
            }

            var direction = GetMoveDirection(position, _workTarget);
            transform.position = MovePosition(position, direction);
            UpdateFacingToward(direction.x);
            SetWalking(direction.sqrMagnitude > 0.0001f);
        }

        void UpdatePatrol()
        {
            if (_patrolPhase == PatrolPhase.Waiting)
            {
                SetWalking(false);
                _patrolWaitRemaining -= Time.deltaTime;
                if (_patrolWaitRemaining <= 0f)
                {
                    _patrolPhase = PatrolPhase.Walking;
                    PickRandomPatrolTarget();
                }

                return;
            }

            var position = transform.position;
            if (Vector2.Distance(position, _patrolTarget) <= WorkArriveDistance)
            {
                transform.position = _patrolTarget;
                _patrolPhase = PatrolPhase.Waiting;
                _patrolWaitRemaining = Random.Range(MinPatrolWaitSeconds, MaxPatrolWaitSeconds);
                SetWalking(false);
                return;
            }

            var direction = GetMoveDirection(position, _patrolTarget);
            transform.position = MovePosition(position, direction);
            UpdateFacingToward(direction.x);
            SetWalking(direction.sqrMagnitude > 0.0001f);
        }

        void StartPatrol()
        {
            _patrolPhase = PatrolPhase.Walking;
            PickRandomPatrolTarget();
        }

        void PickRandomPatrolTarget()
        {
            _patrolTarget = new Vector3(
                Random.Range(_minX, _maxX),
                Random.Range(_minY, _maxY),
                -0.04f);
            BuildPathTo(_patrolTarget);
        }

        Vector3 GetMoveDirection(Vector3 position, Vector3 destination)
        {
            if (_path.Count == 0)
            {
                BuildPathTo(destination);
            }

            Vector3 waypoint = destination;
            while (_pathIndex < _path.Count)
            {
                waypoint = _path[_pathIndex];
                if (Vector2.Distance(position, waypoint) > PathWaypointDistance)
                {
                    break;
                }

                _pathIndex++;
            }

            if (_pathIndex >= _path.Count)
            {
                waypoint = destination;
            }

            var desired = waypoint - position;
            desired.z = 0f;
            if (desired.sqrMagnitude > 0.0001f)
            {
                desired.Normalize();
            }

            desired += GetSeparationVector(position) * SeparationStrength;
            desired.z = 0f;
            return desired.sqrMagnitude > 0.0001f ? desired.normalized : Vector3.zero;
        }

        Vector3 MovePosition(Vector3 position, Vector3 direction)
        {
            Vector3 step = direction * (_speed * Time.deltaTime);
            Vector3 next = position + step;
            next.z = -0.04f;
            if (IsWorldWalkable(next))
            {
                return next;
            }

            Vector3 xOnly = new Vector3(position.x + step.x, position.y, -0.04f);
            if (IsWorldWalkable(xOnly))
            {
                return xOnly;
            }

            Vector3 yOnly = new Vector3(position.x, position.y + step.y, -0.04f);
            if (IsWorldWalkable(yOnly))
            {
                return yOnly;
            }

            position.z = -0.04f;
            return position;
        }

        Vector3 GetSeparationVector(Vector3 position)
        {
            Vector3 separation = Vector3.zero;
            foreach (var other in FindObjectsByType<ResidentAgent>())
            {
                if (other == null || other == this)
                {
                    continue;
                }

                Vector3 delta = position - other.transform.position;
                delta.z = 0f;
                float distance = delta.magnitude;
                if (distance <= 0.001f || distance > SeparationRadius)
                {
                    continue;
                }

                separation += delta.normalized * ((SeparationRadius - distance) / SeparationRadius);
            }

            return separation;
        }

        void BuildPathTo(Vector3 destination)
        {
            _path.Clear();
            _pathIndex = 0;

            if (_grid == null ||
                !_grid.TryWorldToCell(transform.position, out var start) ||
                !_grid.TryWorldToCell(destination, out var goal))
            {
                return;
            }

            if (!FindNearestWalkable(goal, out goal))
            {
                return;
            }

            if (start == goal)
            {
                _path.Add(destination);
                return;
            }

            var queue = new Queue<GridCoordinates>();
            var visited = new bool[_grid.Columns, _grid.Rows];
            var previous = new GridCoordinates[_grid.Columns, _grid.Rows];

            visited[start.X, start.Y] = true;
            queue.Enqueue(start);
            bool found = false;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == goal)
                {
                    found = true;
                    break;
                }

                TryVisit(current, new GridCoordinates(current.X + 1, current.Y), goal, queue, visited, previous);
                TryVisit(current, new GridCoordinates(current.X - 1, current.Y), goal, queue, visited, previous);
                TryVisit(current, new GridCoordinates(current.X, current.Y + 1), goal, queue, visited, previous);
                TryVisit(current, new GridCoordinates(current.X, current.Y - 1), goal, queue, visited, previous);
            }

            if (!found)
            {
                return;
            }

            var cells = new List<GridCoordinates>();
            var step = goal;
            while (step != start)
            {
                cells.Add(step);
                step = previous[step.X, step.Y];
            }

            cells.Reverse();
            foreach (var cell in cells)
            {
                var point = _grid.CellToWorldCenter(cell);
                point.z = -0.04f;
                _path.Add(point);
            }

            destination.z = -0.04f;
            _path.Add(destination);
        }

        void TryVisit(
            GridCoordinates from,
            GridCoordinates next,
            GridCoordinates goal,
            Queue<GridCoordinates> queue,
            bool[,] visited,
            GridCoordinates[,] previous)
        {
            if (!_grid.IsInside(next) || visited[next.X, next.Y])
            {
                return;
            }

            if (!IsWalkable(next) && next != goal)
            {
                return;
            }

            visited[next.X, next.Y] = true;
            previous[next.X, next.Y] = from;
            queue.Enqueue(next);
        }

        bool FindNearestWalkable(GridCoordinates source, out GridCoordinates result)
        {
            if (IsWalkable(source))
            {
                result = source;
                return true;
            }

            for (int radius = 1; radius <= 3; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius)
                        {
                            continue;
                        }

                        var candidate = new GridCoordinates(source.X + dx, source.Y + dy);
                        if (IsWalkable(candidate))
                        {
                            result = candidate;
                            return true;
                        }
                    }
                }
            }

            result = source;
            return false;
        }

        bool IsWalkable(GridCoordinates coordinates)
        {
            if (!_grid.IsInside(coordinates))
            {
                return false;
            }

            if (_grid.IsRoad(coordinates))
            {
                return true;
            }

            return _grid.IsBuildable(coordinates) && !_grid.IsOccupied(coordinates);
        }

        bool IsWorldWalkable(Vector3 position)
        {
            return _grid.TryWorldToCell(position, out var cell) && IsWalkable(cell);
        }

        readonly struct StatusSnapshot
        {
            readonly int _health;
            readonly int _mood;
            readonly int _stress;
            readonly ResidentWorkState _workState;
            readonly WeaponType _weapon;

            StatusSnapshot(
                int health,
                int mood,
                int stress,
                ResidentWorkState workState,
                WeaponType weapon)
            {
                _health = health;
                _mood = mood;
                _stress = stress;
                _workState = workState;
                _weapon = weapon;
            }

            public static StatusSnapshot From(ResidentRecord record)
            {
                var stats = record.Stats;
                return new StatusSnapshot(
                    stats.Health,
                    stats.Mood,
                    stats.Stress,
                    record.WorkState,
                    record.EquippedWeapon);
            }

            public bool Equals(StatusSnapshot other)
            {
                return _health == other._health &&
                       _mood == other._mood &&
                       _stress == other._stress &&
                       _workState == other._workState &&
                       _weapon == other._weapon;
            }
        }

        public void SetPatrolling(bool enabled)
        {
            _patrolling = enabled;
            if (enabled)
            {
                _hasWorkTarget = false;
                StartPatrol();
            }
        }

        void EnsureStatusIndicators()
        {
            if (_statusIndicators != null)
            {
                return;
            }

            _statusIndicators = GetComponent<ResidentStatusIndicatorController>();
            if (_statusIndicators == null)
            {
                _statusIndicators = gameObject.AddComponent<ResidentStatusIndicatorController>();
            }
        }

        void EnsureAnimationController()
        {
            if (_animationController != null)
            {
                return;
            }

            _animationController = GetComponent<ResidentAnimationController>();
            if (_animationController == null)
            {
                _animationController = gameObject.AddComponent<ResidentAnimationController>();
            }
        }

        void EnsureHealthBar()
        {
            if (_healthBar != null)
            {
                return;
            }

            _healthBar = GetComponent<ResidentHealthBarController>();
            if (_healthBar == null)
            {
                _healthBar = gameObject.AddComponent<ResidentHealthBarController>();
            }
        }

        void SetWalking(bool walking)
        {
            EnsureAnimationController();
            _animationController.SetWalking(walking);
        }

        void UpdateStatusIndicators()
        {
            if (_record?.Stats == null)
            {
                return;
            }

            EnsureStatusIndicators();
            _statusIndicatorTimer -= Time.deltaTime;
            var snapshot = StatusSnapshot.From(_record);
            bool changed = !_hasStatusSnapshot || !snapshot.Equals(_lastStatusSnapshot);

            if (!changed && _statusIndicatorTimer > 0f)
            {
                return;
            }

            _lastStatusSnapshot = snapshot;
            _hasStatusSnapshot = true;
            _statusIndicatorTimer = StatusIndicatorIntervalSeconds;
            _statusIndicators.Show(_record);
        }

        void ForceShowStatusIndicators()
        {
            if (_record?.Stats == null)
            {
                return;
            }

            EnsureStatusIndicators();
            _lastStatusSnapshot = StatusSnapshot.From(_record);
            _hasStatusSnapshot = true;
            _statusIndicatorTimer = StatusIndicatorIntervalSeconds;
            _statusIndicators.Show(_record);
        }

        public CombatUnit EnterCombat(
            int hp,
            int attack,
            int accuracy,
            float moveSpeed,
            float attackRange,
            float attackCooldown,
            int targetCount,
            int multiTargetAccuracyPenalty,
            bool accuracyScalesWithDistance,
            int meleeDamageBonus)
        {
            SetPatrolling(false);
            _hasWorkTarget = false;

            var unit = GetComponent<CombatUnit>();
            if (unit == null)
            {
                unit = gameObject.AddComponent<CombatUnit>();
            }

            unit.Initialize(
                false,
                hp,
                attack,
                accuracy,
                moveSpeed,
                _definition?.DisplayName ?? name,
                attackRange,
                attackCooldown,
                targetCount,
                multiTargetAccuracyPenalty,
                accuracyScalesWithDistance,
                meleeDamageBonus,
                _record,
                _definition?.UnitClass);
            return unit;
        }

        public void ExitCombat()
        {
            var unit = GetComponent<CombatUnit>();
            if (unit != null)
            {
                Destroy(unit);
            }

            var renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = Color.white;
            }

            ApplyWorkState();
        }

        void UpdateFacingToward(float xDelta)
        {
            var renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.flipX = xDelta < 0f;
            }
        }
    }
}
