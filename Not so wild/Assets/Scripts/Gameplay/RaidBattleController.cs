using System;
using System.Collections;
using System.Collections.Generic;
using NotSoWild.Core;
using UnityEngine;

namespace NotSoWild.Gameplay
{
    public sealed class RaidBattleController : MonoBehaviour
    {
        readonly List<CombatUnit> _units = new();
        TownState _raidState;
        ResidentManager _raidResidentManager;

        public bool IsRunning { get; private set; }

        public IEnumerator RunRaid(
            TownState state,
            TownBootstrap bootstrap,
            GameSessionConfig config,
            GameObject fighterPrefab,
            ResidentManager residentManager,
            Action<bool, string> onComplete)
        {
            if (IsRunning || bootstrap?.Grid == null)
            {
                yield break;
            }

            IsRunning = true;
            _units.Clear();
            _raidState = state;
            _raidResidentManager = residentManager;

            int elapsedMinutes = Mathf.FloorToInt(state.ElapsedTimeSeconds / 60f);
            int raidPower = config.BaseRaidPower +
                            elapsedMinutes * config.RaidPowerPerMinute +
                            state.RaidCount * config.RaidPowerPerAttack +
                            state.GetRaidRiskBonus();
            int enemyCount = Mathf.Clamp(Mathf.CeilToInt(raidPower / 3f), 1, 6);
            int enemyHp = Mathf.Max(3, raidPower / enemyCount);
            int enemyAttack = Mathf.Max(1, 1 + elapsedMinutes / 3 + state.RaidCount / 3);

            SpawnEnemies(bootstrap, fighterPrefab, config, enemyCount, enemyHp, enemyAttack);
            SpawnDefenders(state, bootstrap, fighterPrefab, residentManager);

            state.AddLog($"Raid! {enemyCount} bandits attack the town.");

            while (GetAliveEnemies() > 0 && GetAliveDefenders() > 0)
            {
                yield return null;
            }

            bool victory = GetAliveEnemies() == 0;
            CleanupUnits(victory);

            string message = victory
                ? "Raid repelled! Defenders held the line."
                : "Raiders broke through! Town losses incoming.";

            IsRunning = false;
            _raidState = null;
            _raidResidentManager = null;
            onComplete?.Invoke(victory, message);
        }

        void SpawnEnemies(TownBootstrap bootstrap, GameObject prefab, GameSessionConfig config, int count, int hp, int attack)
        {
            var grid = bootstrap.Grid;
            var entry = bootstrap.RoadEntry.Position;
            float rowY = grid.CellToWorldCenter(new GridCoordinates(0, grid.RoadCenterRow)).y;

            for (int i = 0; i < count; i++)
            {
                var position = new Vector3(entry.x + i * 0.35f, rowY + (i % 2 == 0 ? 0.08f : -0.08f), -0.04f);
                bool spawnLeader = i == count - 1 &&
                                   config?.RaidBanditLeaderClass != null &&
                                   _raidState != null &&
                                   _raidState.RaidCount >= config.FirstLeaderRaid;
                var unitClass = spawnLeader ? config.RaidBanditLeaderClass : config?.RaidBanditClass;
                int unitHp = unitClass != null ? unitClass.MaxHp : hp;
                int unitAttack = unitClass != null ? unitClass.Attack : attack;
                int unitAccuracy = unitClass != null ? unitClass.Accuracy : 55;
                float unitSpeed = unitClass != null ? unitClass.MoveSpeed : 2.8f;
                string label = unitClass != null ? unitClass.DisplayName : $"Bandit {i + 1}";
                var unit = CreateUnit(prefab, position, true, unitHp, unitAttack, unitAccuracy, label, unitSpeed, unitClass);
                if (unit != null)
                {
                    unit.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                    var renderer = unit.GetComponentInChildren<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.flipX = false;
                    }
                }
            }
        }

        void SpawnDefenders(
            TownState state,
            TownBootstrap bootstrap,
            GameObject prefab,
            ResidentManager residentManager)
        {
            var defenders = residentManager != null
                ? residentManager.PrepareDefendersForRaid(state)
                : new List<CombatUnit>();

            foreach (var unit in defenders)
            {
                if (unit == null)
                {
                    continue;
                }

                unit.Died += OnUnitDied;
                _units.Add(unit);
                unit.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
                var renderer = unit.GetComponentInChildren<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.flipX = true;
                }
            }

