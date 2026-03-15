using System;
using UnityEngine;

namespace KitchenCaravan.Caravan
{
    // Front unit of the caravan. It becomes vulnerable only when all segments are gone.
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class CaptainController : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Transform _damageAnchor;

        private int _currentHP;
        private bool _vulnerable;

        public event Action Destroyed;

        public bool IsVulnerable => _vulnerable;
        public Transform DamageAnchor => _damageAnchor != null ? _damageAnchor : transform;

        public void Initialize(int hp)
        {
            _currentHP = Mathf.Max(1, hp);
            EnsureVisuals();
            SetVulnerable(false);
        }

        public void SetVulnerable(bool vulnerable)
        {
            _vulnerable = vulnerable;
            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            collider.enabled = vulnerable;
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

            _currentHP -= Mathf.Max(0, damage);
            if (_currentHP > 0)
            {
                return false;
            }

            Destroyed?.Invoke();
            return true;
        }

        private void EnsureVisuals()
        {
            if (_visualRoot == null)
            {
                GameObject visual = new GameObject("VisualRoot");
                visual.transform.SetParent(transform, false);
                _visualRoot = visual.transform;
            }

            if (_damageAnchor == null)
            {
                GameObject damage = new GameObject("DamageAnchor");
                damage.transform.SetParent(transform, false);
                damage.transform.localPosition = new Vector3(0f, 0.28f, 0f);
                _damageAnchor = damage.transform;
            }

            SpriteRenderer renderer = _visualRoot.GetComponent<SpriteRenderer>() ?? _visualRoot.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = KitchenCaravan.VerticalSlice.RuntimeSpriteFactory.WhiteSquare;
            _visualRoot.localScale = new Vector3(0.95f, 0.7f, 1f);
            RefreshColor();
        }

        private void RefreshColor()
        {
            if (_visualRoot == null)
            {
                return;
            }

            SpriteRenderer renderer = _visualRoot.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = _vulnerable ? new Color(0.95f, 0.35f, 0.35f, 1f) : new Color(0.95f, 0.75f, 0.25f, 1f);
            }
        }

        private void Reset()
        {
            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            collider.radius = 0.42f;
            collider.isTrigger = false;
        }
    }
}
