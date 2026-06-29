using System.Collections.Generic;
using NotSoWild.Core;
using NotSoWild.Gameplay;
using UnityEngine;

namespace NotSoWild.UI
{
    public sealed class GameUI : MonoBehaviour
    {
        [SerializeField] GameSession _session;

        VisitorDefinition _pendingVisitor;
        Vector2 _buildScroll;
        bool _showBuildPanel = true;

        Rect _hudRect;
        Rect _buildRect;
        Rect _assignRect;

        public static bool BlocksWorldInput { get; private set; }

        void Awake()
        {
            if (_session == null)
            {
                _session = GetComponent<GameSession>();
            }
        }

        void Start()
        {
            BindSessionEvents();
        }

        void BindSessionEvents()
        {
            if (_session == null)
            {
                return;
            }

            _session.StateChanged -= Repaint;
            _session.DecisionRequested -= ShowDecision;
            _session.StateChanged -= OnSessionStateChanged;
            _session.StateChanged += Repaint;
            _session.DecisionRequested += ShowDecision;
            _session.StateChanged += OnSessionStateChanged;
        }

        void OnSessionStateChanged()
        {
            if (_session.Phase != SessionPhase.AwaitingDecision)
            {
                _pendingVisitor = null;
            }
        }

        void OnEnable()
        {
            BindSessionEvents();
        }

        void OnDisable()
        {
            if (_session == null)
            {
                return;
            }

            _session.StateChanged -= Repaint;
            _session.DecisionRequested -= ShowDecision;
            _session.StateChanged -= OnSessionStateChanged;
        }

        void ShowDecision(VisitorDefinition visitor)
        {
            _pendingVisitor = visitor;
        }

        void Repaint()
        {
        }

        void OnGUI()
        {
            BlocksWorldInput = false;
            if (_session?.State == null)
            {
                return;
            }

            DrawHud();
            DrawBuildPanel();
            DrawBuildingAssignPanel();
            DrawConstructionPanel();
            DrawBuilderPickPanel();
            DrawResidentHoverPanel();
            DrawVisitorHoverPanel();
            DrawDecisionPanel();
            DrawHelp();
            DrawEndScreen();

            BlocksWorldInput =
                IsMouseInside(_hudRect) ||
                (_showBuildPanel && IsMouseInside(_buildRect)) ||
                IsMouseInside(_assignRect) ||
                (_session.Phase == SessionPhase.AwaitingDecision && _pendingVisitor != null);
        }

        static bool IsMouseInside(Rect rect)
        {
            if (Event.current == null)
            {
                return false;
            }

            var mouse = Event.current.mousePosition;
            return rect.Contains(mouse);
        }

        void DrawHud()
        {
            var state = _session.State;
            string elapsed = GameSession.FormatTime(state.ElapsedTimeSeconds);
            string target = GameSession.FormatTime(state.TargetTimeSeconds);
            _hudRect = new Rect(10f, 10f, 460f, 104f);
            GUI.Box(_hudRect, "Town Status");
            GUI.Label(new Rect(20f, 32f, 440f, 22f),
                $"Time {elapsed}/{target}   Gold {state.Gold}   Rep {state.Reputation}   Combat {state.GetTotalCombatPower()}   Heat {state.Heat}");
            GUI.Label(new Rect(20f, 54f, 440f, 22f),
                $"Residents {state.GetResidentCapacityUsage()}/{state.GetResidentCapacity()} ({state.Residents.Count} total)   Buildings {state.Buildings.Count}   Sites {state.ConstructionSites.Count}");
            string workerStatus = _session.CanStartConstruction
                ? $"Idle workers: {WorkforceHelper.CountIdleWorkers(state)}"
                : "No free workers for construction";
            GUI.Label(new Rect(20f, 76f, 440f, 22f), workerStatus);
        }

