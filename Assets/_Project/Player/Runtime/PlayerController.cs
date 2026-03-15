using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    [RequireComponent(typeof(WeaponShooter))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 8f;
        [SerializeField] private float _horizontalPadding = 0.6f;
        [SerializeField] private float _bottomPadding = 0.9f;
        [SerializeField] private bool _pointerTargeting = true;

        private Camera _camera;
        private float _targetY;
        private bool _hasTargetY;

        private void Awake()
        {
            _camera = Camera.main;
            EnsurePhysicsComponents();
        }

        private void Start()
        {
            EnsureVisualComponents();
            EnsureBottomLaneY();
        }

        private void OnValidate()
        {
            _moveSpeed = Mathf.Max(0.1f, _moveSpeed);
            _horizontalPadding = Mathf.Max(0f, _horizontalPadding);
            _bottomPadding = Mathf.Max(0.1f, _bottomPadding);
        }

        private void EnsureVisualComponents()
        {
            var sr = GetComponent<SpriteRenderer>();
            sr.sprite = RuntimeSpriteFactory.WhiteSquare;
            sr.color = new Color(0f, 0f, 0f, 0f);
            transform.localScale = Vector3.one;

            if (GetComponent<DroneVisualController>() == null)
            {
                gameObject.AddComponent<DroneVisualController>();
            }
        }

        private void EnsurePhysicsComponents()
        {
            var collider = GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider2D>();
            }

            collider.size = new Vector2(0.9f, 0.9f);

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
            EnsureBottomLaneY();
            HandleHorizontalMove();
        }

        private void HandleHorizontalMove()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                {
                    return;
                }
            }

            float move = Input.GetAxisRaw("Horizontal");
            float effectiveMoveSpeed = LevelRuntimeSettings.PlayerMoveSpeed > 0f ? LevelRuntimeSettings.PlayerMoveSpeed : _moveSpeed;
            float desiredX = transform.position.x + (move * effectiveMoveSpeed * Time.deltaTime);

            if (_pointerTargeting && Mathf.Approximately(move, 0f))
            {
                if (TryGetPointerWorldX(out float pointerX))
                {
                    desiredX = Mathf.MoveTowards(transform.position.x, pointerX, effectiveMoveSpeed * Time.deltaTime);
                }
            }

            float halfWidth = _camera.orthographicSize * _camera.aspect;
            float minX = -halfWidth + _horizontalPadding;
            float maxX = halfWidth - _horizontalPadding;
            float clampedX = Mathf.Clamp(desiredX, minX, maxX);
            transform.position = new Vector3(clampedX, _targetY, 0f);
        }

        private bool TryGetPointerWorldX(out float worldX)
        {
            worldX = 0f;

            if (Input.touchCount > 0)
            {
                worldX = _camera.ScreenToWorldPoint(Input.GetTouch(0).position).x;
                return true;
            }

            if (Input.GetMouseButton(0))
            {
                worldX = _camera.ScreenToWorldPoint(Input.mousePosition).x;
                return true;
            }

            return false;
        }

        private void EnsureBottomLaneY()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                {
                    return;
                }
            }

            _targetY = -_camera.orthographicSize + _bottomPadding;
            if (!_hasTargetY)
            {
                _hasTargetY = true;
                transform.position = new Vector3(transform.position.x, _targetY, 0f);
            }
        }
    }
}
