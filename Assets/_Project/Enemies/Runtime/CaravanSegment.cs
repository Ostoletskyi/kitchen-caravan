using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class CaravanSegment : MonoBehaviour, ICaravanDamageable
    {
        private const int VisualAntCount = 7;

        [SerializeField] private Color _normalColor = new Color(0.45f, 0.95f, 0.4f, 1f);
        [SerializeField] private Color _heavyColor = new Color(0.9f, 0.55f, 0.25f, 1f);
        [SerializeField] private Color _chestColor = new Color(1f, 0.88f, 0.25f, 1f);
        [SerializeField] private Vector2 _payloadSize = new Vector2(0.52f, 0.42f);

        private CaravanController _controller;
        private int _hp;
        private bool _dead;
        private SpriteRenderer _payloadRenderer;
        private SegmentHealthLabel _healthLabel;
        private SpriteRenderer _chestGlowRenderer;
        private SpriteRenderer _chestIconRenderer;

        public int SegmentIndex { get; private set; }
        public int CurrentHealth => _hp;
        public CaravanPayloadType PayloadType { get; private set; }
        public float PositionOnRoute { get; private set; }
        public bool IsChestCarrier { get; private set; }

        public void Initialize(CaravanController controller, int hp, int segmentIndex, CaravanPayloadType payloadType, bool isChestCarrier)
        {
            _controller = controller;
            _hp = Mathf.Max(1, hp);
            SegmentIndex = Mathf.Max(1, segmentIndex);
            PayloadType = payloadType;
            IsChestCarrier = isChestCarrier;
            _dead = false;
            EnsurePhysics();
            EnsureVisual();
            EnsureHealthLabel();
            RefreshVisualState();
            RefreshHealthLabel();
        }

        private void Start()
        {
            EnsurePhysics();
            EnsureVisual();
            EnsureHealthLabel();
            RefreshVisualState();
            RefreshHealthLabel();
        }

        private void Update()
        {
            UpdateChestHighlight();
        }

        public bool ApplyDamage(DamageRequest request, out DamageResult result)
        {
            result = default;
            if (_dead)
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
                _controller.NotifySegmentDestroyed(this);
            }
            else
            {
                Destroy(gameObject);
            }

            return true;
        }

        public void SetFormationState(int segmentIndex, float routeDistance, Vector3 worldPosition)
        {
            SegmentIndex = Mathf.Max(1, segmentIndex);
            PositionOnRoute = routeDistance;
            transform.position = worldPosition;
            RefreshVisualState();
        }

        private void EnsureVisual()
        {
            var sr = GetComponent<SpriteRenderer>();
            sr.sprite = RuntimeSpriteFactory.WhiteSquare;
            sr.color = new Color(0f, 0f, 0f, 0f);

            if (_payloadRenderer == null)
            {
                Transform payload = transform.Find("Payload");
                if (payload == null)
                {
                    var payloadObject = new GameObject("Payload");
                    payloadObject.transform.SetParent(transform, false);
                    payloadObject.transform.localPosition = new Vector3(0f, 0.16f, 0f);
                    _payloadRenderer = payloadObject.AddComponent<SpriteRenderer>();
                }
                else
                {
                    _payloadRenderer = payload.GetComponent<SpriteRenderer>();
                    if (_payloadRenderer == null)
                    {
                        _payloadRenderer = payload.gameObject.AddComponent<SpriteRenderer>();
                    }
                }
            }

            _payloadRenderer.sprite = RuntimeSpriteFactory.WhiteSquare;
            EnsureChestVisuals();
            _payloadRenderer.transform.localScale = new Vector3(_payloadSize.x, _payloadSize.y, 1f);
            EnsureAntVisuals();
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

        private void EnsureChestVisuals()
        {
            if (_chestGlowRenderer == null)
            {
                Transform glow = transform.Find("ChestGlow");
                if (glow == null)
                {
                    var glowObject = new GameObject("ChestGlow");
                    glowObject.transform.SetParent(transform, false);
                    glowObject.transform.localPosition = new Vector3(0f, 0.18f, 0.08f);
                    _chestGlowRenderer = glowObject.AddComponent<SpriteRenderer>();
                }
                else
                {
                    _chestGlowRenderer = glow.GetComponent<SpriteRenderer>() ?? glow.gameObject.AddComponent<SpriteRenderer>();
                }

                _chestGlowRenderer.sprite = RuntimeSpriteFactory.WhiteSquare;
                _chestGlowRenderer.transform.localScale = new Vector3(0.9f, 0.68f, 1f);
            }

            if (_chestIconRenderer == null)
            {
                Transform icon = transform.Find("ChestIcon");
                if (icon == null)
                {
                    var iconObject = new GameObject("ChestIcon");
                    iconObject.transform.SetParent(transform, false);
                    iconObject.transform.localPosition = new Vector3(0f, 0.2f, -0.05f);
                    _chestIconRenderer = iconObject.AddComponent<SpriteRenderer>();
                }
                else
                {
                    _chestIconRenderer = icon.GetComponent<SpriteRenderer>() ?? icon.gameObject.AddComponent<SpriteRenderer>();
                }

                _chestIconRenderer.sprite = RuntimeSpriteFactory.WhiteSquare;
                _chestIconRenderer.transform.localScale = new Vector3(0.28f, 0.28f, 1f);
            }
        }

        private void EnsureAntVisuals()
        {
            for (int i = 0; i < VisualAntCount; i++)
            {
                string antName = $"Ant_{i}";
                Transform ant = transform.Find(antName);
                if (ant == null)
                {
                    var antObject = new GameObject(antName);
                    antObject.transform.SetParent(transform, false);
                    ant = antObject.transform;
                    ant.gameObject.AddComponent<SpriteRenderer>();
                }

                ant.localPosition = GetAntLocalPosition(i);
                ant.localScale = Vector3.one * 0.14f;

                var antRenderer = ant.GetComponent<SpriteRenderer>();
                antRenderer.sprite = RuntimeSpriteFactory.WhiteSquare;
                antRenderer.color = new Color(0.35f, 0.18f, 0.1f, 1f);
            }
        }

        private void RefreshVisualState()
        {
            if (_payloadRenderer == null)
            {
                return;
            }

            _payloadRenderer.color = GetPayloadColor();
            float tint = Mathf.Clamp01(1f - (SegmentIndex - 1) * 0.03f);
            _payloadRenderer.color *= tint;
            _payloadRenderer.transform.localScale = IsChestCarrier ? new Vector3(0.6f, 0.48f, 1f) : new Vector3(_payloadSize.x, _payloadSize.y, 1f);

            if (_chestGlowRenderer != null)
            {
                _chestGlowRenderer.enabled = IsChestCarrier;
            }

            if (_chestIconRenderer != null)
            {
                _chestIconRenderer.enabled = IsChestCarrier;
                if (IsChestCarrier)
                {
                    _chestIconRenderer.color = new Color(0.52f, 0.22f, 0.04f, 1f);
                }
            }
        }

        private void RefreshHealthLabel()
        {
            if (_healthLabel != null)
            {
                _healthLabel.SetValue(_hp);
            }
        }

        private void UpdateChestHighlight()
        {
            if (!IsChestCarrier || _chestGlowRenderer == null)
            {
                return;
            }

            float pulse = 0.72f + Mathf.Sin(Time.time * 6f) * 0.18f;
            _chestGlowRenderer.color = new Color(1f, 0.92f, 0.3f, pulse);
            _chestGlowRenderer.transform.localScale = new Vector3(0.92f, 0.7f, 1f) * (1f + Mathf.Sin(Time.time * 4f) * 0.06f);

            if (_chestIconRenderer != null)
            {
                _chestIconRenderer.transform.localScale = Vector3.one * (0.28f + Mathf.Sin(Time.time * 5f) * 0.015f);
            }
        }

        private Color GetPayloadColor()
        {
            if (IsChestCarrier || PayloadType == CaravanPayloadType.ChestPayload)
            {
                return _chestColor;
            }

            return PayloadType == CaravanPayloadType.HeavyPayload ? _heavyColor : _normalColor;
        }

        private static Vector3 GetAntLocalPosition(int index)
        {
            float x = -0.3f + (index % 4) * 0.2f;
            float y = index < 4 ? -0.12f : -0.3f;
            return new Vector3(x, y, 0f);
        }

        private void EnsurePhysics()
        {
            var collider = GetComponent<CircleCollider2D>();
            collider.radius = 0.42f;
            collider.isTrigger = false;
        }
    }
}
