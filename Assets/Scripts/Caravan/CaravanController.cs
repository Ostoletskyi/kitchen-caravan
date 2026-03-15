using System.Collections.Generic;
using UnityEngine;
using KitchenCaravan.Route;
using KitchenCaravan.Core;

namespace KitchenCaravan.Caravan
{
    // Moves the captain and 8 segments along a fixed route and closes gaps after segment destruction.
    public sealed class CaravanController : MonoBehaviour
    {
        [SerializeField] private RoutePath _routePath;
        [SerializeField] private CaravanConfig _config;
        [SerializeField] private CaptainController _captainPrefab;
        [SerializeField] private SegmentController _segmentPrefab;
        [SerializeField] private bool _drawDebugPositions = true;

        private readonly List<SegmentController> _segments = new List<SegmentController>(16);
        private readonly RouteSampler _routeSampler = new RouteSampler();

        private CaptainController _captain;
        private GameManager _gameManager;
        private float _captainDistance;
        private bool _initialized;

        public IReadOnlyList<SegmentController> Segments => _segments;
        public bool HasRemainingSegments => _segments.Count > 0;

        public void Initialize(RoutePath routePath, CaravanConfig config, GameManager gameManager)
        {
            _routePath = routePath;
            _config = config;
            _gameManager = gameManager;
            BuildCaravan();
        }

        private void Update()
        {
            if (!_initialized || _config == null || _gameManager == null || !_gameManager.IsGameplayActive)
            {
                return;
            }

            _captainDistance += _config.moveSpeed * Time.deltaTime;
            float routeLength = _routeSampler.GetRouteLength();
            if (_captainDistance >= routeLength)
            {
                _captainDistance = routeLength;
                _gameManager.TriggerLose();
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

            _captain = InstantiateCaptain();
            _captain.Initialize(_config.captainHP);
            _captain.Destroyed += HandleCaptainDestroyed;

            for (int i = 0; i < Mathf.Max(1, _config.initialSegmentCount); i++)
            {
                int index = i + 1;
                SegmentController segment = InstantiateSegment(index);
                SegmentData data = new SegmentData
                {
                    SegmentIndex = index,
                    MaxHP = Mathf.RoundToInt(_config.baseHP * (1f + i * _config.positionGrowth)),
                    CurrentHP = Mathf.RoundToInt(_config.baseHP * (1f + i * _config.positionGrowth)),
                    PayloadType = SegmentPayloadType.Bread,
                    IsChestCarrier = false,
                    DistanceOnPath = 0f
                };

                segment.Initialize(data);
                segment.Destroyed += HandleSegmentDestroyed;
                _segments.Add(segment);
            }

            SnapFormation();
            _initialized = true;
        }

        private void HandleSegmentDestroyed(SegmentController segment)
        {
            segment.Destroyed -= HandleSegmentDestroyed;
            _segments.Remove(segment);
            if (Application.isPlaying)
            {
                Destroy(segment.gameObject);
            }
            else
            {
                DestroyImmediate(segment.gameObject);
            }

            for (int i = 0; i < _segments.Count; i++)
            {
                _segments[i].SetSegmentIndex(i + 1);
            }

            if (_captain != null)
            {
                _captain.SetVulnerable(_segments.Count == 0);
            }
        }

        private void HandleCaptainDestroyed()
        {
            if (_gameManager != null)
            {
                _gameManager.TriggerWin();
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
                return Instantiate(_captainPrefab, transform);
            }

            GameObject go = new GameObject("Captain");
            go.transform.SetParent(transform, false);
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
            return go.AddComponent<SegmentController>();
        }

        private void Cleanup()
        {
            if (_captain != null)
            {
                _captain.Destroyed -= HandleCaptainDestroyed;
            }

            for (int i = _segments.Count - 1; i >= 0; i--)
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
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_routeSampler.GetPointAtDistance(_config.initialCaptainDistance), 0.16f);
            Gizmos.color = Color.cyan;
            for (int i = 0; i < Mathf.Max(1, _config.initialSegmentCount); i++)
            {
                float distance = Mathf.Max(0f, _config.initialCaptainDistance - ((i + 1) * _config.segmentSpacing));
                Gizmos.DrawSphere(_routeSampler.GetPointAtDistance(distance), 0.1f);
            }
        }
    }
}
