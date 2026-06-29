using System.Collections.Generic;
using NotSoWild.Core;
using UnityEngine;

namespace NotSoWild.Gameplay
{
    public sealed class ResidentManager : MonoBehaviour
    {
        [SerializeField] float _defaultPatrolSpeed = 1.4f;

        readonly List<ResidentAgent> _agents = new();
        TownBootstrap _bootstrap;
        GameObject _residentPrefab;
        float _patrolSpeed;

        public void Initialize(TownBootstrap bootstrap, GameObject residentPrefab, float patrolSpeed = 0f)
        {
            _bootstrap = bootstrap;
            _residentPrefab = residentPrefab;
            _patrolSpeed = patrolSpeed > 0f ? patrolSpeed : _defaultPatrolSpeed;
        }

        public ResidentAgent SpawnResident(VisitorDefinition definition, Vector3 position, ResidentRecord record)
        {
            if (_bootstrap?.Grid == null || definition == null)
            {
                return null;
            }

            var residentObject = CreateResidentObject(definition, position);
            var agent = residentObject.GetComponent<ResidentAgent>();
            if (agent == null)
            {
                agent = residentObject.AddComponent<ResidentAgent>();
            }

            agent.Initialize(_bootstrap.Grid, definition, position, _patrolSpeed, record);
            _agents.Add(agent);
            return agent;
        }

        public void SyncAgentWorkState(ResidentRecord record)
        {
            if (record?.Agent == null)
            {
                return;
            }

            record.Agent.BindRecord(record);
        }

        public ResidentAgent AdoptVisitor(VisitorAgent visitor, ResidentRecord record)
        {
            if (visitor == null || _bootstrap?.Grid == null)
            {
                return null;
            }

            var definition = visitor.Definition;
            var position = visitor.transform.position;
            var residentObject = visitor.gameObject;
            Destroy(visitor);

            var agent = residentObject.GetComponent<ResidentAgent>();
            if (agent == null)
            {
                agent = residentObject.AddComponent<ResidentAgent>();
            }

            agent.Initialize(_bootstrap.Grid, definition, position, _patrolSpeed, record);
            _agents.Add(agent);
            return agent;
        }

        public List<CombatUnit> PrepareDefendersForRaid(TownState state)
        {
            var units = new List<CombatUnit>();
            if (state == null)
            {
                return units;
            }

            foreach (var record in state.Residents)
            {
                if (record.Agent == null)
                {
                    continue;
                }

                var unitClass = record.Definition?.UnitClass;
                int combatPower = state.GetResidentCombatPower(record);
                int hp = unitClass != null
                    ? unitClass.MaxHp
                    : 4 + combatPower + state.GetResidentHealthBonus(record);
                int attack = unitClass != null
                    ? unitClass.Attack
                    : 1 + combatPower / 2;
                int accuracy = state.GetResidentAccuracy(record);
                units.Add(record.Agent.EnterCombat(
                    hp,
                    attack,
                    accuracy,
                    2.6f,
                    state.GetResidentAttackRange(record),
                    state.GetResidentAttackCooldown(record),
                    state.GetResidentTargetCount(record),
                    state.GetResidentMultiTargetAccuracyPenalty(record),
                    state.ResidentAccuracyScalesWithDistance(record),
                    state.GetResidentMeleeDamageBonus(record)));
            }

            return units;
        }

        public void FinishRaidAfterCombat(TownState state, bool raidVictory)
        {
            if (state == null)
            {
                return;
            }

            for (int i = state.Residents.Count - 1; i >= 0; i--)
            {
                var record = state.Residents[i];
                if (record.Agent == null)
                {
                    continue;
                }

                var combat = record.Agent.GetComponent<CombatUnit>();
                if (combat != null && combat.IsAlive)
                {
                    record.Agent.ExitCombat();
                    continue;
                }

                string name = record.Definition != null ? record.Definition.DisplayName : "A resident";
                ClearResidentAssignments(record);
                state.UnequipWeapon(record);
                _agents.Remove(record.Agent);
                Destroy(record.Agent.gameObject);
                record.Agent = null;
                state.Residents.RemoveAt(i);
                state.AddLog($"{name} fell in the raid.");
            }

            foreach (var record in state.Residents)
            {
                if (record?.Stats == null)
                {
                    continue;
                }

                ResidentStatsHelper.ApplyRaidAftermath(record, raidVictory);
            }
        }

        public void ExpelResident(TownState state, ResidentRecord record, string reason)
        {
            if (state == null || record == null)
            {
                return;
            }

            ClearResidentAssignments(record);
            state.UnequipWeapon(record);

            record.WorkState = ResidentWorkState.Idle;

            if (record.Agent != null)
            {
                _agents.Remove(record.Agent);
                Destroy(record.Agent.gameObject);
                record.Agent = null;
            }

            state.Residents.Remove(record);
            if (!string.IsNullOrEmpty(reason))
            {
                state.AddLog(reason);
            }
        }

        static void ClearResidentAssignments(ResidentRecord record)
        {
            if (record == null)
            {
                return;
            }

            if (record.ConstructionSite != null && record.ConstructionSite.Builder == record)
            {
                record.ConstructionSite.Builder = null;
            }

            if (record.AssignedBuilding != null && record.AssignedBuilding.Worker == record)
            {
                record.AssignedBuilding.Worker = null;
                record.AssignedBuilding.View?.SetStaffed(false);
            }

            record.ConstructionSite = null;
            record.AssignedBuilding = null;
        }

        GameObject CreateResidentObject(VisitorDefinition definition, Vector3 position)
        {
            position.z = -0.04f;

            GameObject residentObject;
            if (_residentPrefab != null)
            {
                residentObject = Instantiate(_residentPrefab, position, Quaternion.identity, transform);
                residentObject.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
                var renderer = residentObject.GetComponentInChildren<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.flipX = true;
                    renderer.sortingOrder = 18;
                }
            }
            else
            {
                residentObject = new GameObject(definition.DisplayName);
                residentObject.transform.SetParent(transform, false);
                residentObject.transform.position = position;
                var renderer = residentObject.AddComponent<SpriteRenderer>();
                renderer.sprite = CreateFallbackSprite();
                renderer.sortingOrder = 18;
                residentObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            }

            residentObject.name = definition.DisplayName;
            return residentObject;
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
    }
}