        void DrawBuildPanel()
        {
            if (_session.Phase == SessionPhase.Defeat || _session.Phase == SessionPhase.Victory)
            {
                return;
            }

            _buildRect = new Rect(Screen.width - 250f, 10f, 240f, 280f);
            GUI.Box(_buildRect, "Build");
            var viewRect = new Rect(_buildRect.x + 8f, _buildRect.y + 24f, 224f, 248f);
            float contentHeight = 150f;
            var contentRect = new Rect(0f, 0f, 210f, contentHeight);
            _buildScroll = GUI.BeginScrollView(viewRect, _buildScroll, contentRect);
            float y = 0f;

            if (_session.BuildInteraction != null && _session.BuildInteraction.IsPlacing)
            {
                var def = _session.BuildInteraction.SelectedDefinition;
                GUI.Label(new Rect(0f, y, 210f, 40f),
                    $"Placing: {def.DisplayName}\nClick a road-adjacent tile.");
                y += 44f;
                if (GUI.Button(new Rect(0f, y, 210f, 28f), "Cancel placement"))
                {
                    _session.BuildInteraction.CancelPlacement();
                }

                y += 36f;
            }

            GUI.Label(new Rect(0f, y, 210f, 44f), "New buildings come from builder events.");
            y += 48f;
            GUI.Label(new Rect(0f, y, 210f, 44f), "Accept a builder, then pick a tile for their plan.");
            y += 48f;
            if (!_session.CanStartConstruction)
            {
                GUI.Label(new Rect(0f, y, 210f, 22f), "All workers are busy.");
            }

            GUI.EndScrollView();
        }

        static string DescribeStats(TownState state, ResidentRecord resident)
        {
            var stats = resident.Stats;
            if (stats == null)
            {
                return "H --  M --  S --  A --";
            }

            int accuracy = state != null ? state.GetResidentAccuracy(resident) : 0;
            return $"H {stats.Health}  M {stats.Mood}  S {stats.Stress}  A {accuracy}";
        }

        static string DescribeResidentRisk(ResidentRecord resident)
        {
            var stats = resident.Stats;
            if (stats == null)
            {
                return string.Empty;
            }

            if (stats.Health <= 15)
            {
                return "Risk: weak health";
            }

            if (stats.Mood <= 15)
            {
                return "Risk: may leave";
            }

            if (stats.Stress >= 85)
            {
                return "Warning: high stress";
            }

            if (stats.Mood <= 30)
            {
                return "Warning: low mood";
            }

            return string.Empty;
        }

        static string DescribeWork(ResidentRecord resident)
        {
            switch (resident.WorkState)
            {
                case ResidentWorkState.Building:
                    return resident.ConstructionSite?.Definition != null
                        ? $"Building {resident.ConstructionSite.Definition.DisplayName}"
                        : "Building";
                case ResidentWorkState.Working:
                    return resident.AssignedBuilding?.Definition != null
                        ? $"Works at {resident.AssignedBuilding.Definition.DisplayName}"
                        : "Working";
                default:
                    return WorkforceHelper.CanBuild(resident) ? "Idle" : "Idle (no build)";
            }
        }

        static string DescribeBuildingEffect(BuildingDefinition building)
        {
            var parts = new List<string>();
            if (building.PassiveDailyGold != 0)
            {
                parts.Add($"{FormatSigned(building.PassiveDailyGold)}g/tick");
            }

            if (building.ResidentCapacityBonus != 0)
            {
                parts.Add($"{FormatSigned(building.ResidentCapacityBonus)} residents");
            }

            if (building.StaffDailyGold != 0)
            {
                parts.Add($"{FormatSigned(building.StaffDailyGold)}g/tick staffed");
            }

            if (building.StaffDefense != 0)
            {
                parts.Add($"{FormatSigned(building.StaffDefense)} defense");
            }

            if (building.StaffDailyHealth != 0)
            {
                parts.Add($"{FormatSigned(building.StaffDailyHealth)} health/tick");
            }

            if (building.StaffDailyMood != 0)
            {
                parts.Add($"{FormatSigned(building.StaffDailyMood)} mood/tick");
            }

            if (building.StaffDailyStress != 0)
            {
                parts.Add($"{FormatSigned(building.StaffDailyStress)} stress/tick");
            }

            if (parts.Count == 0)
            {
                return building.RequiresWorker ? "requires worker" : "passive";
            }

            return string.Join(", ", parts);
        }

