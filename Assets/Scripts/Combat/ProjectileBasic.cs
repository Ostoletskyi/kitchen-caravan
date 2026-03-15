using UnityEngine;
using KitchenCaravan.Caravan;
using KitchenCaravan.Core;
using KitchenCaravan.UI;

namespace KitchenCaravan.Combat
{
    // Simple upward projectile used by the Level 1 prototype.
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ProjectileBasic : MonoBehaviour
    {
        [SerializeField] private int _damage = 5;
        [SerializeField] private float _speed = 8f;
        [SerializeField] private float _lifeTime = 3f;

        private float _age;

        public void Initialize(float speed, int damage)
        {
            _speed = speed;
            _damage = damage;
            _age = 0f;
        }

        private void Awake()
        {
            EnsureCollider();
        }

        private void Start()
        {
            EnsureVisual();
        }

        private void Update()
        {
            _age += Time.deltaTime;
            transform.position += Vector3.up * (_speed * Time.deltaTime);
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
                SpawnHitFlash(segment.DamageAnchor.position);
                Destroy(gameObject);
                return;
            }

            if (other.TryGetComponent(out CaptainController captain))
            {
                if (captain.ApplyDamage(_damage))
                {
                    SpawnHitFlash(captain.DamageAnchor.position);
                }

                Destroy(gameObject);
            }
        }

        private void EnsureVisual()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = KitchenCaravan.VerticalSlice.RuntimeSpriteFactory.WhiteSquare;
            renderer.color = new Color(1f, 0.92f, 0.3f, 1f);
            transform.localScale = new Vector3(0.18f, 0.42f, 1f);
        }

        private void EnsureCollider()
        {
            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<CircleCollider2D>();
            }

            collider.isTrigger = true;

            Rigidbody2D body = GetComponent<Rigidbody2D>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody2D>();
            }

            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
        }

        private static void SpawnHitFlash(Vector3 position)
        {
            GameObject go = new GameObject("HitFlash");
            go.transform.position = position;
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = KitchenCaravan.VerticalSlice.RuntimeSpriteFactory.WhiteSquare;
            renderer.color = new Color(1f, 0.9f, 0.45f, 0.8f);
            go.transform.localScale = new Vector3(0.32f, 0.32f, 1f);
            go.AddComponent<TemporaryHitFlash>();
        }
    }
}