            var grid = bootstrap.Grid;
            Vector3 rallyPoint = bootstrap.TownHall != null
                ? grid.CellToWorldCenter(bootstrap.TownHall.Center)
                : grid.CellToWorldCenter(new GridCoordinates(grid.Columns / 2, grid.RoadCenterRow + 2));

            int townDefense = state.GetTownDefensePower();
            if (townDefense <= 0)
            {
                return;
            }

            int hp = 4 + townDefense;
            int attack = 1 + townDefense / 2;
            CreateUnit(prefab, rallyPoint + new Vector3(-0.2f, -0.35f, -0.04f), false, hp, attack, 65, "Town Guard", 2.4f);
        }

        CombatUnit CreateUnit(
            GameObject prefab,
            Vector3 position,
            bool isEnemy,
            int hp,
            int attack,
            int accuracy,
            string label,
            float moveSpeed,
            UnitClassDefinition unitClass = null)
        {
            GameObject unitObject;
            if (prefab != null)
            {
                unitObject = Instantiate(prefab, position, Quaternion.identity, transform);
            }
            else
            {
                unitObject = new GameObject(label);
                unitObject.transform.SetParent(transform, false);
                unitObject.transform.position = position;
                var renderer = unitObject.AddComponent<SpriteRenderer>();
                renderer.sprite = CreateFallbackSprite(isEnemy);
            }

            var unit = unitObject.GetComponent<CombatUnit>();
            if (unit == null)
            {
                unit = unitObject.AddComponent<CombatUnit>();
            }

            unit.Initialize(
                isEnemy,
                hp,
                attack,
                accuracy,
                moveSpeed,
                label,
                unitClass != null ? unitClass.AttackRange : CombatUnit.DefaultAttackRange,
                unitClass != null ? unitClass.AttackCooldown : CombatUnit.DefaultAttackCooldown,
                unitClass != null ? unitClass.TargetCount : 1,
                unitClass != null ? unitClass.MultiTargetAccuracyPenalty : 0,
                false,
                0,
                null,
                unitClass);
            unit.Died += OnUnitDied;
            _units.Add(unit);
            return unit;
        }

        void OnUnitDied(CombatUnit unit)
        {
            unit.Died -= OnUnitDied;

            if (unit.IsEnemy)
            {
                Destroy(unit.gameObject, 0.35f);
            }
        }

        int GetAliveEnemies()
        {
            int count = 0;
            foreach (var unit in _units)
            {
                if (unit != null && unit.IsEnemy && unit.IsAlive)
                {
                    count++;
                }
            }

            return count;
        }

        int GetAliveDefenders()
        {
            int count = 0;
            foreach (var unit in _units)
            {
                if (unit != null && !unit.IsEnemy && unit.IsAlive)
                {
                    count++;
                }
            }

            return count;
        }

        void CleanupUnits(bool raidVictory)
        {
            foreach (var unit in _units)
            {
                if (unit == null)
                {
                    continue;
                }

                unit.Died -= OnUnitDied;

                if (unit.IsEnemy)
                {
                    Destroy(unit.gameObject);
                    continue;
                }

                if (unit.GetComponent<ResidentAgent>() == null)
                {
                    Destroy(unit.gameObject);
                }
            }

            _units.Clear();
            _raidResidentManager?.FinishRaidAfterCombat(_raidState, raidVictory);
            foreach (var unit in GetComponentsInChildren<CombatUnit>())
            {
                if (unit != null && unit.GetComponent<ResidentAgent>() == null)
                {
                    Destroy(unit.gameObject);
                }
            }
        }

        static Sprite _allyFallback;
        static Sprite _enemyFallback;

        static Sprite CreateFallbackSprite(bool isEnemy)
        {
            ref var cached = ref isEnemy ? ref _enemyFallback : ref _allyFallback;
            if (cached != null)
            {
                return cached;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, isEnemy ? new Color(0.85f, 0.2f, 0.2f, 1f) : new Color(0.35f, 0.55f, 0.95f, 1f));
            texture.Apply();
            cached = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0f), 1f);
            return cached;
        }
    }
}
