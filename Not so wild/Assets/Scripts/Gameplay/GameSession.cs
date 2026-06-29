using System;
using System.Collections;
using NotSoWild.Core;
using UnityEngine;

namespace NotSoWild.Gameplay
{
    public enum SessionPhase
    {
        Playing,
        AwaitingDecision,
        ResolvingRaid,
        Victory,
        Defeat
    }

    public sealed class GameSession : MonoBehaviour
    {
        [SerializeField] TownBootstrap _bootstrap;
        [SerializeField] GameSessionConfig _config;
        [SerializeField] VisitorCatalog _catalog;
        [SerializeField] BuildingCatalog _buildingCatalog;
        [SerializeField] GameObject _visitorPrefab;

        RaidBattleController _raidBattle;
        ResidentManager _residentManager;
        BuildingManager _buildingManager;
        WorkforceManager _workforceManager;
        BuildInteractionController _buildInteraction;
        float _spawnTimer;
        float _decisionTimeRemaining;
        float _townTickTimer;
        float _raidTimer;
        VisitorAgent _activeVisitor;

        public TownState State { get; private set; }
        public SessionPhase Phase { get; private set; } = SessionPhase.Playing;
        public VisitorAgent ActiveVisitor => _activeVisitor;
        public GameSessionConfig Config => _config;
        public TownBootstrap Bootstrap => _bootstrap;
        public BuildingCatalog BuildingCatalog => _buildingCatalog;
        public BuildInteractionController BuildInteraction => _buildInteraction;
        public bool CanStartConstruction => WorkforceHelper.CanStartConstruction(State);
        public float DecisionTimeRemaining => _decisionTimeRemaining;
        public float DecisionTimeLimit => _config != null ? _config.DecisionTimeLimit : 15f;

        public event Action StateChanged;
        public event Action<VisitorDefinition> DecisionRequested;
        public event Action SessionEnded;

        void Awake()
        {
            _raidBattle = GetComponent<RaidBattleController>();
            if (_raidBattle == null)
            {
                _raidBattle = gameObject.AddComponent<RaidBattleController>();
            }

            _residentManager = GetComponent<ResidentManager>();
            if (_residentManager == null)
            {
                _residentManager = gameObject.AddComponent<ResidentManager>();
            }

            _buildingManager = GetComponent<BuildingManager>();
            if (_buildingManager == null)
            {
                _buildingManager = gameObject.AddComponent<BuildingManager>();
            }

            _workforceManager = GetComponent<WorkforceManager>();
            if (_workforceManager == null)
            {
                _workforceManager = gameObject.AddComponent<WorkforceManager>();
            }

            _buildInteraction = GetComponent<BuildInteractionController>();
            if (_buildInteraction == null)
            {
                _buildInteraction = gameObject.AddComponent<BuildInteractionController>();
            }
        }

        void Start()
        {
            if (_bootstrap == null)
            {
                _bootstrap = GetComponent<TownBootstrap>();
            }

            if (_bootstrap != null)
            {
                AssignDependencies(
                    _bootstrap,
                    _bootstrap.GameSessionConfig,
                    _bootstrap.VisitorCatalog,
                    _bootstrap.BuildingCatalog,
                    _bootstrap.VisitorPrefab);
            }

            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<GameSessionConfig>();
            }

            if (_buildingCatalog == null)
            {
                _buildingCatalog = Resources.Load<BuildingCatalog>("NotSoWild/DefaultBuildingCatalog");
            }

            if (_catalog == null)
            {
                Debug.LogWarning("GameSession: VisitorCatalog is not assigned. Visitors will not spawn.");
            }
            else
            {
                Debug.Log($"GameSession started. Residents pool: {_catalog.ResidentCandidates?.Length ?? 0}.");
            }

            State = new TownState
            {
                Gold = _config.StartingGold,
                Reputation = _config.StartingReputation,
                Defense = _config.StartingDefense,
                MaxResidents = _config.MaxResidents,
                TargetTimeSeconds = _config.TargetTimeSeconds
            };
            State.AddLog("The survival timer has started.");
            State.AddLog("Accept a resident to build. Mayor runs Town Hall but cannot construct.");
            _residentManager.Initialize(_bootstrap, _visitorPrefab, _config.ResidentPatrolSpeed);
            _buildingManager.Initialize(_bootstrap, _residentManager);
            _workforceManager.Initialize(_residentManager);
            _buildInteraction.Initialize(this, _buildingManager, _workforceManager, Camera.main);
            AddStartingMayor();
            _townTickTimer = _config.TownTickIntervalSeconds;
            ScheduleNextRaid();
            ScheduleNextVisitorSpawn();
            NotifyChanged();
        }

