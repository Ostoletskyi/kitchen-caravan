using System;
using UnityEngine;
using KitchenCaravan.Combat;
using KitchenCaravan.UI;
using KitchenCaravan.Utils;

namespace KitchenCaravan.Caravan
{
    // Controls the lead caravan unit. It only becomes damageable after all segments are destroyed.
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class CaptainController : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Transform _damageAnchor;
        [SerializeField] private Color _lockedColor = new Color(0.95f, 0.74f, 0.24f, 1f);
        [SerializeField] private Color _vulnerableColor = new Color(1f, 0.35f, 0.32f, 1f);
        [SerializeField] private Color _rageColor = new Color(1f, 0.15f, 0.15f, 1f);

        private int _currentHP;
        private bool _vulnerable;
        private bool _isRaging;
        private SpriteRenderer _renderer;

        public event Action Destroyed;

        public bool IsVulnerable => _vulnerable;
        public Transform DamageAnchor => _damageAnchor != null ? _damageAnchor : transform;

        public void Initialize(int hp)
        {
            _currentHP = Mathf.Max(1, hp);
            EnsureSetup();
            SetVulnerable(false);
            SetRaging(false);
        }

        public void SetVulnerable(bool vulnerable)
        {
            _vulnerable = vulnerable;
            EnsureSetup();
            GetComponent<CircleCollider2D>().enabled = vulnerable;
            RefreshColor();
        }

        public void SetRaging(bool isRaging)
        {
            _isRaging = isRaging;
            EnsureSetup();
            RefreshColor();
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
            if (!_vulnerable)
            {
                return false;
            }

            _currentHP = Mathf.Max(0, _currentHP - Mathf.Max(0, damage));
            FloatingDamageNumber.Spawn(DamageAnchor.position, damage, false);
            TemporaryHitFlash.Spawn(DamageAnchor.position, new Color(1f, 0.62f, 0.28f, 0.92f), 0.46f);
            if (_currentHP > 0)
            {
                return true;
            }

            TemporaryHitFlash.Spawn(transform.position, new Color(1f, 0.35f, 0.25f, 0.95f), 0.95f);
            Destroyed?.Invoke();
            return true;
        }

        private void Awake()
        {
            EnsureSetup();
        }

        private void EnsureSetup()
        {
            if (_visualRoot == null)
            {
                _visualRoot = transform;
            }

            if (_damageAnchor == null)
            {
                Transform existing = transform.Find("DamageAnchor");
                if (existing == null)
                {
                    GameObject anchor = new GameObject("DamageAnchor");
                    anchor.transform.SetParent(transform, false);
                    anchor.transform.localPosition = new Vector3(0f, 0.34f, 0f);
                    _damageAnchor = anchor.transform;
                }
                else
                {
                    _damageAnchor = existing;
                }
            }

            _renderer = GetComponent<SpriteRenderer>();
            _renderer.sprite = PrototypeSpriteLibrary.WhiteSquare;
            RefreshColor();
            transform.localScale = new Vector3(0.95f, 0.8f, 1f);

            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            collider.isTrigger = false;
        }

        private void RefreshColor()
        {
            if (_renderer == null)
            {
                return;
            }

            _renderer.color = _isRaging ? _rageColor : (_vulnerable ? _vulnerableColor : _lockedColor);
        }
    }
}