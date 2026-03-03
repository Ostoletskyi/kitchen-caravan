using System.Collections.Generic;
using UnityEngine;

namespace KitchenCaravan.Enemies.Worm
{
    public sealed class WormController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private WormHead _headPrefab;
        [SerializeField] private WormSegment _segmentPrefab;

        [Header("Chain")]
        [SerializeField, Min(0)] private int _initialSegmentCount = 6;
        [SerializeField, Min(1f)] private float _headMaxHp = 500f;
        [SerializeField, Min(1f)] private float _segmentBaseHp = 120f;
        [SerializeField, Min(0f)] private float _segmentHpStep = 25f;
        [SerializeField, Min(0.2f)] private float _segmentSpacing = 0.9f;
        [SerializeField] private bool _spawnOnStart = true;

        [Header("Head Motion")]
        [SerializeField] private Vector2 _headDirection = Vector2.right;
        [SerializeField, Min(0f)] private float _headSpeed = 2f;
        [SerializeField, Min(0f)] private float _waveAmplitude = 1.2f;
        [SerializeField, Min(0f)] private float _waveFrequency = 1.6f;

        [Header("Follow")]
        [SerializeField, Min(0.1f)] private float _trailStep = 0.15f;
        [SerializeField, Min(1f)] private float _followLerpSpeed = 18f;

        private readonly List<WormSegment> _segments = new List<WormSegment>();
        private readonly List<Vector3> _trail = new List<Vector3>();

        private WormHead _headInstance;
        private Vector3 _headOrigin;
        private Vector3 _headForward;
        private Vector3 _headPerpendicular;
        private float _elapsed;

        public bool HasLivingSegments
        {
            get
            {
                PruneNullSegments();
                return _segments.Count > 0;
            }
        }

        private void Start()
        {
            if (_spawnOnStart)
            {
                SpawnChain();
            }
        }

        private void Update()
        {
            if (_headInstance == null)
            {
                return;
            }

            PruneNullSegments();
            TickHeadMotion();
            UpdateTrail();
            UpdateSegmentsFollow();
        }

        [ContextMenu("Spawn Chain")]
        public void SpawnChain()
        {
            if (_headPrefab == null || _segmentPrefab == null)
            {
                Debug.LogError("WormController: Missing head or segment prefab reference.", this);
                return;
            }

            ClearSpawnedInstances();
            _segments.Clear();
            _trail.Clear();
            _elapsed = 0f;

            _headForward = _headDirection.sqrMagnitude <= 0.0001f
                ? Vector3.right
                : new Vector3(_headDirection.x, _headDirection.y, 0f).normalized;
            _headPerpendicular = new Vector3(-_headForward.y, _headForward.x, 0f);
            _headOrigin = transform.position;

            _headInstance = Instantiate(_headPrefab, _headOrigin, Quaternion.identity, transform);
            _headInstance.name = "WormHead";
            _headInstance.Initialize(this, _headMaxHp);

            for (int i = 0; i < _initialSegmentCount; i++)
            {
                float segmentHp = _segmentBaseHp + (_segmentHpStep * i);
                Vector3 spawnPosition = _headOrigin - (_headForward * _segmentSpacing * (i + 1));
                WormSegment segment = Instantiate(_segmentPrefab, spawnPosition, Quaternion.identity, transform);
                segment.name = $"WormSegment_{i + 1:00}";
                segment.Initialize(this, segmentHp, i);
                _segments.Add(segment);
            }

            _trail.Add(_headOrigin);
        }

        public void NotifySegmentDestroyed(WormSegment segment)
        {
            if (segment == null)
            {
                return;
            }

            bool removed = _segments.Remove(segment);
            if (removed)
            {
                Destroy(segment.gameObject);
            }
        }

        public void NotifyHeadDestroyed(WormHead head)
        {
            if (head != _headInstance)
            {
                return;
            }

            Destroy(gameObject);
        }

        private void TickHeadMotion()
        {
            _elapsed += Time.deltaTime;
            float linearTravel = _elapsed * _headSpeed;
            float wave = Mathf.Sin(_elapsed * _waveFrequency) * _waveAmplitude;
            _headInstance.transform.position = _headOrigin + (_headForward * linearTravel) + (_headPerpendicular * wave);
        }

        private void UpdateTrail()
        {
            Vector3 headPosition = _headInstance.transform.position;
            if (_trail.Count == 0)
            {
                _trail.Add(headPosition);
                return;
            }

            if (Vector3.Distance(_trail[0], headPosition) >= _trailStep)
            {
                _trail.Insert(0, headPosition);
            }
            else
            {
                _trail[0] = headPosition;
            }

            float maxDistance = _segmentSpacing * (_segments.Count + 2);
            int maxPoints = Mathf.Max(4, Mathf.CeilToInt(maxDistance / _trailStep) + 2);
            if (_trail.Count > maxPoints)
            {
                _trail.RemoveRange(maxPoints, _trail.Count - maxPoints);
            }
        }

        private void UpdateSegmentsFollow()
        {
            for (int i = 0; i < _segments.Count; i++)
            {
                WormSegment segment = _segments[i];
                if (segment == null)
                {
                    continue;
                }

                float distanceFromHead = _segmentSpacing * (i + 1);
                Vector3 targetPosition = GetTrailPosition(distanceFromHead);
                segment.UpdateFollowTarget(targetPosition, _followLerpSpeed);
            }
        }

        private Vector3 GetTrailPosition(float distance)
        {
            if (_trail.Count == 0)
            {
                return _headInstance != null ? _headInstance.transform.position : transform.position;
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

        private void ClearSpawnedInstances()
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
    }
}
