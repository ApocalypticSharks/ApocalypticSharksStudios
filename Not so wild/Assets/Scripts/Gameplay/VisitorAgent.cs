using System;
using NotSoWild.Core;
using UnityEngine;

namespace NotSoWild.Gameplay
{
    public sealed class VisitorAgent : MonoBehaviour
    {
        VisitorDefinition _definition;
        float _stopX;
        float _leaveX;
        float _speed;
        bool _arrived;
        bool _leaving;
        bool _decisionPending;
        bool _selected;
        Transform _newPersonRoot;
        GameObject _waitingIndicator;
        GameObject _yesButton;
        GameObject _noButton;
        SpriteRenderer _bodyRenderer;
        SpriteRenderer _yesRenderer;
        SpriteRenderer _noRenderer;
        ResidentAnimationController _animationController;

        public VisitorDefinition Definition => _definition;
        public bool HasArrived => _arrived;
        public bool IsDecisionPending => _decisionPending;
        public bool IsSelected => _selected;
        public bool IsPointerOver => IsPointerOverSelf();

        public event Action<VisitorAgent> Arrived;
        public event Action<VisitorAgent, bool> ChoiceMade;

        public void Initialize(
            VisitorDefinition definition,
            Vector3 spawnPosition,
            float stopX,
            float leaveX,
            float speed)
        {
            _definition = definition;
            _stopX = stopX;
            _leaveX = leaveX;
            _speed = speed;
            _arrived = false;
            _leaving = false;
            _decisionPending = false;
            _selected = false;
            transform.position = spawnPosition;
            EnsureAnimationController();
            SetWalking(false);
            CacheDecisionObjects();
            SetDecisionObjects(false, false);
        }

        void Update()
        {
            if (_definition == null)
            {
                return;
            }

            if (_leaving)
            {
                ContinueLeaving();
                return;
            }

            if (_decisionPending)
            {
                SetWalking(false);
                HandleDecisionClick();
                return;
            }

            if (_arrived)
            {
                SetWalking(false);
                return;
            }

            var position = transform.position;
            if (position.x <= _stopX)
            {
                position.x = _stopX;
                transform.position = position;
                _arrived = true;
                SetWalking(false);
                Arrived?.Invoke(this);
                return;
            }

            position.x -= _speed * Time.deltaTime;
            transform.position = position;
            SetWalking(true);
        }

        public void BeginDecision()
        {
            _decisionPending = true;
            _selected = false;
            SetWalking(false);
            CacheDecisionObjects();
            SetDecisionObjects(true, false);
        }

        public void EndDecision()
        {
            _decisionPending = false;
            _selected = false;
            SetWalking(false);
            SetDecisionObjects(false, false);
        }

        public void LeaveTown()
        {
            EndDecision();
            _arrived = false;
            _leaving = true;
            SetWalking(true);
        }

        void ContinueLeaving()
        {
            var position = transform.position;
            if (position.x <= _leaveX)
            {
                Destroy(gameObject);
                return;
            }

            position.x -= _speed * Time.deltaTime;
            transform.position = position;
            SetWalking(true);
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

        void HandleDecisionClick()
        {
            if (!GameInputHelper.WasLeftMousePressedThisFrame)
            {
                return;
            }

            if (_selected && IsPointerOverRenderer(_yesRenderer))
            {
                ChoiceMade?.Invoke(this, true);
                return;
            }

            if (_selected && IsPointerOverRenderer(_noRenderer))
            {
                ChoiceMade?.Invoke(this, false);
                return;
            }

            if (IsPointerOverSelf())
            {
                _selected = true;
                SetDecisionObjects(false, true);
            }
            else if (_selected)
            {
                _selected = false;
                SetDecisionObjects(true, false);
            }
        }

        void CacheDecisionObjects()
        {
            _bodyRenderer ??= GetComponent<SpriteRenderer>();
            if (_newPersonRoot == null)
            {
                _newPersonRoot = transform.Find("NewPerson");
            }

            if (_newPersonRoot == null)
            {
                return;
            }

            _waitingIndicator ??= FindDecisionChild("Waiting");
            _yesButton ??= FindDecisionChild("Yes");
            _noButton ??= FindDecisionChild("No");
            _yesRenderer ??= _yesButton != null ? _yesButton.GetComponent<SpriteRenderer>() : null;
            _noRenderer ??= _noButton != null ? _noButton.GetComponent<SpriteRenderer>() : null;
            RaiseDecisionSortingOrder();
        }

        GameObject FindDecisionChild(string childName)
        {
            var child = _newPersonRoot.Find(childName);
            return child != null ? child.gameObject : null;
        }

        void SetDecisionObjects(bool waiting, bool choices)
        {
            SetActive(_waitingIndicator, waiting);
            SetActive(_yesButton, choices);
            SetActive(_noButton, choices);
        }

        void RaiseDecisionSortingOrder()
        {
            foreach (var renderer in GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer != _bodyRenderer)
                {
                    renderer.sortingOrder = 30;
                }
            }
        }

        bool IsPointerOverSelf()
        {
            if (_bodyRenderer != null && IsPointerOverRenderer(_bodyRenderer))
            {
                return true;
            }

            var renderer = GetComponentInChildren<SpriteRenderer>();
            return renderer != null && IsPointerOverRenderer(renderer);
        }

        static bool IsPointerOverRenderer(SpriteRenderer renderer)
        {
            if (renderer == null || Camera.main == null)
            {
                return false;
            }

            Vector3 world = Camera.main.ScreenToWorldPoint(GameInputHelper.MousePosition);
            world.z = renderer.bounds.center.z;
            return renderer.bounds.Contains(world);
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
