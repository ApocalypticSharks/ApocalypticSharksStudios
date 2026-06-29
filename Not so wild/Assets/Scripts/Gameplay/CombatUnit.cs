using System;
using UnityEngine;

namespace NotSoWild.Gameplay
{
    public sealed class CombatUnit : MonoBehaviour
    {
        public const float DefaultAttackRange = 3.6f;
        public const float DefaultAttackCooldown = 1.05f;
        const float BulletSpeed = 7.5f;
        const float MeleeRange = 0.75f;

        float _moveSpeed = 2.5f;
        float _attackRange = DefaultAttackRange;
        float _attackCooldown = DefaultAttackCooldown;
        float _attackTimer;
        float _cooldownMultiplier = 1f;
        float _cooldownBuffTimer;
        CombatUnit _target;
        int _targetCount = 1;
        int _multiTargetAccuracyPenalty;
        int _meleeDamageBonus;
        bool _accuracyScalesWithDistance;
        bool _dogSummoned;
        ResidentAnimationController _animationController;

        public int MaxHp { get; private set; }
        public int Hp { get; private set; }
        public int Attack { get; private set; }
        public int Accuracy { get; private set; }
        public bool IsEnemy { get; private set; }
        public ResidentRecord Resident { get; private set; }
        public UnitClassDefinition UnitClass { get; private set; }
        public bool IsAlive => Hp > 0;
        public float HealthPercent => MaxHp > 0 ? Hp / (float)MaxHp : 0f;

        public event Action<CombatUnit> Died;

        public void Initialize(
            bool isEnemy,
            int hp,
            int attack,
            int accuracy,
            float moveSpeed,
            string label,
            float attackRange = DefaultAttackRange,
            float attackCooldown = DefaultAttackCooldown,
            int targetCount = 1,
            int multiTargetAccuracyPenalty = 0,
            bool accuracyScalesWithDistance = false,
            int meleeDamageBonus = 0,
            ResidentRecord resident = null,
            UnitClassDefinition unitClass = null)
        {
            IsEnemy = isEnemy;
            MaxHp = Mathf.Max(1, hp);
            Hp = MaxHp;
            Attack = Mathf.Max(1, attack);
            Accuracy = Mathf.Clamp(accuracy, 5, 95);
            _moveSpeed = moveSpeed;
            _attackRange = Mathf.Max(0.3f, attackRange);
            _attackCooldown = Mathf.Max(0.1f, attackCooldown);
            _targetCount = Mathf.Max(1, targetCount);
            _multiTargetAccuracyPenalty = Mathf.Max(0, multiTargetAccuracyPenalty);
            _accuracyScalesWithDistance = accuracyScalesWithDistance;
            _meleeDamageBonus = Mathf.Max(0, meleeDamageBonus);
            Resident = resident;
            UnitClass = unitClass;
            name = label;
            EnsureAnimationController();
            _animationController.SetWalking(false);
            _animationController.SetShooting(false);

            var renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = isEnemy
                    ? new Color(0.95f, 0.3f, 0.28f, 1f)
                    : new Color(0.55f, 0.75f, 1f, 1f);
                renderer.sortingOrder = 25;
            }
        }

        void Update()
        {
            UpdateCooldownBuff();

            if (!IsAlive)
            {
                SetWalking(false);
                SetShooting(false);
                return;
            }

            if (_target == null || !_target.IsAlive)
            {
                AcquireTarget();
            }

            if (_target == null)
            {
                SetWalking(false);
                SetShooting(false);
                return;
            }

            var position = transform.position;
            var targetPosition = _target.transform.position;
            float distance = Vector2.Distance(position, targetPosition);
            if (UnitClass != null && UnitClass.HasAbility(UnitAbilityFlags.HealMostWoundedAlly) && _target.IsEnemy == IsEnemy)
            {
                UpdateHealingTarget(position, targetPosition, distance);
                return;
            }

            if (distance > _attackRange)
            {
                var direction = (targetPosition - position).normalized;
                transform.position = position + (Vector3)(direction * (_moveSpeed * Time.deltaTime));
                UpdateFacingToward(direction.x);
                SetWalking(true);
                SetShooting(false);
                return;
            }

            TrySummonDog();
            TryExecuteWeakAlly();
            TauntNearbyEnemies();

            SetWalking(false);
            SetShooting(true);
            UpdateFacingToward(targetPosition.x - position.x);
            _attackTimer -= Time.deltaTime;
            if (_attackTimer > 0f)
            {
                return;
            }

            ShootAt(_target, _targetCount > 1);
            ShootAtExtraTargets(_target);
            _attackTimer = GetCurrentAttackCooldown();
        }

