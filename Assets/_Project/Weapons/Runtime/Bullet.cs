using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private int _damage = 1;
        [SerializeField] private float _speed = 12f;
        [SerializeField] private float _lifeSeconds = 3f;
        [SerializeField] private WeaponDamageType _damageType = WeaponDamageType.RapidFire;

        private SpriteRenderer _trailRenderer;
        private float _trailPulse;

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

            if (_trailRenderer == null)
            {
                Transform trail = transform.Find("Trail");
                if (trail == null)
                {
                    var trailObject = new GameObject("Trail");
                    trailObject.transform.SetParent(transform, false);
                    trailObject.transform.localPosition = new Vector3(0f, -0.48f, 0.05f);
                    _trailRenderer = trailObject.AddComponent<SpriteRenderer>();
                }
                else
                {
                    _trailRenderer = trail.GetComponent<SpriteRenderer>() ?? trail.gameObject.AddComponent<SpriteRenderer>();
                }
            }

            _trailRenderer.sprite = RuntimeSpriteFactory.WhiteSquare;
            _trailRenderer.color = new Color(1f, 0.85f, 0.2f, 0.45f);
            _trailRenderer.transform.localScale = new Vector3(0.12f, 1.4f, 1f);
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
            UpdateTrailVisual();
        }

        public void Initialize(float speed)
        {
            _speed = speed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out ICaravanDamageable caravanDamageable))
            {
                DamageRequest request = WeaponSystem.CreateProjectileDamageRequest(other.bounds.ClosestPoint(transform.position), _damageType, GetDamage());
                if (caravanDamageable.ApplyDamage(request, out _))
                {
                    Destroy(gameObject);
                }
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
            return Mathf.Max(1, LevelRuntimeSettings.WeaponPower > 0 ? LevelRuntimeSettings.WeaponPower : _damage);
        }

        private void UpdateTrailVisual()
        {
            if (_trailRenderer == null)
            {
                return;
            }

            _trailPulse += Time.deltaTime * 18f;
            float alpha = 0.28f + Mathf.Sin(_trailPulse) * 0.1f;
            _trailRenderer.color = new Color(1f, 0.85f, 0.2f, alpha);
            _trailRenderer.transform.localScale = new Vector3(0.12f, 1.25f + Mathf.Sin(_trailPulse * 0.5f) * 0.18f, 1f);
        }
    }
}
