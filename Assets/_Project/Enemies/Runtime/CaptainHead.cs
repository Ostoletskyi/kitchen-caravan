using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class CaptainHead : MonoBehaviour, ICaravanDamageable
    {
        [SerializeField] private Color _vulnerableColor = new Color(1f, 0.45f, 0.3f, 1f);
        [SerializeField] private Color _invulnerableColor = new Color(1f, 0.72f, 0.2f, 1f);
        [SerializeField] private Color _rageColor = new Color(1f, 0.1f, 0.1f, 1f);
        [SerializeField] private Vector2 _size = new Vector2(1.2f, 1.2f);

        private CaravanController _controller;
        private int _hp;
        private bool _dead;
        private bool _isRaging;
        private SegmentHealthLabel _healthLabel;

        public void Initialize(CaravanController controller, int hp)
        {
            _controller = controller;
            _hp = Mathf.Max(1, hp);
            _dead = false;
            EnsurePhysics();
            EnsureVisual();
            EnsureHealthLabel();
            RefreshHealthLabel();
        }

        private void Start()
        {
            EnsurePhysics();
            EnsureVisual();
            EnsureHealthLabel();
            RefreshHealthLabel();
        }

        private void Update()
        {
            EnsureVisual();
        }

        public void SetRageState(bool isRaging)
        {
            _isRaging = isRaging;
            EnsureVisual();
        }

        public bool ApplyDamage(DamageRequest request, out DamageResult result)
        {
            result = default;
            if (_dead)
            {
                return false;
            }

            if (_controller != null && _controller.HasLivingSegments)
            {
                return false;
            }

            result = DamageSystem.Evaluate(request);
            _hp -= result.finalDamage;
            DamageFeedbackService.ShowDamage(result);
            RefreshHealthLabel();
            if (_hp > 0)
            {
                return true;
            }

            _dead = true;
            DamageFeedbackService.ShowEffect(DamageFeedbackType.SegmentDestroyed, transform.position);
            if (_controller != null)
            {
                _controller.NotifyCaptainDestroyed(this);
            }
            else
            {
                Destroy(gameObject);
            }

            return true;
        }

        public void SetWorldPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        private void EnsureHealthLabel()
        {
            if (_healthLabel == null)
            {
                _healthLabel = GetComponent<SegmentHealthLabel>();
                if (_healthLabel == null)
                {
                    _healthLabel = gameObject.AddComponent<SegmentHealthLabel>();
                }
            }
        }

        private void RefreshHealthLabel()
        {
            _healthLabel?.SetValue(_hp);
        }

        private void EnsureVisual()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr.sprite == null)
            {
                sr.sprite = RuntimeSpriteFactory.WhiteSquare;
            }

            sr.color = _isRaging ? _rageColor : (_controller != null && _controller.HasLivingSegments ? _invulnerableColor : _vulnerableColor);
            float breathe = 1f + Mathf.Sin(Time.time * 2f) * 0.04f;
            float ragePulse = _isRaging ? 1f + Mathf.Sin(Time.time * 18f) * 0.12f : 1f;
            transform.localScale = new Vector3(_size.x * breathe * ragePulse, _size.y * breathe * ragePulse, 1f);
        }

        private void EnsurePhysics()
        {
            var collider = GetComponent<CircleCollider2D>();
            collider.radius = 0.58f;
            collider.isTrigger = false;
        }
    }
}