        void AcquireTarget()
        {
            if (UnitClass != null && UnitClass.HasAbility(UnitAbilityFlags.HealMostWoundedAlly))
            {
                _target = FindMostWoundedAlly();
                if (_target != null)
                {
                    return;
                }
            }

            if (UnitClass != null && UnitClass.HasAbility(UnitAbilityFlags.TargetLowestHealthIncludingAllies))
            {
                _target = FindLowestHealthUnit();
                if (_target != null)
                {
                    return;
                }
            }

            CombatUnit closest = null;
            float closestDistance = float.MaxValue;

            foreach (var unit in FindObjectsByType<CombatUnit>())
            {
                if (unit == this || !unit.IsAlive || unit.IsEnemy == IsEnemy)
                {
                    continue;
                }

                float distance = Vector2.Distance(transform.position, unit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = unit;
                }
            }

            _target = closest;
        }

        CombatUnit FindMostWoundedAlly()
        {
            CombatUnit target = null;
            float lowestHealth = 1f;
            foreach (var unit in FindObjectsByType<CombatUnit>())
            {
                if (unit == this || !unit.IsAlive || unit.IsEnemy != IsEnemy || unit.HealthPercent >= 0.98f)
                {
                    continue;
                }

                if (unit.HealthPercent < lowestHealth)
                {
                    lowestHealth = unit.HealthPercent;
                    target = unit;
                }
            }

            return target;
        }

        CombatUnit FindLowestHealthUnit()
        {
            CombatUnit target = null;
            float lowestHealth = 1f;
            foreach (var unit in FindObjectsByType<CombatUnit>())
            {
                if (unit == this || !unit.IsAlive)
                {
                    continue;
                }

                if (unit.HealthPercent < lowestHealth)
                {
                    lowestHealth = unit.HealthPercent;
                    target = unit;
                }
            }

            return target;
        }

        void ShootAtExtraTargets(CombatUnit primary)
        {
            if (_targetCount <= 1)
            {
                return;
            }

            int shots = 1;
            foreach (var unit in FindObjectsByType<CombatUnit>())
            {
                if (shots >= _targetCount)
                {
                    return;
                }

                if (unit == this || unit == primary || !unit.IsAlive || unit.IsEnemy == IsEnemy)
                {
                    continue;
                }

                if (Vector2.Distance(transform.position, unit.transform.position) > _attackRange)
                {
                    continue;
                }

                ShootAt(unit, true);
                shots++;
            }
        }

        void ShootAt(CombatUnit target, bool multiTargetShot)
        {
            if (target == null)
            {
                return;
            }

            Vector3 origin = transform.position + Vector3.up * 0.16f;
            Vector3 targetPosition = target.transform.position + Vector3.up * 0.16f;
            float distance = Vector2.Distance(transform.position, target.transform.position);
            int shotAccuracy = Accuracy;
            if (multiTargetShot)
            {
                shotAccuracy -= _multiTargetAccuracyPenalty;
            }

            if (_accuracyScalesWithDistance)
            {
                shotAccuracy += Mathf.RoundToInt(distance * 6f);
            }

            shotAccuracy = Mathf.Clamp(shotAccuracy, 5, 95);
            bool hit = UnityEngine.Random.Range(0, 100) < shotAccuracy;
            if (!hit)
            {
                Vector2 missOffset = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(0.35f, 0.8f);
                targetPosition += new Vector3(missOffset.x, missOffset.y, 0f);
            }

            var bulletObject = new GameObject(IsEnemy ? "Bandit Bullet" : "Defender Bullet");
            bulletObject.transform.position = origin;
            var renderer = bulletObject.AddComponent<SpriteRenderer>();
            renderer.sprite = BulletProjectile.GetSprite();
            renderer.color = IsEnemy ? new Color(1f, 0.42f, 0.18f, 1f) : new Color(1f, 0.9f, 0.35f, 1f);
            renderer.sortingOrder = 32;
            bulletObject.AddComponent<BulletProjectile>().Initialize(
                this,
                target,
                targetPosition,
                Attack + (distance <= MeleeRange ? _meleeDamageBonus : 0),
                hit,
                BulletSpeed);

            if (hit)
            {
                ApplySpecialAttackEffects(target, Attack);
            }
        }

