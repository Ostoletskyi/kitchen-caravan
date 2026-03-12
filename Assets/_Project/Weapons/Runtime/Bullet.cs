using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private int _damage = 1;
        [SerializeField] private float _speed = 12f;
        [SerializeField] private float _lifeSeconds = 3f;

        private void Awake()
        {
            EnsurePhysicsComponents();
        }

        private void Start()
        {
            EnsureVisualComponents();
        }

        private void OnValidate()
        {
            _lifeSeconds = Mathf.Max(0.01f, _lifeSeconds);
            _speed = Mathf.Max(0f, _speed);
        }

        private void EnsureVisualComponents()
        {
            var sr = GetComponent<SpriteRenderer>();
            sr.sprite = RuntimeSpriteFactory.WhiteSquare;
            sr.color = new Color(1f, 0.9f, 0.1f, 1f);
            transform.localScale = new Vector3(0.2f, 0.6f, 1f);
        }

        private void EnsurePhysicsComponents()
        {
            var collider = GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider2D>();
            }

            collider.isTrigger = true;

            var body = GetComponent<Rigidbody2D>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody2D>();
            }

            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
        }

        private void OnEnable()
        {
            Destroy(gameObject, _lifeSeconds);
        }

        private void Update()
        {
            transform.position += Vector3.up * (_speed * Time.deltaTime);
        }

        public void Initialize(float speed)
        {
            _speed = speed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out ICaravanDamageable caravanDamageable))
            {
                caravanDamageable.ApplyDamage(GetDamage());
                Destroy(gameObject);
                return;
            }

            if (!other.TryGetComponent(out Enemy enemy))
            {
                return;
            }

            enemy.ApplyDamage(GetDamage());
            Destroy(gameObject);
        }

        private int GetDamage()
        {
            return Mathf.Max(1, BalanceDebugSettings.BulletDamage > 0 ? BalanceDebugSettings.BulletDamage : _damage);
        }
    }
}
