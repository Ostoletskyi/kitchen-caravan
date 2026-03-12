using System;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public class CaravanController : MonoBehaviour
    {
        private readonly List<CaravanSegment> _segments = new List<CaravanSegment>();
        private readonly List<Vector3> _trail = new List<Vector3>();

        private CaptainHead _captain;
        private CaravanRuntimeSettings _settings;
        private float _elapsed;
        private float _originX;
        private int _routeSegmentIndex;
        private Vector3 _fallbackForward = Vector3.down;
        private readonly List<Vector3> _routePoints = new List<Vector3>();
        private bool _configured;
        private bool _destroyedRaised;
        private bool _captainKilled;

        public event Action<CaravanController, bool> Destroyed;

        public bool HasLivingSegments
        {
            get
            {
                PruneNullSegments();
                return _segments.Count > 0;
            }
        }

        public void Configure(CaravanRuntimeSettings settings)
        {
            _settings = settings;
            _configured = true;
            SpawnChain();
        }

        private void Start()
        {
            if (!_configured)
            {
                Configure(new CaravanRuntimeSettings
                {
                    chainLength = 6,
                    segmentBaseHp = 3,
                    segmentHpIncrement = 2,
                    captainHp = 12,
                    moveSpeed = 1.8f,
                    segmentSpacing = 0.9f,
                    swayAmplitude = 1f,
                    swayFrequency = 1.2f,
                    followLerpSpeed = 16f,
                    trailStep = 0.14f
                });
            }
        }

        private void Update()
        {
            if (_captain == null)
            {
                return;
            }

            TickCaptainMotion();
            UpdateTrail();
            UpdateFollowers();
        }

        public void NotifySegmentDestroyed(CaravanSegment segment)
        {
            if (segment == null)
            {
                return;
            }

            _segments.Remove(segment);
            Destroy(segment.gameObject);
        }

        public void NotifyCaptainDestroyed(CaptainHead head)
        {
            if (head != _captain)
            {
                return;
            }

            _captainKilled = true;
            RaiseDestroyed(_captainKilled);
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
            _trail.Clear();
            _routePoints.Clear();
            _elapsed = 0f;
            _originX = transform.position.x;
            _routeSegmentIndex = 0;

            CacheRoutePoints();
            EnsureFallbackDirection();

            GameObject captainObject = new GameObject("CaptainHead");
            captainObject.transform.SetParent(transform, false);
            captainObject.transform.position = GetSpawnPosition();
            _captain = captainObject.AddComponent<CaptainHead>();
            _captain.Initialize(this, Mathf.Max(1, _settings.captainHp));

            int chainLength = Mathf.Clamp(_settings.chainLength, 1, 100);
            int baseHp = Mathf.Max(1, _settings.segmentBaseHp);
            int increment = Mathf.Max(0, _settings.segmentHpIncrement);
            float spacing = Mathf.Max(0.3f, _settings.segmentSpacing);

            for (int i = 0; i < chainLength; i++)
            {
                int hp = baseHp + (increment * i);
                GameObject segmentObject = new GameObject($"Segment_{i + 1:00}");
                segmentObject.transform.SetParent(transform, false);
                segmentObject.transform.position = _captain.transform.position - (_fallbackForward * spacing * (i + 1));

                CaravanSegment segment = segmentObject.AddComponent<CaravanSegment>();
                segment.Initialize(this, hp, i);
                _segments.Add(segment);
            }

            _trail.Add(_captain.transform.position);
        }

        private void TickCaptainMotion()
        {
            Vector3 pos = _captain.transform.position;
            if (_routePoints.Count >= 2)
            {
                pos = MoveAlongRoute(pos, Mathf.Max(0.1f, _settings.moveSpeed) * Time.deltaTime);
                _captain.transform.position = pos;
            }
            else
            {
                _elapsed += Time.deltaTime;
                pos.y -= Mathf.Max(0.1f, _settings.moveSpeed) * Time.deltaTime;
                pos.x = _originX + Mathf.Sin(_elapsed * Mathf.Max(0f, _settings.swayFrequency)) * Mathf.Max(0f, _settings.swayAmplitude);
                _captain.transform.position = pos;
            }

            float despawnY = -9f;
            if (pos.y < despawnY)
            {
                Destroy(gameObject);
            }
        }

        private void CacheRoutePoints()
        {
            if (_settings.routeData == null || _settings.routeData.Points == null)
            {
                return;
            }

            var points = _settings.routeData.Points;
            for (int i = 0; i < points.Count; i++)
            {
                _routePoints.Add(points[i]);
            }
        }

        private Vector3 GetSpawnPosition()
        {
            if (_routePoints.Count > 0)
            {
                return _routePoints[0];
            }

            return transform.position;
        }

        private void EnsureFallbackDirection()
        {
            if (_routePoints.Count >= 2)
            {
                _fallbackForward = (_routePoints[1] - _routePoints[0]).normalized;
                if (_fallbackForward.sqrMagnitude <= 0.001f)
                {
                    _fallbackForward = Vector3.down;
                }
                return;
            }

            _fallbackForward = Vector3.down;
        }

        private Vector3 MoveAlongRoute(Vector3 currentPosition, float moveDistance)
        {
            if (_routePoints.Count < 2)
            {
                return currentPosition;
            }

            float remaining = Mathf.Max(0f, moveDistance);
            Vector3 position = currentPosition;

            while (remaining > 0f && _routeSegmentIndex < _routePoints.Count - 1)
            {
                Vector3 target = _routePoints[_routeSegmentIndex + 1];
                float distanceToTarget = Vector3.Distance(position, target);
                if (distanceToTarget <= 0.0001f)
                {
                    _routeSegmentIndex++;
                    continue;
                }

                if (remaining < distanceToTarget)
                {
                    position = Vector3.MoveTowards(position, target, remaining);
                    remaining = 0f;
                }
                else
                {
                    position = target;
                    remaining -= distanceToTarget;
                    _routeSegmentIndex++;
                }
            }

            if (_routeSegmentIndex >= _routePoints.Count - 1)
            {
                Destroy(gameObject);
            }

            return position;
        }

        private void UpdateTrail()
        {
            Vector3 headPosition = _captain.transform.position;
            float step = Mathf.Max(0.05f, _settings.trailStep);

            if (_trail.Count == 0)
            {
                _trail.Add(headPosition);
                return;
            }

            if (Vector3.Distance(_trail[0], headPosition) >= step)
            {
                _trail.Insert(0, headPosition);
            }
            else
            {
                _trail[0] = headPosition;
            }

            float maxDistance = Mathf.Max(0.3f, _settings.segmentSpacing) * (_segments.Count + 3);
            int maxPoints = Mathf.Max(8, Mathf.CeilToInt(maxDistance / step) + 3);
            if (_trail.Count > maxPoints)
            {
                _trail.RemoveRange(maxPoints, _trail.Count - maxPoints);
            }
        }

        private void UpdateFollowers()
        {
            PruneNullSegments();

            float spacing = Mathf.Max(0.3f, _settings.segmentSpacing);
            float followSpeed = Mathf.Max(1f, _settings.followLerpSpeed);
            for (int i = 0; i < _segments.Count; i++)
            {
                CaravanSegment segment = _segments[i];
                float distanceFromHead = spacing * (i + 1);
                Vector3 targetPosition = GetTrailPosition(distanceFromHead);
                segment.UpdateFollowTarget(targetPosition, followSpeed);
            }
        }

        private Vector3 GetTrailPosition(float distance)
        {
            if (_trail.Count == 0)
            {
                return _captain != null ? _captain.transform.position : transform.position;
            }

            float remaining = distance;
            for (int i = 0; i < _trail.Count - 1; i++)
            {
                Vector3 from = _trail[i];
                Vector3 to = _trail[i + 1];
                float segmentLength = Vector3.Distance(from, to);
                if (segmentLength <= 0.0001f)
                {
                    continue;
                }

                if (remaining <= segmentLength)
                {
                    return Vector3.Lerp(from, to, remaining / segmentLength);
                }

                remaining -= segmentLength;
            }

            return _trail[_trail.Count - 1];
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
    }
}