        void UpdateHealingTarget(Vector3 position, Vector3 targetPosition, float distance)
        {
            if (distance > Mathf.Max(0.2f, UnitClass.HealRange))
            {
                var direction = (targetPosition - position).normalized;
                transform.position = position + (Vector3)(direction * (_moveSpeed * Time.deltaTime));
                UpdateFacingToward(direction.x);
                SetWalking(true);
                SetShooting(false);
                return;
            }

            SetWalking(false);
            SetShooting(false);
            _attackTimer -= Time.deltaTime;
            if (_attackTimer > 0f)
            {
                return;
            }

            _target.Heal(UnitClass.HealAmount);
            _attackTimer = GetCurrentAttackCooldown();
            AcquireTarget();
        }

        void TauntNearbyEnemies()
        {
            if (UnitClass == null || !UnitClass.HasAbility(UnitAbilityFlags.TauntNearbyEnemies))
            {
                return;
            }

            foreach (var unit in FindObjectsByType<CombatUnit>())
            {
                if (unit == this || !unit.IsAlive || unit.IsEnemy == IsEnemy)
                {
                    continue;
                }

                if (Vector2.Distance(transform.position, unit.transform.position) <= UnitClass.TauntRadius)
                {
                    unit._target = this;
                }
            }
        }

        void TrySummonDog()
        {
            if (_dogSummoned || UnitClass == null || !UnitClass.HasAbility(UnitAbilityFlags.SummonDog))
            {
                return;
            }

            _dogSummoned = true;
            var dogObject = new GameObject($"{name}'s Dog");
            dogObject.transform.SetParent(transform.parent, false);
            dogObject.transform.position = transform.position + new Vector3(IsEnemy ? -0.18f : 0.18f, -0.15f, 0f);
            var renderer = dogObject.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateDogSprite();
            renderer.color = IsEnemy ? new Color(0.7f, 0.28f, 0.22f, 1f) : new Color(0.45f, 0.32f, 0.2f, 1f);
            renderer.sortingOrder = 24;
            var unit = dogObject.AddComponent<CombatUnit>();
            unit.Initialize(IsEnemy, 3, 1, 75, _moveSpeed * 1.15f, dogObject.name, 0.65f, 0.9f);
        }

        void TryExecuteWeakAlly()
        {
            if (UnitClass == null || !UnitClass.HasAbility(UnitAbilityFlags.ExecuteWeakAlly))
            {
                return;
            }

            foreach (var unit in FindObjectsByType<CombatUnit>())
            {
                if (unit == this ||
                    !unit.IsAlive ||
                    unit.IsEnemy != IsEnemy ||
                    unit.UnitClass == null ||
                    unit.UnitClass.Faction != UnitFaction.Bandits ||
                    unit.HealthPercent > UnitClass.ExecuteHealthThreshold)
                {
                    continue;
                }

                unit.TakeDamage(unit.Hp, this);
                BuffAlliedBandits(UnitClass.ExecuteBuffMultiplier, UnitClass.ExecuteBuffSeconds);
                _attackTimer = Mathf.Max(_attackTimer, GetCurrentAttackCooldown());
                return;
            }
        }

        void ApplySpecialAttackEffects(CombatUnit target, int baseDamage)
        {
            if (UnitClass == null || target == null)
            {
                return;
            }

            if (UnitClass.HasAbility(UnitAbilityFlags.AreaDamageIncludingAllies))
            {
                DamageArea(target.transform.position, baseDamage, UnitClass.AreaRadius, target);
            }

            if (UnitClass.HasAbility(UnitAbilityFlags.ConeDamageIncludingAllies))
            {
                DamageCone(target.transform.position - transform.position, baseDamage, target);
            }

            if (UnitClass.HasAbility(UnitAbilityFlags.RicochetIncludingAllies))
            {
                Ricochet(target, baseDamage, UnitClass.RicochetCount);
            }
        }