        void AddStartingMayor()
        {
            var mayor = ResolveStartingMayor();
            if (mayor == null)
            {
                return;
            }

            var record = new ResidentRecord(mayor);
            State.Residents.Add(record);
            ResidentStatsHelper.Initialize(record);
            ApplyTraits(mayor);
            if (_bootstrap?.Grid != null)
            {
                var grid = _bootstrap.Grid;
                grid.GetRoadPatrolBounds(out float minX, out float maxX, out float minY, out float maxY);
                float roadY = (minY + maxY) * 0.5f;
                float spawnX = _bootstrap.TownHall != null
                    ? grid.CellToWorldCenter(_bootstrap.TownHall.Center).x
                    : (minX + maxX) * 0.5f;
                record.Agent = _residentManager.SpawnResident(mayor, new Vector3(spawnX, roadY, -0.04f), record);
            }

            State.AddLog($"{mayor.DisplayName} runs the town from Town Hall.");
        }

        VisitorDefinition ResolveStartingMayor()
        {
            if (_config?.StartingMayor != null)
            {
                return _config.StartingMayor;
            }

#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<VisitorDefinition>(
                "Assets/Settings/Visitors/Visitor_Mayor.asset");
#else
            return null;
#endif
        }

        void Update()
        {
            if (_raidBattle.IsRunning)
            {
                return;
            }

            if (Phase == SessionPhase.Playing || Phase == SessionPhase.AwaitingDecision)
            {
                AdvanceSessionTimer(Time.deltaTime);
                _buildingManager.AdvanceConstruction(State, Time.deltaTime);
            }

            if (Phase == SessionPhase.AwaitingDecision)
            {
                UpdateDecisionTimer();
                return;
            }

            if (Phase != SessionPhase.Playing || _bootstrap?.Grid == null)
            {
                return;
            }

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0f)
            {
                return;
            }

            if (_activeVisitor != null)
            {
                return;
            }

            TrySpawnVisitor();
        }

        void UpdateDecisionTimer()
        {
            _decisionTimeRemaining -= Time.deltaTime;
            if (_decisionTimeRemaining > 0f)
            {
                return;
            }

            RejectCurrentVisitor(timedOut: true);
        }

        void TrySpawnVisitor()
        {
            var definition = PickNextVisitor();
            if (definition == null)
            {
                State.AddLog("No travelers came by this time.");
                ScheduleNextVisitorSpawn();
                return;
            }

            SpawnVisitor(definition);
            ScheduleNextVisitorSpawn();
        }

        void AdvanceSessionTimer(float deltaTime)
        {
            if (Phase == SessionPhase.Defeat || Phase == SessionPhase.Victory)
            {
                return;
            }

            State.ElapsedTimeSeconds += deltaTime;
            _townTickTimer -= deltaTime;
            _raidTimer -= deltaTime;

            if (State.ElapsedTimeSeconds >= State.TargetTimeSeconds)
            {
                EndSession(true, $"The town survived for {FormatTime(State.TargetTimeSeconds)}.");
                return;
            }

            if (_townTickTimer <= 0f)
            {
                _townTickTimer += Mathf.Max(1f, _config.TownTickIntervalSeconds);
                ProcessTownTick();
            }

            if (_raidTimer <= 0f && Phase == SessionPhase.Playing)
            {
                State.RaidCount++;
                ScheduleNextRaid();
                StartCoroutine(RunRaidBattle());
            }
        }

        void ScheduleNextRaid()
        {
            float min = Mathf.Max(1f, _config.RaidIntervalMinSeconds);
            float max = Mathf.Max(min, _config.RaidIntervalMaxSeconds);
            _raidTimer = UnityEngine.Random.Range(min, max);
        }

        void ScheduleNextVisitorSpawn()
        {
            if (State != null && State.ElapsedTimeSeconds <= 0.01f)
            {
                _spawnTimer = 1f;
                return;
            }

            float min = Mathf.Max(0f, _config.VisitorSpawnIntervalMin);
            float max = Mathf.Max(min, _config.VisitorSpawnIntervalMax);
            _spawnTimer = UnityEngine.Random.Range(min, max);
        }

        VisitorDefinition PickNextVisitor()
        {
            if (_catalog == null)
            {
                return null;
            }

            bool preferEvent = State.IsResidentCapReached || UnityEngine.Random.value < 0.25f;
            if (preferEvent && _catalog.EventVisitors != null && _catalog.EventVisitors.Length > 0)
            {
                return _catalog.EventVisitors[UnityEngine.Random.Range(0, _catalog.EventVisitors.Length)];
            }

            if (_catalog.ResidentCandidates != null && _catalog.ResidentCandidates.Length > 0 &&
                !State.IsResidentCapReached)
            {
                var candidate = PickReputationGatedResident();
                if (candidate != null)
                {
                    return candidate;
                }
            }

            if (_catalog.EventVisitors != null && _catalog.EventVisitors.Length > 0)
            {
                return _catalog.EventVisitors[UnityEngine.Random.Range(0, _catalog.EventVisitors.Length)];
            }

            return null;
        }

        VisitorDefinition PickReputationGatedResident()
        {
            int reputationLevel = Mathf.Max(1, State.Reputation);
            var eligible = new System.Collections.Generic.List<VisitorDefinition>();
            foreach (var candidate in _catalog.ResidentCandidates)
            {
                if (candidate == null)
                {
                    continue;
                }

                var unitClass = candidate.UnitClass;
                if (unitClass == null || unitClass.Tier <= reputationLevel)
                {
                    eligible.Add(candidate);
                }
            }

            if (eligible.Count == 0)
            {
                return null;
            }

            return eligible[UnityEngine.Random.Range(0, eligible.Count)];
        }

        void SpawnVisitor(VisitorDefinition definition)
        {
            var grid = _bootstrap.Grid;
            var entry = _bootstrap.RoadEntry;
            var center = _bootstrap.TownHall != null
                ? _bootstrap.TownHall.Center
                : new GridCoordinates(_bootstrap.Grid.Columns / 2, grid.RoadCenterRow + 1);

            float stopX = grid.CellToWorldCenter(center).x;
            float leaveX = grid.CellToWorldCenter(new GridCoordinates(0, grid.RoadCenterRow)).x - 2f;
            var spawnPosition = entry.Position;
            spawnPosition.z = -0.05f;

            GameObject visitorObject;
            if (_visitorPrefab != null)
            {
                visitorObject = Instantiate(_visitorPrefab, spawnPosition, Quaternion.identity, transform);
                visitorObject.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
                var renderer = visitorObject.GetComponentInChildren<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.flipX = true;
                    renderer.sortingOrder = 20;
                }
            }
            else
            {
                visitorObject = new GameObject(definition.DisplayName);
                visitorObject.transform.SetParent(transform, false);
                visitorObject.transform.position = spawnPosition;
                var renderer = visitorObject.AddComponent<SpriteRenderer>();
                renderer.sprite = CreateFallbackSprite();
                renderer.sortingOrder = 20;
                visitorObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            }

            visitorObject.name = definition.DisplayName;
            var agent = visitorObject.GetComponent<VisitorAgent>();
            if (agent == null)
            {
                agent = visitorObject.AddComponent<VisitorAgent>();
            }

            agent.Initialize(definition, spawnPosition, stopX, leaveX, _config.VisitorMoveSpeed);
            agent.Arrived += OnVisitorArrived;
            agent.ChoiceMade += OnVisitorChoiceMade;
            _activeVisitor = agent;
        }

        void OnVisitorArrived(VisitorAgent agent)
        {
            agent.Arrived -= OnVisitorArrived;
            if (agent.Definition.Kind != VisitorKind.Event)
            {
                agent.BeginDecision();
                NotifyChanged();
                return;
            }

            Phase = SessionPhase.AwaitingDecision;
            _decisionTimeRemaining = DecisionTimeLimit;
            DecisionRequested?.Invoke(agent.Definition);
            NotifyChanged();
        }

        void OnVisitorChoiceMade(VisitorAgent agent, bool accepted)
        {
            if (agent == null || agent != _activeVisitor)
            {
                return;
            }

            if (accepted)
            {
                AcceptCurrentVisitor();
            }
            else
            {
                RejectCurrentVisitor();
            }
        }

        public void AcceptCurrentVisitor()
        {
            if (_activeVisitor == null)
            {
                return;
            }

            var definition = _activeVisitor.Definition;
            if (definition.Kind == VisitorKind.Event || definition.Kind == VisitorKind.BuilderEvent)
            {
                if (definition.Kind == VisitorKind.BuilderEvent)
                {
                    AcceptBuilderEvent(definition);
                }
                else
                {
                    ApplyEventChoice(
                        definition.AcceptGold,
                        definition.AcceptReputation,
                        definition.AcceptHeat,
                        definition.AcceptLabel);
                }
            }
            else
            {
                AcceptResident(definition);
            }

            FinishVisitorDecision();
        }

        public void RejectCurrentVisitor(bool timedOut = false)
        {
            if (_activeVisitor == null)
            {
                return;
            }

            var definition = _activeVisitor.Definition;
            if (definition.Kind == VisitorKind.Event || definition.Kind == VisitorKind.BuilderEvent)
            {
                ApplyEventChoice(
                    definition.RejectGold,
                    definition.RejectReputation,
                    definition.RejectHeat,
                    timedOut ? $"{definition.RejectLabel} (time expired)" : definition.RejectLabel);
            }
            else
            {
                State.Reputation = Mathf.Max(0, State.Reputation - definition.RejectReputationPenalty);
                if (timedOut)
                {
                    State.AddLog(
                        $"{definition.DisplayName} left — no answer in time. Reputation -{definition.RejectReputationPenalty}.");
                }
                else
                {
                    State.AddLog($"{definition.DisplayName} was turned away. Reputation -{definition.RejectReputationPenalty}.");
                }
            }

            FinishVisitorDecision();
        }

        void AcceptResident(VisitorDefinition definition)
        {
            if (State.IsResidentCapReached)
            {
                State.AddLog($"No room for {definition.DisplayName}.");
                return;
            }

            var record = new ResidentRecord(definition);
            State.Residents.Add(record);
            ResidentStatsHelper.Initialize(record);
            State.Reputation += definition.AcceptReputationBonus;
            ApplyTraits(definition);
            _activeVisitor.EndDecision();
            record.Agent = _residentManager.AdoptVisitor(_activeVisitor, record);
            _activeVisitor = null;
            State.AddLog($"{definition.DisplayName} joined the town.");
            NotifyChanged();
        }

        void ApplyTraits(VisitorDefinition definition)
        {
            if (definition.Traits == null)
            {
                return;
            }

            foreach (var trait in definition.Traits)
            {
                if (trait == null)
                {
                    continue;
                }

                State.Reputation += trait.ReputationBonus;
                State.Heat += trait.HeatBonus;
            }
        }

        void ApplyEventChoice(int gold, int reputation, int heat, string label)
        {
            State.Gold += gold;
            State.Reputation += reputation;
            State.Heat += heat;
            State.AddLog($"{label}: gold {FormatDelta(gold)}, rep {FormatDelta(reputation)}.");
            NotifyChanged();
        }

        void AcceptBuilderEvent(VisitorDefinition definition)
        {
            ApplyEventChoice(
                definition.AcceptGold,
                definition.AcceptReputation,
                definition.AcceptHeat,
                definition.OfferedBuilding != null
                    ? $"{definition.AcceptLabel}: {definition.OfferedBuilding.DisplayName} plans ready"
                    : definition.AcceptLabel);

            if (definition.OfferedBuilding != null && _buildInteraction != null)
            {
                _buildInteraction.BeginPlacement(definition.OfferedBuilding);
            }
        }

        static string FormatDelta(int value) => value >= 0 ? $"+{value}" : value.ToString();

        void FinishVisitorDecision()
        {
            if (_activeVisitor != null)
            {
                _activeVisitor.ChoiceMade -= OnVisitorChoiceMade;
                _activeVisitor.LeaveTown();
                _activeVisitor = null;
            }

            Phase = SessionPhase.Playing;
            NotifyChanged();
        }

        void ProcessTownTick()
        {
            ProcessResidentNeeds();

            int traitIncome = State.GetResidentTraitIncome();
            int buildingIncome = State.GetOperationalBuildingIncome();
            int income = traitIncome + buildingIncome;
            State.Gold += income;

            int repFromBuildings = 0;
            foreach (var building in State.Buildings)
            {
                if (!building.IsOperational || building.Definition == null)
                {
                    continue;
                }

                repFromBuildings += building.Definition.StaffReputation;
            }

            if (repFromBuildings > 0)
            {
                State.Reputation += repFromBuildings;
            }

            int moodReputation = CalculateMoodReputationDelta();
            if (moodReputation != 0)
            {
                State.Reputation = Mathf.Max(0, State.Reputation + moodReputation);
                State.AddLog($"Town reputation {FormatDelta(moodReputation)} from resident morale.");
            }

            if (income > 0)
            {
                if (buildingIncome > 0 && traitIncome > 0)
                {
                    State.AddLog($"Income: +{income} gold (buildings +{buildingIncome}, residents +{traitIncome}).");
                }
                else if (buildingIncome > 0)
                {
                    State.AddLog($"Income: +{buildingIncome} gold from staffed buildings.");
                }
                else
                {
                    State.AddLog($"Income: +{traitIncome} gold.");
                }
            }

            NotifyChanged();
            CheckDefeatConditions();
        }

        int CalculateMoodReputationDelta()
        {
            int happy = 0;
            int unhappy = 0;
            foreach (var resident in State.Residents)
            {
                if (resident?.Stats == null)
                {
                    continue;
                }

                if (resident.Stats.Mood >= 65 && resident.Stats.Stress <= 45)
                {
                    happy++;
                }
                else if (resident.Stats.Mood <= 35 || resident.Stats.Stress >= 75)
                {
                    unhappy++;
                }
            }

            return Mathf.Clamp(happy - unhappy, -2, 2);
        }

        void ProcessResidentNeeds()
        {
            for (int i = State.Residents.Count - 1; i >= 0; i--)
            {
                var resident = State.Residents[i];
                ResidentStatsHelper.ProcessTownTick(resident, State);
                if (!resident.Stats.ShouldLeaveTown)
                {
                    continue;
                }

                _workforceManager.TryUnassignWorker(State, resident);
                _residentManager.ExpelResident(State, resident, ResidentStatsHelper.GetLeaveReason(resident));
            }
        }

        void CheckDefeatConditions()
        {
            if (Phase == SessionPhase.Defeat)
            {
                return;
            }

            if (State.Reputation <= 0)
            {
                EndSession(false, "The town lost all trust. Everyone left.");
                return;
            }
        }

        IEnumerator RunRaidBattle()
        {
            Phase = SessionPhase.ResolvingRaid;
            NotifyChanged();

            bool? victory = null;
            string message = null;
            yield return _raidBattle.RunRaid(
                State,
                _bootstrap,
                _config,
                _visitorPrefab,
                _residentManager,
                (raidVictory, raidMessage) =>
                {
                    victory = raidVictory;
                    message = raidMessage;
                });

            if (victory == null)
            {
                Phase = SessionPhase.Playing;
                yield break;
            }

            ApplyRaidOutcome(victory.Value, message);

            CheckDefeatConditions();
        }

        void ApplyRaidOutcome(bool victory, string message)
        {
            State.AddLog(message);

            if (victory)
            {
                State.Reputation += _config.RaidSuccessReputationGain;
                State.AddLog($"+{_config.RaidSuccessReputationGain} reputation for holding the town.");
            }
            else
            {
                State.Gold -= _config.RaidFailureGoldLoss;
                State.Reputation -= _config.RaidFailureReputationLoss;
                State.AddLog(
                    $"Lost {_config.RaidFailureGoldLoss} gold and {_config.RaidFailureReputationLoss} reputation.");
            }

            Phase = SessionPhase.Playing;
            NotifyChanged();

            if (State.Gold < 0 && State.Reputation <= 2)
            {
                EndSession(false, "After the raid, the town collapsed.");
                return;
            }

            if (State.Reputation <= 0)
            {
                EndSession(false, "The town lost all trust after the raid.");
            }
        }

        void EndSession(bool victory, string message)
        {
            Phase = victory ? SessionPhase.Victory : SessionPhase.Defeat;
            State.AddLog(message);
            NotifyChanged();
            SessionEnded?.Invoke();
        }

        public void AssignDependencies(
            TownBootstrap bootstrap,
            GameSessionConfig config,
            VisitorCatalog catalog,
            BuildingCatalog buildingCatalog,
            GameObject visitorPrefab)
        {
            if (_bootstrap == null)
            {
                _bootstrap = bootstrap;
            }

            if (_config == null && config != null)
            {
                _config = config;
            }

            if (_catalog == null && catalog != null)
            {
                _catalog = catalog;
            }

            if (_buildingCatalog == null && buildingCatalog != null)
            {
                _buildingCatalog = buildingCatalog;
            }

            if (_visitorPrefab == null && visitorPrefab != null)
            {
                _visitorPrefab = visitorPrefab;
            }
        }

        public void NotifyChanged() => StateChanged?.Invoke();

        public static string FormatTime(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            int totalSeconds = Mathf.FloorToInt(seconds);
            int minutes = totalSeconds / 60;
            int remainder = totalSeconds % 60;
            return $"{minutes:00}:{remainder:00}";
        }

        static Sprite _fallbackSprite;

        static Sprite CreateFallbackSprite()
        {
            if (_fallbackSprite != null)
            {
                return _fallbackSprite;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, new Color(0.85f, 0.55f, 0.2f, 1f));
            texture.Apply();
            _fallbackSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0f), 1f);
            return _fallbackSprite;
        }

        void OnValidate()
        {
            if (_bootstrap == null)
            {
                _bootstrap = GetComponent<TownBootstrap>();
            }
        }
    }
}
