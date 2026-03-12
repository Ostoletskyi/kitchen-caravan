using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private int _maxHp = 2;
        [SerializeField] private float _fallSpeed = 2.5f;
        [SerializeField] private float _sineAmplitude = 0.35f;
        [SerializeField] private float _sineFrequency = 2f;
        [SerializeField] private float _despawnY = -7f;

        private int _hp;
        private float _phase;
        private float _baseX;
        private GameFlowController _flow;

        private void Awake()
        {
            _hp = _maxHp;
            _phase = Random.Range(0f, Mathf.PI * 2f);
            _baseX = transform.position.x;
            EnsurePhysicsComponents();
        }

        private void Start()
        {
            EnsureVisualComponents();
        }

        private void OnValidate()
        {
            _maxHp = Mathf.Max(1, _maxHp);
        }

        private void EnsureVisualComponents()
        {
            var sr = GetComponent<SpriteRenderer>();
            sr.sprite = RuntimeSpriteFactory.WhiteSquare;
            sr.color = new Color(1f, 0.35f, 0.35f, 1f);
            transform.localScale = new Vector3(0.85f, 0.85f, 1f);
        }

        private void EnsurePhysicsComponents()
        {
            var collider = GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider2D>();
            }

            collider.isTrigger = false;

            var body = GetComponent<Rigidbody2D>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody2D>();
            }

            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
        }

        private void Update()
        {
            float t = Time.time + _phase;
            float drift = Mathf.Sin(t * _sineFrequency) * _sineAmplitude;

            Vector3 pos = transform.position;
            pos.y -= _fallSpeed * Time.deltaTime;
            pos.x = _baseX + drift;
            transform.position = pos;

            if (transform.position.y < _despawnY)
            {
                Destroy(gameObject);
            }
        }

        public void SetFlow(GameFlowController flow)
        {
            _flow = flow;
        }

        public void ApplyDamage(int amount)
        {
            _hp -= amount;
            if (_hp > 0)
            {
                return;
            }

            _flow?.RegisterEnemyDefeated();
            Destroy(gameObject);
        }
    }
}
