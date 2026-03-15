using System;
using UnityEngine;
using KitchenCaravan.UI;
using KitchenCaravan.Utils;

namespace KitchenCaravan.Caravan
{
    // One gameplay segment with shared HP, placeholder payload, HP view, and damage feedback anchors.
    [RequireComponent(typeof(SegmentHealth))]
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class SegmentController : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Transform _payloadPlaceholder;
        [SerializeField] private Transform _hpAnchor;
        [SerializeField] private Transform _damageAnchor;

        private SegmentData _data;
        private SegmentHealth _health;
        private SegmentHpView _hpView;

        public event Action<SegmentController> Destroyed;

        public int SegmentIndex => _data != null ? _data.SegmentIndex : 0;
        public bool IsChestCarrier => _data != null && _data.IsChestCarrier;
        public Transform DamageAnchor => _damageAnchor != null ? _damageAnchor : transform;
        public int CurrentHP => _health != null ? _health.CurrentHP : 0;

        public void Initialize(SegmentData data)
        {
            _data = data != null ? data.Clone() : new SegmentData();
            _health = GetComponent<SegmentHealth>();
            _health.Initialize(_data.MaxHP);
            EnsureVisuals();
            EnsureHpView();
            RefreshHpLabel();
        }

        public void SetSegmentIndex(int segmentIndex)
        {
            if (_data == null)
            {
                _data = new SegmentData();
            }

            _data.SegmentIndex = Mathf.Max(1, segmentIndex);
            RefreshVisualTint();
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

            bool destroyed = _health.ApplyDamage(damage);
            RefreshHpLabel();
            SpawnDamageNumber(damage);
            if (destroyed)
            {
                Destroyed?.Invoke(this);
            }

            return destroyed;
        }

        private void EnsureVisuals()
        {
            if (_visualRoot == null)
            {
                _visualRoot = GetOrCreateChild("VisualRoot");
            }

            if (_payloadPlaceholder == null)
            {
                _payloadPlaceholder = GetOrCreateChild("PayloadPlaceholder");
                _payloadPlaceholder.localPosition = new Vector3(0f, 0.18f, -0.05f);
            }

            if (_hpAnchor == null)
            {
                _hpAnchor = GetOrCreateChild("HPAnchor");
                _hpAnchor.localPosition = new Vector3(0f, 0.72f, 0f);
            }

            if (_damageAnchor == null)
            {
                _damageAnchor = GetOrCreateChild("DamageAnchor");
                _damageAnchor.localPosition = new Vector3(0f, 0.2f, 0f);
            }

            SpriteRenderer main = _visualRoot.GetComponent<SpriteRenderer>() ?? _visualRoot.gameObject.AddComponent<SpriteRenderer>();
            main.sprite = KitchenCaravan.VerticalSlice.RuntimeSpriteFactory.WhiteSquare;
            main.color = IsChestCarrier ? new Color(1f, 0.82f, 0.3f, 1f) : new Color(0.45f, 0.88f, 0.45f, 1f);
            _visualRoot.localScale = new Vector3(0.7f, 0.54f, 1f);

            SpriteRenderer payload = _payloadPlaceholder.GetComponent<SpriteRenderer>() ?? _payloadPlaceholder.gameObject.AddComponent<SpriteRenderer>();
            payload.sprite = KitchenCaravan.VerticalSlice.RuntimeSpriteFactory.WhiteSquare;
            payload.color = IsChestCarrier ? new Color(0.55f, 0.24f, 0.05f, 1f) : new Color(0.95f, 0.95f, 0.95f, 0.9f);
            _payloadPlaceholder.localScale = IsChestCarrier ? new Vector3(0.22f, 0.22f, 1f) : new Vector3(0.28f, 0.18f, 1f);
            RefreshVisualTint();
        }

        private void EnsureHpView()
        {
            if (_hpAnchor == null)
            {
                return;
            }

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

        private void SpawnDamageNumber(int damage)
        {
            GameObject go = new GameObject("FloatingDamageNumber");
            go.transform.position = DamageAnchor.position;
            FloatingDamageNumber number = go.AddComponent<FloatingDamageNumber>();
            number.Show(damage);
        }

        private void RefreshVisualTint()
        {
            if (_visualRoot == null)
            {
                return;
            }

            SpriteRenderer renderer = _visualRoot.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                return;
            }

            float tint = Mathf.Clamp01(1f - (Mathf.Max(1, SegmentIndex) - 1) * 0.03f);
            Color baseColor = IsChestCarrier ? new Color(1f, 0.82f, 0.3f, 1f) : new Color(0.45f, 0.88f, 0.45f, 1f);
            renderer.color = baseColor * tint;
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

        private void Reset()
        {
            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            collider.radius = 0.38f;
            collider.isTrigger = false;
        }
    }
}
