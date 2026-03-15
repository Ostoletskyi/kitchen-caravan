using System;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public class CaravanController : MonoBehaviour
    {
        private const float RageInterval = 10f;
        private const float RageDuration = 2f;
        private const float RageMultiplier = 2f;

        private readonly List<CaravanSegment> _segments = new List<CaravanSegment>();

        private CaptainHead _captain;
        private CaravanRuntimeSettings _settings;
        private RouteLayoutData _routeLayout;
        private GameFlowController _flow;
        private LineRenderer _routeRenderer;
        private float _captainDistance;
        private float _effectiveSegmentSpacing;
        private float _rageTimer;
        private bool _configured;
        private bool _destroyedRaised;
        private bool _captainKilled;
        private bool _isRaging;

        public event Action<CaravanController, bool> Destroyed;
        public event Action<CaravanSegment> ChestSegmentDestroyed;

        public bool HasLivingSegments
        {
            get
            {
                PruneNullSegments();
                return _segments.Count > 0;
            }
        }

        public int LivingSegmentCount
        {
            get
            {
                PruneNullSegments();
                return _segments.Count;
            }
        }

        public bool IsRaging => _isRaging;

        public void Configure(CaravanRuntimeSettings settings, GameFlowController flow)
        {
            _settings = settings;
            _flow = flow;
            _configured = true;
            SpawnChain();
        }

        private void Start()
        {
            if (!_configured)
            {
                Configure(new CaravanRuntimeSettings
                {
                    levelNumber = 1,
                    chainLength = 10,
                    segmentBaseHp = 20,
                    segmentLevelGrowth = 0.10f,
                    segmentPositionGrowth = 0.25f,
                    normalPayloadHpMultiplier = 1f,
                    chestPayloadHpMultiplier = 1.35f,
                    heavyPayloadHpMultiplier = 1.6f,
                    captainHp = 100,
                    moveSpeed = 1.85f,
                    segmentSpacing = 0.9f,
                    routeData = null,
                    segmentData = null
                }, FindFirstObjectByType<GameFlowController>());
            }
        }

        private void Update()
        {
            if (_captain == null || (_flow != null && _flow.State != GameFlowController.FlowState.Playing))
            {
                return;
            }

            AdvanceCaravan();
            ApplyFormationPositions();
        }

        public void NotifySegmentDestroyed(CaravanSegment segment)
        {
            if (segment == null)
            {
                return;
            }

            if (segment.IsChestCarrier)
            {
                ChestSegmentDestroyed?.Invoke(segment);
                _ = ChestRewardSystem.BuildDefaultReward(segment);
            }

            _segments.Remove(segment);
            Destroy(segment.gameObject);

            _captainDistance = CaravanCollapseSystem.CollapseCaptainDistance(_captainDistance, _effectiveSegmentSpacing);
            ApplyFormationPositions();
        }

        public void NotifyCaptainDestroyed(CaptainHead head)
        {
            if (head != _captain)
            {
                return;
            }

            _captainKilled = true;
            RaiseDestroyed(true);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            RaiseDestroyed(_captainKilled);
        }

        private void SpawnChain()
        {
            ClearChildren();
            _segments.Clear();
            _rageTimer = 0f;
            _isRaging = false;
            _routeLayout = RouteSystem.Build(_settings.routeData, transform.position);
            _effectiveSegmentSpacing = ResolveSegmentSpacing();
            EnsureRouteRenderer();

            float visibleLeadIn = _effectiveSegmentSpacing * (_settings.chainLength + 1);
            float safeLeadIn = Mathf.Max(visibleLeadIn, _routeLayout.totalLength * 0.18f);
            _captainDistance = Mathf.Clamp(safeLeadIn, 0f, Mathf.Max(0f, _routeLayout.totalLength * 0.3f));

            GameObject captainObject = new GameObject("CaptainHead");
            captainObject.transform.SetParent(transform, false);
            _captain = captainObject.AddComponent<CaptainHead>();
            _captain.Initialize(this, Mathf.Max(1, _settings.captainHp));
            _captain.SetRageState(false);

            int chainLength = Mathf.Clamp(_settings.chainLength, 1, 100);
            CaravanSegmentRuntimeData[] segmentData = BuildSegmentDefinitions(chainLength);
            for (int i = 0; i < chainLength; i++)
            {
                int hp = SegmentHealthSystem.Evaluate(_settings, i + 1, segmentData[i]);
                GameObject segmentObject = new GameObject($"Segment_{i + 1:00}");
                segmentObject.transform.SetParent(transform, false);

                CaravanSegment segment = segmentObject.AddComponent<CaravanSegment>();
                segment.Initialize(this, hp, i + 1, segmentData[i].payloadType, segmentData[i].isChestCarrier);
                _segments.Add(segment);
            }

            ApplyFormationPositions();
        }

        private float ResolveSegmentSpacing()
        {
            if (_routeLayout.points == null || _routeLayout.points.Count < 2)
            {
                return Mathf.Max(0.35f, _settings.segmentSpacing);
            }

            float firstRowLength = Vector3.Distance(_routeLayout.points[0], _routeLayout.points[1]);
            float desiredSpacing = Mathf.Max(0.35f, _settings.segmentSpacing);
            float maxVisibleSpacing = Mathf.Max(0.35f, firstRowLength / Mathf.Max(3f, _settings.chainLength + 1.5f));
            return Mathf.Min(desiredSpacing, maxVisibleSpacing);
        }

        private void EnsureRouteRenderer()
        {
            if (_routeRenderer == null)
            {
                GameObject rendererObject = new GameObject("RouteDebugLine");
                rendererObject.transform.SetParent(transform, false);
                _routeRenderer = rendererObject.AddComponent<LineRenderer>();
                _routeRenderer.material = new Material(Shader.Find("Sprites/Default"));
                _routeRenderer.startWidth = 0.08f;
                _routeRenderer.endWidth = 0.08f;
                _routeRenderer.positionCount = 0;
                _routeRenderer.useWorldSpace = true;
                _routeRenderer.startColor = new Color(1f, 0.8f, 0.25f, 0.55f);
                _routeRenderer.endColor = new Color(1f, 0.35f, 0.25f, 0.45f);
                _routeRenderer.sortingOrder = -5;
            }

            if (_routeLayout.points == null)
            {
                return;
            }

            _routeRenderer.positionCount = _routeLayout.points.Count;
            for (int i = 0; i < _routeLayout.points.Count; i++)
            {
                _routeRenderer.SetPosition(i, _routeLayout.points[i]);
            }
        }

        private void AdvanceCaravan()
        {
            _rageTimer += Time.deltaTime;
            if (_rageTimer >= RageInterval + RageDuration)
            {
                _rageTimer -= RageInterval + RageDuration;
            }

            bool rageNow = _rageTimer >= RageInterval;
            if (rageNow != _isRaging)
            {
                _isRaging = rageNow;
                _captain?.SetRageState(_isRaging);
            }

            float moveSpeed = Mathf.Max(0.1f, _settings.moveSpeed) * (_isRaging ? RageMultiplier : 1f);
            _captainDistance += moveSpeed * Time.deltaTime;
            if (_captainDistance >= _routeLayout.totalLength)
            {
                _captainDistance = _routeLayout.totalLength;
                _flow?.TriggerLose();
            }
        }

        private void ApplyFormationPositions()
        {
            if (_captain == null)
            {
                return;
            }

            Vector3 captainPosition = RouteSystem.SamplePosition(_routeLayout, _captainDistance, transform.position);
            _captain.SetWorldPosition(captainPosition);

            for (int i = 0; i < _segments.Count; i++)
            {
                CaravanSegment segment = _segments[i];
                if (segment == null)
                {
                    continue;
                }

                float routeDistance = _captainDistance - _effectiveSegmentSpacing * (i + 1);
                Vector3 segmentPosition = RouteSystem.SamplePosition(_routeLayout, routeDistance, transform.position);
                segment.SetFormationState(i + 1, routeDistance, segmentPosition);
            }
        }

        private CaravanSegmentRuntimeData[] BuildSegmentDefinitions(int chainLength)
        {
            var result = new CaravanSegmentRuntimeData[chainLength];
            var source = _settings.segmentData;
            for (int i = 0; i < chainLength; i++)
            {
                if (source != null && i < source.Length)
                {
                    result[i] = source[i];
                    if (result[i].isChestCarrier && result[i].payloadType == CaravanPayloadType.NormalPayload)
                    {
                        result[i].payloadType = CaravanPayloadType.ChestPayload;
                    }
                }
                else
                {
                    result[i] = new CaravanSegmentRuntimeData
                    {
                        payloadType = CaravanPayloadType.NormalPayload,
                        isChestCarrier = false
                    };
                }
            }

            return result;
        }

        private void PruneNullSegments()
        {
            for (int i = _segments.Count - 1; i >= 0; i--)
            {
                if (_segments[i] == null)
                {
                    _segments.RemoveAt(i);
                }
            }
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            _routeRenderer = null;
        }

        private void RaiseDestroyed(bool countedAsDefeated)
        {
            if (_destroyedRaised)
            {
                return;
            }

            _destroyedRaised = true;
            Destroyed?.Invoke(this, countedAsDefeated);
        }

        private void OnDrawGizmos()
        {
            RouteLayoutData layout = RouteSystem.Build(_configured ? _settings.routeData : null, transform.position);
            if (layout.points == null || layout.points.Count < 2)
            {
                return;
            }

            Gizmos.color = new Color(1f, 0.82f, 0.22f, 1f);
            for (int i = 1; i < layout.points.Count; i++)
            {
                Gizmos.DrawLine(layout.points[i - 1], layout.points[i]);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(layout.points[0], 0.22f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(layout.points[layout.points.Count - 1], 0.26f);
        }
    }
}
