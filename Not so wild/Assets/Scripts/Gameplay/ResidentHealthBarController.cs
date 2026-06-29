using UnityEngine;

namespace NotSoWild.Gameplay
{
    public sealed class ResidentHealthBarController : MonoBehaviour
    {
        const float Width = 0.46f;
        const float Height = 0.035f;
        const float VerticalOffset = 0.12f;

        static Sprite _centerSprite;
        static Sprite _leftSprite;

        ResidentAgent _agent;
        SpriteRenderer _bodyRenderer;
        SpriteRenderer _backRenderer;
        SpriteRenderer _fillRenderer;

        void Awake()
        {
            CacheComponents();
            Hide();
        }

        void LateUpdate()
        {
            CacheComponents();
            UpdateHealthBar();
        }

        void CacheComponents()
        {
            _agent ??= GetComponent<ResidentAgent>();
            _bodyRenderer ??= GetComponent<SpriteRenderer>();
        }

        void UpdateHealthBar()
        {
            var record = _agent != null ? _agent.Record : null;
            var stats = record != null ? record.Stats : null;
            if (stats == null || stats.Health >= ResidentStats.MaxValue || stats.Health <= ResidentStats.MinValue)
            {
                Hide();
                return;
            }

            EnsureBar();
            UpdateBarTransform();

            float normalized = Mathf.Clamp01(stats.Health / (float)ResidentStats.MaxValue);
            _backRenderer.gameObject.SetActive(true);
            _fillRenderer.gameObject.SetActive(true);
            _fillRenderer.transform.localScale = new Vector3(Width * normalized, Height, 1f);
            _fillRenderer.color = GetHealthColor(normalized);
        }

        void EnsureBar()
        {
            if (_backRenderer != null && _fillRenderer != null)
            {
                return;
            }

            _backRenderer = CreateBarRenderer("Health Back", new Color(0.05f, 0.04f, 0.03f, 0.9f), centered: true);
            _fillRenderer = CreateBarRenderer("Health Fill", new Color(0.35f, 0.9f, 0.25f, 1f), centered: false);
        }

        SpriteRenderer CreateBarRenderer(string objectName, Color color, bool centered)
        {
            var child = new GameObject(objectName);
            child.transform.SetParent(transform, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = BarSprite(centered);
            renderer.color = color;
            return renderer;
        }

        void UpdateBarTransform()
        {
            if (_bodyRenderer == null || _bodyRenderer.sprite == null)
            {
                return;
            }

            int sortingOrder = _bodyRenderer.sortingOrder + 4;
            _backRenderer.sortingLayerID = _bodyRenderer.sortingLayerID;
            _fillRenderer.sortingLayerID = _bodyRenderer.sortingLayerID;
            _backRenderer.sortingOrder = sortingOrder;
            _fillRenderer.sortingOrder = sortingOrder + 1;

            float y = _bodyRenderer.sprite.bounds.max.y + VerticalOffset;
            _backRenderer.transform.localPosition = new Vector3(0f, y, -0.02f);
            _backRenderer.transform.localScale = new Vector3(Width, Height, 1f);
            _fillRenderer.transform.localPosition = new Vector3(-Width * 0.5f, y, -0.03f);
        }

        void Hide()
        {
            if (_backRenderer != null)
            {
                _backRenderer.gameObject.SetActive(false);
            }

            if (_fillRenderer != null)
            {
                _fillRenderer.gameObject.SetActive(false);
            }
        }

        static Color GetHealthColor(float normalized)
        {
            if (normalized <= 0.3f)
            {
                return new Color(0.95f, 0.18f, 0.12f, 1f);
            }

            if (normalized <= 0.6f)
            {
                return new Color(0.95f, 0.75f, 0.18f, 1f);
            }

            return new Color(0.35f, 0.9f, 0.25f, 1f);
        }

        static Sprite BarSprite(bool centered)
        {
            var cached = centered ? _centerSprite : _leftSprite;
            if (cached != null)
            {
                return cached;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            cached = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                centered ? new Vector2(0.5f, 0.5f) : new Vector2(0f, 0.5f),
                1f);

            if (centered)
            {
                _centerSprite = cached;
            }
            else
            {
                _leftSprite = cached;
            }

            return cached;
        }
    }
}
