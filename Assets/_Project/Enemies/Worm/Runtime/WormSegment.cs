using UnityEngine;

namespace KitchenCaravan.Enemies.Worm
{
    public sealed class WormSegment : MonoBehaviour, IWormDamageable
    {
        [SerializeField] private float _clickDamage = 100f;
        [SerializeField] private Color _baseColor = new Color(0.45f, 0.95f, 0.4f, 1f);

        private WormController _controller;
        private WormHpLabel _hpLabel;
        private WormVisual2D _visual;
        private float _currentHp;
        private float _maxHp;
        private bool _isDead;

        public void Initialize(WormController controller, float maxHp, int index)
        {
            _controller = controller;
            _maxHp = Mathf.Max(1f, maxHp);
            _currentHp = _maxHp;
            _isDead = false;
            EnsureComponents();

            float shade = Mathf.Clamp01(0.92f - (index * 0.045f));
            _visual.SetVisual(_baseColor * shade, new Vector2(0.82f, 0.82f));
            _hpLabel.SetValue(_currentHp);
        }

        public bool ApplyDamage(float amount)
        {
            if (_isDead)
            {
                return false;
            }

            float damageAmount = Mathf.Max(0f, amount);
            _currentHp = Mathf.Max(0f, _currentHp - damageAmount);
            WormFloatingText.Spawn(transform.position + Vector3.up * 0.7f, Mathf.RoundToInt(damageAmount).ToString(), Color.red);
            _hpLabel.SetValue(_currentHp);

            if (_currentHp <= 0f)
            {
                _isDead = true;
                if (_controller != null)
                {
                    _controller.NotifySegmentDestroyed(this);
                }
                else
                {
                    Destroy(gameObject);
                }
            }

            return true;
        }

        public void UpdateFollowTarget(Vector3 targetPosition, float followLerpSpeed)
        {
            float t = 1f - Mathf.Exp(-Mathf.Max(0f, followLerpSpeed) * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, targetPosition, t);
        }

        private void Awake()
        {
            EnsureComponents();
        }

        private void OnMouseDown()
        {
            ApplyDamage(_clickDamage);
        }

        private void EnsureComponents()
        {
            _visual = GetComponent<WormVisual2D>();
            if (_visual == null)
            {
                _visual = gameObject.AddComponent<WormVisual2D>();
            }

            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<CircleCollider2D>();
            }

            collider.radius = 0.38f;

            _hpLabel = GetComponent<WormHpLabel>();
            if (_hpLabel == null)
            {
                _hpLabel = gameObject.AddComponent<WormHpLabel>();
            }

            _hpLabel.SetOffset(new Vector3(0f, 0.75f, 0f));
            _hpLabel.SetTextColor(Color.white);
        }
    }
}
