using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    [RequireComponent(typeof(WeaponShooter))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 8f;
        [SerializeField] private float _padding = 0.6f;

        private WeaponShooter _weapon;
        private Camera _camera;

        private void Awake()
        {
            _weapon = GetComponent<WeaponShooter>();
            _camera = Camera.main;
            EnsureComponents();
        }

        private void OnValidate()
        {
            EnsureComponents();
        }

        private void EnsureComponents()
        {
            var sr = GetComponent<SpriteRenderer>();
            sr.sprite = RuntimeSpriteFactory.WhiteSquare;
            sr.color = new Color(0.35f, 0.85f, 1f, 1f);
            transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            var collider = GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;
            }

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
            float move = Input.GetAxisRaw("Horizontal");
            transform.position += Vector3.right * (move * _moveSpeed * Time.deltaTime);
            ClampToViewport();

            if (Input.GetKey(KeyCode.Space))
            {
                _weapon.TryShoot();
            }
        }

        private void ClampToViewport()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                {
                    return;
                }
            }

            float halfWidth = _camera.orthographicSize * _camera.aspect;
            float minX = -halfWidth + _padding;
            float maxX = halfWidth - _padding;
            float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
            transform.position = new Vector3(clampedX, transform.position.y, 0f);
        }
    }
}
