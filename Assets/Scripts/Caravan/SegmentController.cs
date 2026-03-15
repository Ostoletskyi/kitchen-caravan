using System;
using UnityEngine;
using KitchenCaravan.Combat;
using KitchenCaravan.UI;
using KitchenCaravan.Utils;

namespace KitchenCaravan.Caravan
{
    // One gameplay segment with shared HP, placeholder payload, HP view, and feedback anchors.
    [RequireComponent(typeof(SegmentHealth))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SegmentController : MonoBehaviour
    {
        [SerializeField] private Transform _payloadPlaceholder;
        [SerializeField] private Transform _hpAnchor;
        [SerializeField] private Transform _damageAnchor;
        [SerializeField] private Color _segmentColor = new Color(0.45f, 0.88f, 0.45f, 1f);
        [SerializeField] private Color _chestColor = new Color(1f, 0.84f, 0.28f, 1f);

        private SegmentData _data;
        private SegmentHealth _health;
        private SegmentHpView _hpView;
        private SpriteRenderer _renderer;
        private SpriteRenderer _payloadRenderer;

        public event Action<SegmentController> Destroyed;

        public int SegmentIndex => _data != null ? _data.SegmentIndex : 0;
        public bool IsChestCarrier => _data != null && _data.IsChestCarrier;
        public Transform DamageAnchor => _damageAnchor != null ? _damageAnchor : transform;
        public int CurrentHP => _health != null ? _health.CurrentHP : 0;
        public float DistanceOnPath => _data != null ? _data.DistanceOnPath : 0f;

        public void Initialize(SegmentData data)
        {
            _data = data != null ? data.Clone() : new SegmentData();
            _health = GetComponent<SegmentHealth>();
            _health.Initialize(_data.MaxHP);
            EnsureSetup();
            RefreshVisuals();
            RefreshHpLabel();
        }

        public void SetSegmentIndex(int segmentIndex)
        {
            if (_data == null)
            {
                _data = new SegmentData();
            }

            _data.SegmentIndex = Mathf.Max(1, segmentIndex);
            RefreshVisuals();
        }

        public void SetDistanceOnPath(float distanceOnPath)
        {
            if (_data == null)
            {
                _data = new SegmentData();
            }

            _data.DistanceOnPath = distanceOnPath;
        }

        public void ApplyPose(Vector3 targetPosition, Vector3 targetDirection, float positionSmoothness, float rotationSmoothness, float deltaTime)
        {
            float moveT = 1f - Mathf.Exp(-Mathf.Max(0.01f, positionSmoothness) * deltaTime);
            float rotateT = 1f - Mathf.Exp(-Mathf.Max(0.01f, rotationSmoothness) * deltaTime);
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveT);
            if (targetDirection.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateT);
            }
        }

        public void SnapPose(Vector3 position, Vector3 forward)
        {
            transform.position = position;
            if (forward.sqrMagnitude > 0.0001f)
            {
                transform.up = forward;
            }
        }

        public bool ApplyDamage(int damage)
        {
            if (_health == null)
            {
                _health = GetComponent<SegmentHealth>();
            }

            int applied = _health.ApplyDamage(damage);
            FloatingDamageNumber.Spawn(DamageAnchor.position, applied, false);
            TemporaryHitFlash.Spawn(DamageAnchor.position, IsChestCarrier ? new Color(1f, 0.9f, 0.3f, 0.9f) : new Color(1f, 1f, 1f, 0.85f), 0.38f);
            RefreshHpLabel();
            if (!_health.IsDepleted())
            {
                return false;
            }

            TemporaryHitFlash.Spawn(transform.position, new Color(1f, 0.52f, 0.3f, 0.92f), IsChestCarrier ? 0.88f : 0.72f);
            Destroyed?.Invoke(this);
            return true;
        }

        private void Awake()
        {
            EnsureSetup();
        }

        private void EnsureSetup()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.sprite = PrototypeSpriteLibrary.WhiteSquare;

            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            collider.radius = 0.38f;
            collider.isTrigger = false;

            if (_payloadPlaceholder == null)
            {
                _payloadPlaceholder = GetOrCreateChild("PayloadPlaceholder");
                _payloadPlaceholder.localPosition = new Vector3(0f, 0.22f, -0.05f);
            }

            if (_hpAnchor == null)
            {
                _hpAnchor = GetOrCreateChild("HPAnchor");
                _hpAnchor.localPosition = new Vector3(0f, 0.74f, 0f);
            }

            if (_damageAnchor == null)
            {
                _damageAnchor = GetOrCreateChild("DamageAnchor");
                _damageAnchor.localPosition = new Vector3(0f, 0.2f, 0f);
            }

            _payloadRenderer = _payloadPlaceholder.GetComponent<SpriteRenderer>();
            if (_payloadRenderer == null)
            {
                _payloadRenderer = _payloadPlaceholder.gameObject.AddComponent<SpriteRenderer>();
            }

            _payloadRenderer.sprite = PrototypeSpriteLibrary.WhiteSquare;

            _hpView = _hpAnchor.GetComponent<SegmentHpView>();
            if (_hpView == null)
            {
                _hpView = _hpAnchor.gameObject.AddComponent<SegmentHpView>();
            }
        }

        private void RefreshHpLabel()
        {
            if (_hpView != null)
            {
                _hpView.SetValue(CurrentHP);
            }
        }

        private void RefreshVisuals()
        {
            if (_renderer == null)
            {
                return;
            }

            Color baseColor = IsChestCarrier ? _chestColor : _segmentColor;
            float tint = Mathf.Clamp01(1f - (Mathf.Max(1, SegmentIndex) - 1) * 0.025f);
            _renderer.color = baseColor * tint;
            transform.localScale = IsChestCarrier ? new Vector3(0.8f, 0.62f, 1f) : new Vector3(0.74f, 0.56f, 1f);

            if (_payloadRenderer != null)
            {
                _payloadRenderer.color = IsChestCarrier ? new Color(0.5f, 0.2f, 0.05f, 1f) : GetPayloadColor();
                _payloadRenderer.transform.localScale = IsChestCarrier ? new Vector3(0.24f, 0.24f, 1f) : new Vector3(0.28f, 0.18f, 1f);
            }
        }

        private Color GetPayloadColor()
        {
            switch (_data != null ? _data.PayloadType : SegmentPayloadType.Bread)
            {
                case SegmentPayloadType.Cheese:
                    return new Color(1f, 0.93f, 0.45f, 1f);
                case SegmentPayloadType.Tomato:
                    return new Color(1f, 0.32f, 0.3f, 1f);
                case SegmentPayloadType.Cucumber:
                    return new Color(0.45f, 0.85f, 0.45f, 1f);
                case SegmentPayloadType.Bacon:
                    return new Color(0.78f, 0.4f, 0.34f, 1f);
                case SegmentPayloadType.Egg:
                    return new Color(1f, 0.96f, 0.82f, 1f);
                default:
                    return new Color(0.95f, 0.85f, 0.58f, 1f);
            }
        }

        private Transform GetOrCreateChild(string childName)
        {
            Transform child = transform.Find(childName);
            if (child != null)
            {
                return child;
            }

            GameObject go = new GameObject(childName);
            go.transform.SetParent(transform, false);
            return go.transform;
        }
    }
}