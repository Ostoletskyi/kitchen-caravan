using UnityEngine;

namespace KitchenCaravan.Enemies.Worm
{
    public sealed class WormHead : MonoBehaviour, IWormDamageable
    {
        [SerializeField] private float _clickDamage = 100f;
        [SerializeField] private Color _vulnerableColor = new Color(0.95f, 0.35f, 0.25f, 1f);
        [SerializeField] private Color _immuneColor = new Color(0.95f, 0.75f, 0.2f, 1f);

        private WormController _controller;
        private WormHpLabel _hpLabel;
        private WormVisual2D _visual;
        private float _currentHp;
        private float _maxHp;
        private bool _isDead;

        public void Initialize(WormController controller, float maxHp)
        {
            _controller = controller;
            _maxHp = Mathf.Max(1f, maxHp);
            _currentHp = _maxHp;
            _isDead = false;
            EnsureComponents();
            RefreshVisual();
            RefreshLabel();
        }

        public bool ApplyDamage(float amount)
        {
            if (_isDead)
            {
                return false;
            }

            if (_controller != null && _controller.HasLivingSegments)
            {
                WormFloatingText.Spawn(transform.position + Vector3.up * 0.9f, "IMMUNE", _immuneColor);
                return false;
            }

            _currentHp = Mathf.Max(0f, _currentHp - Mathf.Max(0f, amount));
            WormFloatingText.Spawn(transform.position + Vector3.up * 0.75f, Mathf.RoundToInt(amount).ToString(), Color.red);
            RefreshLabel();

            if (_currentHp <= 0f)
            {
                _isDead = true;
                if (_controller != null)
                {
                    _controller.NotifyHeadDestroyed(this);
                }
                else
                {
                    Destroy(gameObject);
                }
            }

            return true;
        }

        private void Awake()
        {
            EnsureComponents();
        }

        private void Update()
        {
            RefreshVisual();
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

            _visual.SetVisual(_immuneColor, new Vector2(1f, 1f));

            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<CircleCollider2D>();
            }

            collider.radius = 0.45f;

            _hpLabel = GetComponent<WormHpLabel>();
            if (_hpLabel == null)
            {
                _hpLabel = gameObject.AddComponent<WormHpLabel>();
            }

            _hpLabel.SetOffset(new Vector3(0f, 0.95f, 0f));
            _hpLabel.SetTextColor(Color.white);
        }

        private void RefreshVisual()
        {
            if (_visual == null)
            {
                return;
            }

            bool vulnerable = _controller == null || !_controller.HasLivingSegments;
            _visual.SetColor(vulnerable ? _vulnerableColor : _immuneColor);
        }

        private void RefreshLabel()
        {
            if (_hpLabel != null)
            {
                _hpLabel.SetValue(_currentHp);
            }
        }
    }
}
