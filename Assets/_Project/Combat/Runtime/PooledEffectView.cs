using System;
using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public sealed class PooledEffectView : MonoBehaviour
    {
        [SerializeField] private float _duration = 0.45f;

        private SpriteRenderer _renderer;
        private Action<PooledEffectView> _release;
        private float _elapsed;
        private Color _baseColor;
        private Vector3 _velocity;
        private Vector3 _baseScale;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer == null)
            {
                _renderer = gameObject.AddComponent<SpriteRenderer>();
            }

            _renderer.sprite = RuntimeSpriteFactory.WhiteSquare;
            gameObject.SetActive(false);
        }

        public void Show(DamageFeedbackType effectType, Vector3 position, Action<PooledEffectView> release)
        {
            if (_renderer == null)
            {
                Awake();
            }

            _release = release;
            _elapsed = 0f;
            transform.position = position;
            transform.localRotation = Quaternion.identity;

            switch (effectType)
            {
                case DamageFeedbackType.SegmentDestroyed:
                    _baseColor = new Color(0.6f, 0.6f, 0.6f, 0.9f);
                    _baseScale = new Vector3(0.65f, 0.65f, 1f);
                    _velocity = new Vector3(0f, 0.65f, 0f);
                    break;
                case DamageFeedbackType.CriticalHit:
                    _baseColor = new Color(1f, 0.35f, 0.25f, 1f);
                    _baseScale = new Vector3(0.35f, 0.35f, 1f);
                    _velocity = new Vector3(0f, 0.4f, 0f);
                    break;
                default:
                    _baseColor = new Color(1f, 0.9f, 0.3f, 0.95f);
                    _baseScale = new Vector3(0.24f, 0.24f, 1f);
                    _velocity = new Vector3(0f, 0.25f, 0f);
                    break;
            }

            _renderer.color = _baseColor;
            transform.localScale = _baseScale;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            transform.position += _velocity * Time.deltaTime;
            transform.localScale = Vector3.Lerp(_baseScale, _baseScale * 1.8f, t);

            Color color = _baseColor;
            color.a = Mathf.Lerp(_baseColor.a, 0f, t);
            _renderer.color = color;

            if (_elapsed >= _duration)
            {
                _release?.Invoke(this);
            }
        }
    }
}
