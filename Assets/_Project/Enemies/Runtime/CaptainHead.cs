using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class CaptainHead : MonoBehaviour, ICaravanDamageable
    {
        [SerializeField] private Color _vulnerableColor = new Color(0.95f, 0.3f, 0.25f, 1f);
        [SerializeField] private Color _invulnerableColor = new Color(0.95f, 0.75f, 0.2f, 1f);
        [SerializeField] private Vector2 _size = new Vector2(0.95f, 0.95f);

        private CaravanController _controller;
        private int _hp;
        private bool _dead;

        public void Initialize(CaravanController controller, int hp)
        {
            _controller = controller;
            _hp = Mathf.Max(1, hp);
            _dead = false;
            EnsurePhysics();
            EnsureVisual();
        }

        private void Start()
        {
            EnsurePhysics();
            EnsureVisual();
        }

        private void Update()
        {
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

        private void EnsureVisual()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr.sprite == null)
            {
                sr.sprite = RuntimeSpriteFactory.WhiteSquare;
            }

            sr.color = _controller != null && _controller.HasLivingSegments ? _invulnerableColor : _vulnerableColor;
            transform.localScale = new Vector3(_size.x, _size.y, 1f);
        }

        private void EnsurePhysics()
        {
            var collider = GetComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            collider.isTrigger = false;
        }
    }
}
