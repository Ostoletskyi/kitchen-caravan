using System.Collections.Generic;
using UnityEngine;
using KitchenCaravan.Core;
using KitchenCaravan.Route;

namespace KitchenCaravan.Caravan
{
    // Owns the full caravan runtime: movement on the route, target selection, collapse, and win/lose hooks.
    public sealed class CaravanController : MonoBehaviour
    {
        [SerializeField] private RoutePath _routePath;
        [SerializeField] private CaravanConfig _config;
        [SerializeField] private CaptainController _captainPrefab;
        [SerializeField] private SegmentController _segmentPrefab;
        [SerializeField] private bool _drawDebugPositions = true;

        private readonly List<SegmentController> _segments = new List<SegmentController>(16);
        private readonly RouteSampler _routeSampler = new RouteSampler();

        private static readonly SegmentPayloadType[] PayloadCycle =
        {
            SegmentPayloadType.Bread,
            SegmentPayloadType.Cheese,
            SegmentPayloadType.Tomato,
            SegmentPayloadType.Cucumber,
            SegmentPayloadType.Bacon,
            SegmentPayloadType.Egg
        };

        private CaptainController _captain;
        private GameManager _gameManager;
        private float _captainDistance;
        private float _rageCycleElapsed;
        private bool _initialized;

        public bool HasRemainingSegments => _segments.Count > 0;
        public int RemainingSegments => _segments.Count;

        public void Initialize(RoutePath routePath, CaravanConfig config, GameManager gameManager)
        {
            _routePath = routePath;
            _config = config;
            _gameManager = gameManager;
            BuildCaravan();
        }

        public bool TryGetAimPoint(Vector3 origin, out Vector3 aimPoint)
        {
            for (int i = 0; i < _segments.Count; i++)
            {
                if (_segments[i] == null)
                {
                    continue;
                }

                aimPoint = _segments[i].DamageAnchor.position;
                return true;
            }

            if (_captain != null && _captain.IsVulnerable)
            {
                aimPoint = _captain.DamageAnchor.position;
                return true;
            }

            aimPoint = origin + Vector3.up * 8f;
            return false;
        }

        public bool TryGetFrontPosition(out Vector3 frontPosition)
        {
            if (_captain != null)
            {
                frontPosition = _captain.transform.position;
                return true;
            }

            frontPosition = transform.position;
            return false;
        }

        private void Update()
        {
            if (!_initialized || _config == null || _routePath == null || _gameManager == null || !_gameManager.IsGameplayActive)
            {
                return;
            }

            _rageCycleElapsed += Time.deltaTime;
            bool isRaging = IsRaging();
            float currentSpeed = _config.moveSpeed * (isRaging ? _config.rageSpeedMultiplier : 1f);
            _captainDistance += currentSpeed * Time.deltaTime;
            if (_captain != null)
            {
                _captain.SetRaging(isRaging);
            }

            float routeLength = _routeSampler.GetRouteLength();
            if (_captainDistance >= routeLength)
            {
                _captainDistance = routeLength;
                ApplyFormation(Time.deltaTime);
                _gameManager.TriggerLose();
                return;
            }

            ApplyFormation(Time.deltaTime);
        }

        private void BuildCaravan()
        {
            Cleanup();
            if (_routePath == null || _config == null)
            {
                return;
            }

            _routeSampler.Rebuild(_routePath);
            _captainDistance = Mathf.Max(0f, _config.initialCaptainDistance);
            _rageCycleElapsed = 0f;

            _captain = InstantiateCaptain();
            _captain.Initialize(_config.captainHP);
            _captain.Destroyed += HandleCaptainDestroyed;

            int count = Mathf.Clamp(_config.initialSegmentCount, 1, 24);
            for (int i = 0; i < count; i++)
            {
                int index = i + 1;
                SegmentController segment = InstantiateSegment(index);
                SegmentData data = new SegmentData
                {
                    SegmentIndex = index,
                    MaxHP = _config.GetSegmentHp(index),
                    CurrentHP = _config.GetSegmentHp(index),
                    PayloadType = PayloadCycle[i % PayloadCycle.Length],
                    IsChestCarrier = false,
                    DistanceOnPath = Mathf.Max(0f, _captainDistance - index * _config.segmentSpacing)
                };

                segment.Initialize(data);
                segment.Destroyed += HandleSegmentDestroyed;
                _segments.Add(segment);
            }

            SnapFormation();
            RefreshCaptainState();
            _initialized = true;
        }

        private bool IsRaging()
        {
            float cycleLength = Mathf.Max(0.1f, _config.rageInterval + _config.rageDuration);
            float cycleTime = _rageCycleElapsed % cycleLength;
            return cycleTime >= _config.rageInterval;
        }