        void DamageArea(Vector3 center, int damage, float radius, CombatUnit primary)
        {
            int splashDamage = Mathf.Max(1, Mathf.RoundToInt(damage * 0.7f));
            foreach (var unit in FindObjectsByType<CombatUnit>())
            {
                if (unit == this || unit == primary || !unit.IsAlive)
                {
                    continue;
                }

                if (Vector2.Distance(center, unit.transform.position) <= radius)
                {
                    unit.TakeDamage(splashDamage, this);
                }
            }
        }

        void DamageCone(Vector3 direction, int damage, CombatUnit primary)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            int coneDamage = Mathf.Max(1, Mathf.RoundToInt(damage * 0.85f));
            direction.Normalize();
            foreach (var unit in FindObjectsByType<CombatUnit>())
            {
                if (unit == this || unit == primary || !unit.IsAlive)
                {
                    continue;
                }

                Vector3 delta = unit.transform.position - transform.position;
                if (delta.magnitude > UnitClass.ConeRange)
                {
                    continue;
                }

                if (Vector3.Angle(direction, delta) <= UnitClass.ConeAngle * 0.5f)
                {
                    unit.TakeDamage(coneDamage, this);
                    if (UnitClass.HasAbility(UnitAbilityFlags.HasteDamagedAllies) && unit.IsEnemy == IsEnemy)
                    {
                        unit.ApplyCooldownBuff(UnitClass.AllyHasteMultiplier, UnitClass.AllyHasteSeconds);
                    }
                }
            }
        }

        void Ricochet(CombatUnit firstTarget, int damage, int count)
        {
            var hitUnits = new System.Collections.Generic.List<CombatUnit> { firstTarget };
            var current = firstTarget;
            int ricochetDamage = damage;
            for (int i = 0; i < count; i++)
            {
                ricochetDamage = Mathf.Max(1, Mathf.RoundToInt(ricochetDamage * UnitClass.RicochetDamageMultiplier));
                var next = FindRicochetTarget(current, hitUnits);
                if (next == null)
                {
                    return;
                }

                next.TakeDamage(ricochetDamage, this);
                hitUnits.Add(next);
                current = next;
            }
        }

        CombatUnit FindRicochetTarget(CombatUnit from, System.Collections.Generic.List<CombatUnit> alreadyHit)
        {
            CombatUnit bestEnemy = null;
            CombatUnit bestAny = null;
            float bestEnemyDistance = float.MaxValue;
            float bestAnyDistance = float.MaxValue;
            foreach (var unit in FindObjectsByType<CombatUnit>())
            {
                if (unit == this || !unit.IsAlive || alreadyHit.Contains(unit))
                {
                    continue;
                }

                float distance = Vector2.Distance(from.transform.position, unit.transform.position);
                if (unit.IsEnemy != IsEnemy && distance < bestEnemyDistance)
                {
                    bestEnemyDistance = distance;
                    bestEnemy = unit;
                }

                if (distance < bestAnyDistance)
                {
                    bestAnyDistance = distance;
                    bestAny = unit;
                }
            }

            return bestEnemy != null ? bestEnemy : bestAny;
        }

        public void TakeDamage(int amount)
        {
            TakeDamage(amount, null);
        }

        public void TakeDamage(int amount, CombatUnit source)
        {
            if (!IsAlive)
            {
                return;
            }

            Hp = Mathf.Max(0, Hp - amount);
            if (Hp <= 0)
            {
                source?.OnKilledTarget(this);
                Died?.Invoke(this);
            }
        }

        public void Heal(int amount)
        {
            if (!IsAlive)
            {
                return;
            }

            Hp = Mathf.Min(MaxHp, Hp + Mathf.Max(1, amount));
        }

        public void ApplyCooldownBuff(float multiplier, float seconds)
        {
            _cooldownMultiplier = Mathf.Min(_cooldownMultiplier, Mathf.Clamp(multiplier, 0.1f, 1f));
            _cooldownBuffTimer = Mathf.Max(_cooldownBuffTimer, seconds);
        }

        void BuffAlliedBandits(float multiplier, float seconds)
        {
            foreach (var unit in FindObjectsByType<CombatUnit>())
            {
                if (unit != null &&
                    unit.IsAlive &&
                    unit.IsEnemy == IsEnemy &&
                    unit.UnitClass != null &&
                    unit.UnitClass.Faction == UnitFaction.Bandits)
                {
                    unit.ApplyCooldownBuff(multiplier, seconds);
                }
            }
        }

        void OnKilledTarget(CombatUnit target)
        {
            if (UnitClass == null ||
                !UnitClass.HasAbility(UnitAbilityFlags.MoodOnKill) ||
                Resident?.Stats == null ||
                target == null ||
                target.IsEnemy == IsEnemy)
            {
                return;
            }

            Resident.Stats.Apply(0, 4, -2);
            Resident.Stats.Clamp();
        }

        float GetCurrentAttackCooldown()
        {
            return Mathf.Max(0.1f, _attackCooldown * _cooldownMultiplier);
        }

        void UpdateCooldownBuff()
        {
            if (_cooldownBuffTimer <= 0f)
            {
                _cooldownMultiplier = 1f;
                return;
            }

            _cooldownBuffTimer -= Time.deltaTime;
            if (_cooldownBuffTimer <= 0f)
            {
                _cooldownMultiplier = 1f;
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

        void SetWalking(bool walking)
        {
            EnsureAnimationController();
            _animationController.SetWalking(walking);
        }

        void SetShooting(bool shooting)
        {
            EnsureAnimationController();
            _animationController.SetShooting(shooting);
        }

        void OnDisable()
        {
            if (_animationController != null)
            {
                _animationController.SetWalking(false);
                _animationController.SetShooting(false);
            }
        }

        void UpdateFacingToward(float xDelta)
        {
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer != null && Mathf.Abs(xDelta) > 0.001f)
            {
                renderer.flipX = xDelta < 0f;
            }
        }

        static Sprite _dogSprite;

        static Sprite CreateDogSprite()
        {
            if (_dogSprite != null)
            {
                return _dogSprite;
            }

            var texture = new Texture2D(3, 2, TextureFormat.RGBA32, false);
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            texture.SetPixel(0, 0, Color.white);
            texture.SetPixel(1, 0, Color.white);
            texture.SetPixel(2, 0, Color.white);
            texture.SetPixel(1, 1, Color.white);
            texture.Apply();
            _dogSprite = Sprite.Create(texture, new Rect(0f, 0f, 3f, 2f), new Vector2(0.5f, 0f), 16f);
            return _dogSprite;
        }

        sealed class BulletProjectile : MonoBehaviour
        {
            static Sprite _sprite;

            CombatUnit _source;
            CombatUnit _target;
            Vector3 _destination;
            int _damage;
            bool _hit;
            float _speed;

            public void Initialize(CombatUnit source, CombatUnit target, Vector3 destination, int damage, bool hit, float speed)
            {
                _source = source;
                _target = target;
                _destination = destination;
                _damage = Mathf.Max(1, damage);
                _hit = hit;
                _speed = Mathf.Max(0.1f, speed);

                Vector3 direction = _destination - transform.position;
                if (direction.sqrMagnitude > 0.0001f)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0f, 0f, angle);
                }
            }

            void Update()
            {
                transform.position = Vector3.MoveTowards(transform.position, _destination, _speed * Time.deltaTime);
                if (Vector3.Distance(transform.position, _destination) > 0.03f)
                {
                    return;
                }

                if (_hit && _target != null && _target.IsAlive)
                {
                    _target.TakeDamage(_damage, _source);
                }

                Destroy(gameObject);
            }

            public static Sprite GetSprite()
            {
                if (_sprite != null)
                {
                    return _sprite;
                }

                var texture = new Texture2D(3, 1, TextureFormat.RGBA32, false);
                texture.SetPixel(0, 0, Color.clear);
                texture.SetPixel(1, 0, Color.white);
                texture.SetPixel(2, 0, Color.white);
                texture.Apply();
                _sprite = Sprite.Create(texture, new Rect(0f, 0f, 3f, 1f), new Vector2(0f, 0.5f), 16f);
                return _sprite;
            }
        }
    }
}