        static string FormatSigned(int value) => value >= 0 ? $"+{value}" : value.ToString();

        void DrawResidentHoverPanel()
        {
            if (_session.Phase == SessionPhase.Defeat || _session.Phase == SessionPhase.Victory)
            {
                return;
            }

            var resident = FindHoveredResident();
            if (resident == null)
            {
                return;
            }

            DrawResidentInfoPanel(resident.Record, Event.current.mousePosition + new Vector2(18f, 18f));
        }

        ResidentAgent FindHoveredResident()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                return null;
            }

            Vector3 worldMouse = camera.ScreenToWorldPoint(GameInputHelper.MousePosition);
            worldMouse.z = -0.04f;

            ResidentAgent best = null;
            float bestDistance = float.MaxValue;
            foreach (var agent in FindObjectsByType<ResidentAgent>())
            {
                if (agent == null || agent.Record == null)
                {
                    continue;
                }

                var renderer = agent.GetComponent<SpriteRenderer>();
                if (renderer == null)
                {
                    renderer = agent.GetComponentInChildren<SpriteRenderer>();
                }

                if (renderer == null || !renderer.bounds.Contains(worldMouse))
                {
                    continue;
                }

                float distance = Vector2.SqrMagnitude(
                    (Vector2)agent.transform.position - (Vector2)worldMouse);
                if (distance < bestDistance)
                {
                    best = agent;
                    bestDistance = distance;
                }
            }