        private void HandleSegmentDestroyed(SegmentController segment)
        {
            segment.Destroyed -= HandleSegmentDestroyed;
            _segments.Remove(segment);
            TemporaryDisable(segment.gameObject);

            for (int i = 0; i < _segments.Count; i++)
            {
                _segments[i].SetSegmentIndex(i + 1);
            }

            RefreshCaptainState();
        }

        private void HandleCaptainDestroyed()
        {
            if (_gameManager != null)
            {
                _gameManager.TriggerWin();
            }

            if (_captain != null)
            {
                TemporaryDisable(_captain.gameObject);
            }
        }

        private void RefreshCaptainState()
        {
            if (_captain != null)
            {
                _captain.SetVulnerable(_segments.Count == 0);
            }

            if (_gameManager != null)
            {
                _gameManager.SetRemainingTargets(_segments.Count + (_captain != null ? 1 : 0), _segments.Count);
            }
        }

        private void ApplyFormation(float deltaTime)
        {
            if (_captain == null)
            {
                return;
            }

            Vector3 captainPosition = _routeSampler.GetPointAtDistance(_captainDistance);
            Vector3 captainDirection = _routeSampler.GetDirectionAtDistance(_captainDistance);
            _captain.ApplyPose(captainPosition, captainDirection, _config.positionSmoothness, _config.rotationSmoothness, deltaTime);

            for (int i = 0; i < _segments.Count; i++)
            {
                float distance = Mathf.Max(0f, _captainDistance - ((i + 1) * _config.segmentSpacing));
                Vector3 targetPosition = _routeSampler.GetPointAtDistance(distance);
                Vector3 targetDirection = _routeSampler.GetDirectionAtDistance(distance);
                _segments[i].SetDistanceOnPath(distance);
                _segments[i].ApplyPose(targetPosition, targetDirection, _config.positionSmoothness, _config.rotationSmoothness, deltaTime);
            }
        }

        private void SnapFormation()
        {
            if (_captain == null)
            {
                return;
            }

            Vector3 captainPosition = _routeSampler.GetPointAtDistance(_captainDistance);
            Vector3 captainDirection = _routeSampler.GetDirectionAtDistance(_captainDistance);
            _captain.SnapPose(captainPosition, captainDirection);

            for (int i = 0; i < _segments.Count; i++)
            {
                float distance = Mathf.Max(0f, _captainDistance - ((i + 1) * _config.segmentSpacing));
                Vector3 targetPosition = _routeSampler.GetPointAtDistance(distance);
                Vector3 targetDirection = _routeSampler.GetDirectionAtDistance(distance);
                _segments[i].SetDistanceOnPath(distance);
                _segments[i].SnapPose(targetPosition, targetDirection);
            }
        }

        private CaptainController InstantiateCaptain()
        {
            if (_captainPrefab != null)
            {
                CaptainController instance = Instantiate(_captainPrefab, transform);
                instance.name = "Captain";
                return instance;
            }

            GameObject go = new GameObject("Captain");
            go.transform.SetParent(transform, false);
            go.AddComponent<CircleCollider2D>();
            go.AddComponent<SpriteRenderer>();
            return go.AddComponent<CaptainController>();
        }

        private SegmentController InstantiateSegment(int index)
        {
            if (_segmentPrefab != null)
            {
                SegmentController instance = Instantiate(_segmentPrefab, transform);
                instance.name = $"Segment_{index:00}";
                return instance;
            }

            GameObject go = new GameObject($"Segment_{index:00}");
            go.transform.SetParent(transform, false);
            go.AddComponent<CircleCollider2D>();
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<SegmentHealth>();
            return go.AddComponent<SegmentController>();
        }

        private void TemporaryDisable(GameObject target)
        {
            Collider2D collider = target.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            target.SetActive(false);
            if (Application.isPlaying)
            {
                Destroy(target, Mathf.Max(0.01f, _config != null ? _config.destructionPause : 0.02f));
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private void Cleanup()
        {
            if (_captain != null)
            {
                _captain.Destroyed -= HandleCaptainDestroyed;
            }

            for (int i = 0; i < _segments.Count; i++)
            {
                if (_segments[i] != null)
                {
                    _segments[i].Destroyed -= HandleSegmentDestroyed;
                }
            }

            _segments.Clear();
            _initialized = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (!_drawDebugPositions || _routePath == null || _config == null)
            {
                return;
            }

            _routeSampler.Rebuild(_routePath);
            Gizmos.color = new Color(1f, 0.3f, 0.2f, 1f);
            Gizmos.DrawSphere(_routeSampler.GetPointAtDistance(_config.initialCaptainDistance), 0.16f);
            Gizmos.color = new Color(0.1f, 0.9f, 1f, 1f);
            for (int i = 0; i < Mathf.Max(1, _config.initialSegmentCount); i++)
            {
                float distance = Mathf.Max(0f, _config.initialCaptainDistance - ((i + 1) * _config.segmentSpacing));
                Gizmos.DrawSphere(_routeSampler.GetPointAtDistance(distance), 0.1f);
            }
        }
    }
}