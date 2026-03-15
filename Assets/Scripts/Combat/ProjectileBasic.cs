using UnityEngine;
using KitchenCaravan.Caravan;
using KitchenCaravan.Utils;

namespace KitchenCaravan.Combat
{
    // Basic projectile for the prototype. It travels in a fixed direction and damages the first caravan unit hit.
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ProjectileBasic : MonoBehaviour
    {
        [SerializeField] private int _damage = 5;
        [SerializeField] private float _speed = 8.5f;
        [SerializeField] private float _lifeTime = 3f;

        private Vector3 _direction = Vector3.up;
        private float _age;

        public void Initialize(Vector3 direction, float speed, int damage)
        {
            _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.up;
            _speed = speed;
            _damage = damage;
            _age = 0f;
            if (_direction.sqrMagnitude > 0.0001f)
            {
                transform.up = _direction;
            }
        }

        private void Awake()
        {
            EnsurePhysics();
            EnsureVisual();
        }

        private void Update()
        {
            _age += Time.deltaTime;
            transform.position += _direction * (_speed * Time.deltaTime);
            if (_age >= _lifeTime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out SegmentController segment))
            {
                segment.ApplyDamage(_damage);
                Destroy(gameObject);
                return;
            }

            if (other.TryGetComponent(out CaptainController captain))
            {
                captain.ApplyDamage(_damage);
                Destroy(gameObject);
            }
        }

        private void EnsureVisual()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = PrototypeSpriteLibrary.WhiteSquare;
            renderer.color = new Color(1f, 0.93f, 0.42f, 1f);
            renderer.sortingOrder = 25;
            transform.localScale = new Vector3(0.16f, 0.42f, 1f);
        }

        private void EnsurePhysics()
        {
            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<CircleCollider2D>();
            }

            collider.isTrigger = true;
            collider.radius = 0.2f;

            Rigidbody2D body = GetComponent<Rigidbody2D>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody2D>();
            }

            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.simulated = true;
        }
    }
}