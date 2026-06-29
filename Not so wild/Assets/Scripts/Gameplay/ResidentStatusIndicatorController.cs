using UnityEngine;

namespace NotSoWild.Gameplay
{
    public sealed class ResidentStatusIndicatorController : MonoBehaviour
    {
        const float DefaultShowSeconds = 3f;

        Transform _root;
        GameObject _happy;
        GameObject _sad;
        GameObject _ok;
        GameObject _damaged;
        GameObject _stressed;
        float _hideTimer;

        void Awake()
        {
            CacheIndicators();
            HideAll();
        }

        void Update()
        {
            if (_hideTimer <= 0f)
            {
                return;
            }

            _hideTimer -= Time.deltaTime;
            if (_hideTimer <= 0f)
            {
                HideAll();
            }
        }

        public void Show(ResidentRecord record, float seconds = DefaultShowSeconds)
        {
            CacheIndicators();
            HideAll();

            if (record?.Stats == null)
            {
                return;
            }

            var stats = record.Stats;
            bool hasWarning = false;

            if (stats.Health <= 30)
            {
                SetActive(_damaged, true);
                hasWarning = true;
            }

            if (stats.Stress >= 70)
            {
                SetActive(_stressed, true);
                hasWarning = true;
            }

            if (stats.Mood <= 30)
            {
                SetActive(_sad, true);
                hasWarning = true;
            }

            if (!hasWarning && stats.Mood >= 75 && stats.Stress <= 35 && stats.Health >= 60)
            {
                SetActive(_happy, true);
                hasWarning = true;
            }

            if (!hasWarning)
            {
                SetActive(_ok, true);
            }

            _hideTimer = Mathf.Max(0.1f, seconds);
        }

        public void HideAll()
        {
            SetActive(_happy, false);
            SetActive(_sad, false);
            SetActive(_ok, false);
            SetActive(_damaged, false);
            SetActive(_stressed, false);
            _hideTimer = 0f;
        }

        void CacheIndicators()
        {
            if (_root == null)
            {
                _root = transform.Find("Statuses");
            }

            if (_root == null)
            {
                return;
            }

            _happy ??= FindChild("Happy");
            _sad ??= FindChild("Sad");
            _ok ??= FindChild("Ok");
            _damaged ??= FindChild("Damaged");
            _stressed ??= FindChild("Stressed");
        }

        GameObject FindChild(string childName)
        {
            var child = _root.Find(childName);
            return child != null ? child.gameObject : null;
        }

        static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }
    }
}