            return best;
        }

        void DrawResidentInfoPanel(ResidentRecord resident, Vector2 position)
        {
            if (resident == null)
            {
                return;
            }

            string name = resident.Definition != null ? resident.Definition.DisplayName : "Resident";
            var panel = ClampPanelToScreen(new Rect(position.x, position.y, 300f, 190f));
            GUI.Box(panel, name);

            float y = panel.y + 28f;
            GUI.Label(new Rect(panel.x + 12f, y, panel.width - 24f, 20f), DescribeWork(resident));
            y += 22f;
            GUI.Label(new Rect(panel.x + 12f, y, panel.width - 24f, 20f), DescribeStats(_session.State, resident));
            y += 22f;

            string risk = DescribeResidentRisk(resident);
            if (!string.IsNullOrEmpty(risk))
            {
                GUI.Label(new Rect(panel.x + 12f, y, panel.width - 24f, 20f), risk);
                y += 22f;
            }

            GUI.Label(new Rect(panel.x + 12f, y, panel.width - 24f, 20f), $"Weapon: {WeaponLabel(resident.EquippedWeapon)}");
            y += 22f;

            string unitClass = DescribeUnitClass(resident.Definition);
            if (!string.IsNullOrEmpty(unitClass))
            {
                GUI.Label(new Rect(panel.x + 12f, y, panel.width - 24f, 38f), unitClass);
                y += 40f;
            }

            string traits = DescribeTraitNames(resident.Definition);
            if (!string.IsNullOrEmpty(traits))
            {
                GUI.Label(new Rect(panel.x + 12f, y, panel.width - 24f, 40f), traits);
            }
        }

        void DrawVisitorHoverPanel()
        {
            var visitor = _session.ActiveVisitor;
            if (visitor == null ||
                visitor.Definition == null ||
                (!visitor.IsPointerOver && !visitor.IsSelected))
            {
                return;
            }

            DrawVisitorInfoPanel(visitor.Definition, Event.current.mousePosition + new Vector2(18f, 18f));
        }

        void DrawVisitorInfoPanel(VisitorDefinition visitor, Vector2 position)
        {
            string title = string.IsNullOrEmpty(visitor.DisplayName) ? "Visitor" : visitor.DisplayName;
            var panel = ClampPanelToScreen(new Rect(position.x, position.y, 330f, 224f));
            GUI.Box(panel, title);

            float y = panel.y + 28f;
            string description = visitor.Kind == VisitorKind.Event || visitor.Kind == VisitorKind.BuilderEvent
                ? visitor.EventText
                : visitor.Description;
            GUI.Label(new Rect(panel.x + 12f, y, panel.width - 24f, 48f), description ?? string.Empty);
            y += 52f;

            if (visitor.Kind == VisitorKind.ResidentCandidate)
            {
                string capacity = _session.State.IsResidentCapReached
                    ? "Town is full."
                    : $"Accept: +{visitor.AcceptReputationBonus} rep";
                GUI.Label(new Rect(panel.x + 12f, y, panel.width - 24f, 20f), capacity);
                y += 22f;
                GUI.Label(new Rect(panel.x + 12f, y, panel.width - 24f, 20f),
                    $"Reject: -{visitor.RejectReputationPenalty} rep");
                y += 24f;
            }

            string unitClass = DescribeUnitClass(visitor);
            if (!string.IsNullOrEmpty(unitClass))
            {
                GUI.Label(new Rect(panel.x + 12f, y, panel.width - 24f, 38f), unitClass);
                y += 42f;
            }

            DrawTraitDetails(visitor, panel.x + 12f, ref y, panel.width - 24f, panel.yMax - y - 8f);
        }

        static void DrawTraitDetails(VisitorDefinition visitor, float x, ref float y, float width, float maxHeight)
        {
            float maxY = y + Mathf.Max(0f, maxHeight);
            if (visitor.Traits == null || visitor.Traits.Length == 0)
            {
                GUI.Label(new Rect(x, y, width, 20f), "No special traits.");
                return;
            }

            foreach (var trait in visitor.Traits)
            {
                if (trait == null)
                {
                    continue;
                }

                if (y + 20f > maxY)
                {
                    break;
                }

                GUI.Label(new Rect(x, y, width, 20f), $"{trait.DisplayName}: {trait.Description}");
                y += 22f;
            }
        }

        static Rect ClampPanelToScreen(Rect rect)
        {
            rect.x = Mathf.Clamp(rect.x, 8f, Mathf.Max(8f, Screen.width - rect.width - 8f));
            rect.y = Mathf.Clamp(rect.y, 8f, Mathf.Max(8f, Screen.height - rect.height - 8f));
            return rect;
        }

        static string DescribeTraitNames(VisitorDefinition definition)
        {
            if (definition?.Traits == null || definition.Traits.Length == 0)
            {
                return string.Empty;
            }

            var names = new List<string>();
            foreach (var trait in definition.Traits)
            {
                if (trait != null && !string.IsNullOrEmpty(trait.DisplayName))
                {
                    names.Add(trait.DisplayName);
                }
            }

            return names.Count > 0 ? "Traits: " + string.Join(", ", names) : string.Empty;
        }

        static string DescribeUnitClass(VisitorDefinition definition)
        {
            var unitClass = definition?.UnitClass;
            if (unitClass == null)
            {
                return string.Empty;
            }

            return $"{unitClass.Faction} T{unitClass.Tier}: {unitClass.DisplayName}\n" +
                   $"{unitClass.AttackKind}, {FormatAbilities(unitClass.Abilities)}";
        }

        static string FormatAbilities(UnitAbilityFlags abilities)
        {
            if (abilities == UnitAbilityFlags.None)
            {
                return "no ability";
            }

            return abilities.ToString().Replace(", ", ", ");
        }

        void DrawBuildingAssignPanel()
        {
            _assignRect = default;
            var selected = _session.BuildInteraction?.SelectedBuilding;
            if (selected == null)
            {
                return;
            }

            bool isArmory = selected.Definition?.WorkRole == WorkRole.Armory;
            _assignRect = new Rect(Screen.width * 0.5f - 170f, 10f, 340f, isArmory ? 380f : 180f);
            GUI.Box(_assignRect, selected.Definition != null ? selected.Definition.DisplayName : "Building");

            float y = _assignRect.y + 28f;
            if (selected.Definition != null && !selected.Definition.RequiresWorker)
            {
                GUI.Label(new Rect(_assignRect.x + 12f, y, 316f, 36f), "Passive building — active after construction.");
                y += 40f;
                GUI.Label(new Rect(_assignRect.x + 12f, y, 316f, 22f), DescribeBuildingEffect(selected.Definition));
            }
            else if (selected.IsOperational)
            {
                string workerName = selected.Worker?.Definition != null
                    ? selected.Worker.Definition.DisplayName
                    : "Worker";
                GUI.Label(new Rect(_assignRect.x + 12f, y, 316f, 22f), $"Staffed by {workerName} — active.");
                y += 28f;
                if (GUI.Button(new Rect(_assignRect.x + 12f, y, 150f, 28f), "Remove worker"))
                {
                    _session.BuildInteraction.UnassignFromSelectedBuilding();
                }
            }
            else
            {
                GUI.Label(new Rect(_assignRect.x + 12f, y, 316f, 36f), "No worker — building is inactive (dimmed).");
                y += 40f;
                GUI.Label(new Rect(_assignRect.x + 12f, y, 316f, 22f), "Assign idle resident:");
                y += 24f;

                foreach (var resident in _session.State.Residents)
                {
                    if (!WorkforceHelper.IsIdle(resident) ||
                        !WorkforceHelper.CanWorkAt(resident, selected.Definition))
                    {
                        continue;
                    }

                    string name = resident.Definition != null ? resident.Definition.DisplayName : "Resident";
                    int bonus = selected.Definition != null
                        ? WorkforceHelper.GetWorkBonus(resident, selected.Definition.WorkRole)
                        : 0;
                    string bonusText = bonus > 0 ? $" (+{bonus} bonus)" : string.Empty;
                    if (GUI.Button(new Rect(_assignRect.x + 12f, y, 316f, 24f), $"{name}{bonusText}"))
                    {
                        _session.BuildInteraction.AssignWorkerToSelectedBuilding(resident);
                    }

                    y += 26f;
                }
            }

            if (isArmory)
            {
                y += 8f;
                DrawArmoryControls(selected, ref y);
            }

            if (GUI.Button(new Rect(_assignRect.x + 250f, _assignRect.y + _assignRect.height - 34f, 80f, 24f), "Close"))
            {
                _session.BuildInteraction.ClearSelectedBuilding();
            }
        }

        void DrawArmoryControls(PlacedBuildingRecord selected, ref float y)
        {
            var state = _session.State;
            bool active = selected.IsOperational;
            GUI.Label(new Rect(_assignRect.x + 12f, y, 316f, 22f),
                $"Stock: Pistols {state.Pistols}, Rifles {state.Rifles}, Shotguns {state.Shotguns}");
            y += 24f;

            GUI.enabled = active && state.Gold >= TownState.GetWeaponCost(WeaponType.Pistol);
            if (GUI.Button(new Rect(_assignRect.x + 12f, y, 96f, 24f), $"Buy pistol {TownState.GetWeaponCost(WeaponType.Pistol)}g"))
            {
                BuyWeapon(WeaponType.Pistol);
            }

            GUI.enabled = active && state.Gold >= TownState.GetWeaponCost(WeaponType.Rifle);
            if (GUI.Button(new Rect(_assignRect.x + 116f, y, 96f, 24f), $"Buy rifle {TownState.GetWeaponCost(WeaponType.Rifle)}g"))
            {
                BuyWeapon(WeaponType.Rifle);
            }

            GUI.enabled = active && state.Gold >= TownState.GetWeaponCost(WeaponType.Shotgun);
            if (GUI.Button(new Rect(_assignRect.x + 220f, y, 108f, 24f), $"Buy shotgun {TownState.GetWeaponCost(WeaponType.Shotgun)}g"))
            {
                BuyWeapon(WeaponType.Shotgun);
            }

            GUI.enabled = true;
            y += 30f;

            if (!active)
            {
                GUI.Label(new Rect(_assignRect.x + 12f, y, 316f, 22f), "Assign a worker to use the armory.");
                y += 24f;
                return;
            }

            GUI.Label(new Rect(_assignRect.x + 12f, y, 316f, 22f), "Equip residents:");
            y += 24f;

            foreach (var resident in state.Residents)
            {
                string name = resident.Definition != null ? resident.Definition.DisplayName : "Resident";
                GUI.Label(new Rect(_assignRect.x + 12f, y, 112f, 22f), $"{name}: {WeaponLabel(resident.EquippedWeapon)}");

                DrawEquipButton(resident, WeaponType.Pistol, _assignRect.x + 126f, y);
                DrawEquipButton(resident, WeaponType.Rifle, _assignRect.x + 188f, y);
                DrawEquipButton(resident, WeaponType.Shotgun, _assignRect.x + 250f, y);
                y += 26f;

                if (y > _assignRect.y + _assignRect.height - 42f)
                {
                    break;
                }
            }

            GUI.enabled = true;
        }

        void DrawEquipButton(ResidentRecord resident, WeaponType weapon, float x, float y)
        {
            bool hasWeapon = _session.State.GetWeaponCount(weapon) > 0 || resident.EquippedWeapon == weapon;
            GUI.enabled = hasWeapon && resident.EquippedWeapon != weapon;
            if (GUI.Button(new Rect(x, y, 58f, 22f), WeaponShortLabel(weapon)))
            {
                if (_session.State.TryEquipWeapon(resident, weapon))
                {
                    _session.State.AddLog($"{resident.Definition?.DisplayName ?? "Resident"} equipped {WeaponLabel(weapon)}.");
                    _session.NotifyChanged();
                }
            }
        }

        void BuyWeapon(WeaponType weapon)
        {
            int cost = TownState.GetWeaponCost(weapon);
            if (_session.State.Gold < cost)
            {
                return;
            }

            _session.State.Gold -= cost;
            _session.State.AddWeapon(weapon, 1);
            _session.State.AddLog($"Bought {WeaponLabel(weapon)} for {cost} gold.");
            _session.NotifyChanged();
        }

        static string WeaponLabel(WeaponType weapon)
        {
            return weapon switch
            {
                WeaponType.Pistol => "Pistol",
                WeaponType.Rifle => "Rifle",
                WeaponType.Shotgun => "Shotgun",
                _ => "None"
            };
        }

        static string WeaponShortLabel(WeaponType weapon)
        {
            return weapon switch
            {
                WeaponType.Pistol => "Pistol",
                WeaponType.Rifle => "Rifle",
                WeaponType.Shotgun => "Shotgun",
                _ => "-"
            };
        }

        void DrawConstructionPanel()
        {
            var selected = _session.BuildInteraction?.SelectedConstruction;
            if (selected == null)
            {
                return;
            }

            _assignRect = new Rect(Screen.width * 0.5f - 170f, 10f, 340f, 210f);
            GUI.Box(_assignRect, selected.Definition != null ? selected.Definition.DisplayName : "Construction");

            float y = _assignRect.y + 28f;
            float required = Mathf.Max(1f, selected.RequiredSeconds);
            float progress = Mathf.Clamp01(selected.Progress / required);
            GUI.Label(new Rect(_assignRect.x + 12f, y, 316f, 22f), $"Progress: {Mathf.RoundToInt(progress * 100f)}%");
            y += 24f;

            string builderName = selected.Builder?.Definition != null
                ? selected.Builder.Definition.DisplayName
                : "No builder assigned";
            GUI.Label(new Rect(_assignRect.x + 12f, y, 316f, 22f), $"Builder: {builderName}");
            y += 28f;

            GUI.Label(new Rect(_assignRect.x + 12f, y, 316f, 22f), "Assign idle builder:");
            y += 24f;
            foreach (var resident in _session.State.Residents)
            {
                if (!WorkforceHelper.IsIdle(resident) || !WorkforceHelper.CanBuild(resident))
                {
                    continue;
                }

                string name = resident.Definition != null ? resident.Definition.DisplayName : "Resident";
                if (GUI.Button(new Rect(_assignRect.x + 12f, y, 150f, 24f), name))
                {
                    _session.BuildInteraction.AssignBuilderToSelectedConstruction(resident);
                }

                y += 26f;
            }

            if (GUI.Button(new Rect(_assignRect.x + 12f, _assignRect.y + 170f, 120f, 26f), "Cancel site"))
            {
                _session.BuildInteraction.CancelSelectedConstruction();
            }

            if (GUI.Button(new Rect(_assignRect.x + 250f, _assignRect.y + 170f, 80f, 26f), "Close"))
            {
                _session.BuildInteraction.ClearSelectedBuilding();
            }
        }

        void DrawBuilderPickPanel()
        {
            if (_session.BuildInteraction == null ||
                !_session.BuildInteraction.IsPlacing ||
                _session.BuildInteraction.PendingBuilder != null)
            {
                return;
            }

            if (WorkforceHelper.CountIdleWorkers(_session.State) <= 1)
            {
                return;
            }

            var panel = new Rect(Screen.width * 0.5f - 160f, Screen.height * 0.5f - 80f, 320f, 160f);
            _assignRect = RectUnion(_assignRect, panel);
            GUI.Box(panel, "Choose builder");
            float y = panel.y + 28f;
            GUI.Label(new Rect(panel.x + 12f, y, 296f, 22f), "Pick who will construct this building:");
            y += 28f;

            foreach (var resident in _session.State.Residents)
            {
                if (!WorkforceHelper.IsIdle(resident) || !WorkforceHelper.CanBuild(resident))
                {
                    continue;
                }

                string name = resident.Definition != null ? resident.Definition.DisplayName : "Resident";
                int buildBonus = WorkforceHelper.GetWorkBonus(resident, WorkRole.Construction);
                string bonus = buildBonus > 0 ? $" (+{buildBonus} build speed)" : string.Empty;
                if (GUI.Button(new Rect(panel.x + 12f, y, 296f, 24f), name + bonus))
                {
                    _session.BuildInteraction.SelectBuilderForPlacement(resident);
                }

                y += 26f;
            }
        }

        static Rect RectUnion(Rect a, Rect b)
        {
            if (a.width <= 0f)
            {
                return b;
            }

            float xMin = Mathf.Min(a.xMin, b.xMin);
            float yMin = Mathf.Min(a.yMin, b.yMin);
            float xMax = Mathf.Max(a.xMax, b.xMax);
            float yMax = Mathf.Max(a.yMax, b.yMax);
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        void DrawDecisionPanel()
        {
            if (_session == null ||
                _session.Phase != SessionPhase.AwaitingDecision ||
                _pendingVisitor == null)
            {
                return;
            }

            string title = string.IsNullOrEmpty(_pendingVisitor.DisplayName)
                ? "Visitor"
                : _pendingVisitor.DisplayName;
            float panelHeight = Mathf.Min(220f, Screen.height - 24f);
            var panel = new Rect(Screen.width * 0.5f - 220f, Screen.height - panelHeight - 20f, 440f, panelHeight);
            GUI.Box(panel, title);

            float timeRemaining = Mathf.Max(0f, _session.DecisionTimeRemaining);
            float timeLimit = _session.DecisionTimeLimit;
            GUI.Label(
                new Rect(panel.x + 12f, panel.y + 24f, panel.width - 24f, 20f),
                $"Time: {timeRemaining:F1}s / {timeLimit:F0}s");

            float y = panel.y + 44f;
            float buttonY = panel.yMax - 46f;
            if (_pendingVisitor.Kind == VisitorKind.Event)
            {
                GUI.Label(new Rect(panel.x + 12f, y, panel.width - 24f, buttonY - y - 8f), _pendingVisitor.EventText ?? string.Empty);
                string acceptLabel = string.IsNullOrEmpty(_pendingVisitor.AcceptLabel) ? "Accept" : _pendingVisitor.AcceptLabel;
                string rejectLabel = string.IsNullOrEmpty(_pendingVisitor.RejectLabel) ? "Refuse" : _pendingVisitor.RejectLabel;
                if (GUI.Button(new Rect(panel.x + 20f, buttonY, 180f, 36f), acceptLabel))
                {
                    _session.AcceptCurrentVisitor();
                    _pendingVisitor = null;
                }

                if (GUI.Button(new Rect(panel.x + 240f, buttonY, 180f, 36f), rejectLabel))
                {
                    _session.RejectCurrentVisitor();
                    _pendingVisitor = null;
                }

                return;
            }

            GUI.Label(new Rect(panel.x + 12f, y, panel.width - 24f, 40f), _pendingVisitor.Description);
            y += 42f;
            DrawTraits(panel.x + 12f, ref y, panel.width - 24f, buttonY - y - 8f);

            bool canAccept = !_session.State.IsResidentCapReached;
            GUI.enabled = canAccept;
            if (GUI.Button(new Rect(panel.x + 20f, buttonY, 180f, 36f), "Accept"))
            {
                _session.AcceptCurrentVisitor();
                _pendingVisitor = null;
            }

            GUI.enabled = true;
            if (GUI.Button(new Rect(panel.x + 240f, buttonY, 180f, 36f), "Refuse"))
            {
                _session.RejectCurrentVisitor();
                _pendingVisitor = null;
            }

            if (!canAccept)
            {
                GUI.Label(new Rect(panel.x + 20f, buttonY - 22f, 400f, 20f), "Town is full — only events can be accepted.");
            }
        }

        void DrawTraits(float x, ref float y, float width, float maxHeight)
        {
            float maxY = y + Mathf.Max(0f, maxHeight);
            if (_pendingVisitor.Traits == null || _pendingVisitor.Traits.Length == 0)
            {
                GUI.Label(new Rect(x, y, width, 20f), "No special traits.");
                y += 22f;
                return;
            }

            foreach (var trait in _pendingVisitor.Traits)
            {
                if (y + 20f > maxY)
                {
                    break;
                }

                if (trait == null)
                {
                    continue;
                }

                GUI.Label(new Rect(x, y, width, 20f), $"• {trait.DisplayName}: {trait.Description}");
                y += 22f;
            }
        }

        void DrawHelp()
        {
            if (_session.Phase == SessionPhase.ResolvingRaid)
            {
                GUI.Label(new Rect(10f, Screen.height - 36f, 760f, 24f), "Raid in progress — defenders are fighting bandits on the road.");
                return;
            }

            if (_session.Phase != SessionPhase.Playing && _session.Phase != SessionPhase.AwaitingDecision)
            {
                return;
            }

            GUI.Label(
                new Rect(10f, Screen.height - 36f, 900f, 24f),
                "Build road-adjacent structures with a free worker. Click a building to staff it. Mayor cannot build.");
        }

        void DrawEndScreen()
        {
            if (_session.Phase != SessionPhase.Victory && _session.Phase != SessionPhase.Defeat)
            {
                return;
            }

            var panel = new Rect(Screen.width * 0.5f - 180f, Screen.height * 0.5f - 70f, 360f, 140f);
            GUI.Box(panel, _session.Phase == SessionPhase.Victory ? "Victory" : "Defeat");
            GUI.Label(new Rect(panel.x + 16f, panel.y + 36f, panel.width - 32f, 50f), _session.State.Log[0]);
            if (GUI.Button(new Rect(panel.x + 110f, panel.y + 92f, 140f, 32f), "Restart"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
        }

        void OnValidate()
        {
            if (_session == null)
            {
                _session = GetComponent<GameSession>();
            }
        }
    }
}
