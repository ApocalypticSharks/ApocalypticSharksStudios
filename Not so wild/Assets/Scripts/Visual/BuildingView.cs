using NotSoWild.Core;
using UnityEngine;

namespace NotSoWild.Visual
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class BuildingView : MonoBehaviour
    {
        const float RoadVisualSetbackCells = 0.26f;
        const float SmallBuildingExtraSetbackCells = 0.18f;

        BuildingDefinition _definition;
        GridCoordinates _origin;
        GridCoordinates _center;
        SpriteRenderer _renderer;
        SpriteRenderer _progressBack;
        SpriteRenderer _progressFill;
        float _progressWidth;
        float _progressHeight;

        public BuildingDefinition Definition => _definition;
        public GridCoordinates Origin => _origin;
        public GridCoordinates Center => _center;

        public void Setup(
            BuildingDefinition definition,
            TownGrid grid,
            GridCoordinates origin,
            GridCoordinates center,
            Sprite spriteOverride = null)
        {
            _definition = definition;
            _origin = origin;
            _center = center;
            _renderer = GetComponent<SpriteRenderer>();

            var sprite = spriteOverride ?? definition.Sprite;
            _renderer.sprite = sprite;
            _renderer.sortingOrder = definition.SortingOrder;

            var footprintSize = grid.GetFootprintWorldSize(definition.Width, definition.Height);
            var spriteSize = sprite != null ? (Vector2)sprite.bounds.size : footprintSize;
            if (spriteSize.x > 0f && spriteSize.y > 0f)
            {
                transform.localScale = new Vector3(
                    footprintSize.x / spriteSize.x,
                    footprintSize.y / spriteSize.y,
                    1f);
            }

            if (sprite != null)
            {
                var position = grid.AlignBuildingToFootprintCenter(
                    center,
                    sprite,
                    definition.Width,
                    definition.Height);
                position += GetRoadVisualSetback(grid, origin, definition.Width, definition.Height);
                transform.position = position;
            }
            else
            {
                transform.position = grid.GetFootprintCenterWorldPosition(center) +
                                     GetRoadVisualSetback(grid, origin, definition.Width, definition.Height);
            }

            UpdateProgressBarTransform();
        }

        static Vector3 GetRoadVisualSetback(TownGrid grid, GridCoordinates origin, int width, int height)
        {
            int top = origin.Y + height - 1;
            int bottom = origin.Y;
            float setbackCells = RoadVisualSetbackCells;
            if (width <= 2 && height <= 2)
            {
                setbackCells += SmallBuildingExtraSetbackCells;
            }

            float offset = grid.CellSize * setbackCells;

            if (bottom > grid.RoadRowMax)
            {
                return Vector3.up * offset;
            }

            if (top < grid.RoadRowMin)
            {
                return Vector3.down * offset;
            }

            return Vector3.zero;
        }

        public void SetScaffold(bool scaffold)
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<SpriteRenderer>();
            }

            if (scaffold)
            {
                _renderer.color = Color.white;
            }
        }

        public void SetStaffed(bool staffed)
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<SpriteRenderer>();
            }

            _renderer.color = staffed
                ? Color.white
                : new Color(0.62f, 0.62f, 0.68f, 1f);
        }

        public void SetPreview(bool valid)
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<SpriteRenderer>();
            }

            _renderer.color = valid
                ? new Color(0.55f, 1f, 0.55f, 0.55f)
                : new Color(1f, 0.45f, 0.45f, 0.55f);
        }

        public void ShowConstructionProgress(float normalized)
        {
            EnsureProgressBar();
            UpdateProgressBarTransform();

            normalized = Mathf.Clamp01(normalized);
            _progressBack.gameObject.SetActive(true);
            _progressFill.gameObject.SetActive(true);
            _progressFill.transform.localScale = new Vector3(_progressWidth * normalized, _progressHeight, 1f);
        }

        public void HideConstructionProgress()
        {
            if (_progressBack != null)
            {
                _progressBack.gameObject.SetActive(false);
            }

            if (_progressFill != null)
            {
                _progressFill.gameObject.SetActive(false);
            }
        }

        void EnsureProgressBar()
        {
            if (_progressBack != null && _progressFill != null)
            {
                return;
            }

            _progressBack = CreateProgressRenderer("Build Progress Back", new Color(0.05f, 0.04f, 0.03f, 0.9f), 40, centered: true);
            _progressFill = CreateProgressRenderer("Build Progress Fill", new Color(0.35f, 0.9f, 0.25f, 1f), 41, centered: false);
        }

        SpriteRenderer CreateProgressRenderer(string objectName, Color color, int sortingOrder, bool centered)
        {
            var child = new GameObject(objectName);
            child.transform.SetParent(transform, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = ProgressSprite(centered);
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        void UpdateProgressBarTransform()
        {
            if (_progressBack == null || _progressFill == null || _renderer == null || _renderer.sprite == null)
            {
                return;
            }

            var bounds = _renderer.sprite.bounds;
            _progressWidth = Mathf.Max(0.5f, bounds.size.x * 0.62f);
            _progressHeight = Mathf.Max(0.05f, bounds.size.y * 0.025f);
            float y = bounds.max.y + _progressHeight * 2.4f;
            float x = -_progressWidth * 0.5f;

            _progressBack.transform.localPosition = new Vector3(0f, y, -0.02f);
            _progressBack.transform.localScale = new Vector3(_progressWidth, _progressHeight, 1f);
            _progressFill.transform.localPosition = new Vector3(x, y, -0.03f);
        }

        static Sprite _progressCenterSprite;
        static Sprite _progressLeftSprite;

        static Sprite ProgressSprite(bool centered)
        {
            var cached = centered ? _progressCenterSprite : _progressLeftSprite;
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
                _progressCenterSprite = cached;
            }
            else
            {
                _progressLeftSprite = cached;
            }

            return cached;
        }
    }
}
