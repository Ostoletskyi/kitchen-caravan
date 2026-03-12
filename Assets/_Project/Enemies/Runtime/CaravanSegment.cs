using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class CaravanSegment : MonoBehaviour, ICaravanDamageable
    {
        [SerializeField] private Color _baseColor = new Color(0.45f, 0.95f, 0.4f, 1f);
        [SerializeField] private Vector2 _size = new Vector2(0.82f, 0.82f);

        private CaravanController _controller;
        private int _hp;
        private int _index;
        private bool _dead;

        public void Initialize(CaravanController controller, int hp, int index)
        {
            _controller = controller;
            _hp = Mathf.Max(1, hp);
            _index = Mathf.Max(0, index);
            _dead = false;
            EnsurePhysics();
            EnsureVisual();
        }

        private void Start()
        {
            EnsurePhysics();
            EnsureVisual();
        }

        public bool ApplyDamage(int amount)
        {
            if (_dead)
            {
                return false;
            }

            _hp -= Mathf.Max(0, amount);
            if (_hp > 0)
            {
                return true;
            }

            _dead = true;
            if (_controller != null)
            {
                _controller.NotifySegmentDestroyed(this);
            }
            else
            {
                Destroy(gameObject);
            }

            return true;
        }

        public void UpdateFollowTarget(Vector3 targetPosition, float followLerpSpeed)
        {
            float t = 1f - Mathf.Exp(-Mathf.Max(0f, followLerpSpeed) * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, targetPosition, t);
        }

        private void EnsureVisual()
        {
            var sr = GetComponent<SpriteRenderer>();
            sr.sprite = RuntimeSpriteFactory.WhiteSquare;

            float shade = Mathf.Clamp01(0.95f - (_index * 0.04f));
            sr.color = _baseColor * shade;
            transform.localScale = new Vector3(_size.x, _size.y, 1f);
        }

        private void EnsurePhysics()
        {
            var collider = GetComponent<CircleCollider2D>();
            collider.radius = 0.42f;
            collider.isTrigger = false;
        }
    }
}
