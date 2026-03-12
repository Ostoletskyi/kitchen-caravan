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
            _elapsed = 0f;
            _originX = transform.position.x;

            GameObject captainObject = new GameObject("CaptainHead");
            captainObject.transform.SetParent(transform, false);
            captainObject.transform.position = transform.position;
            _captain = captainObject.AddComponent<CaptainHead>();
            _captain.Initialize(this, Mathf.Max(1, _settings.captainHp));

            int chainLength = Mathf.Max(1, _settings.chainLength);
            int baseHp = Mathf.Max(1, _settings.segmentBaseHp);
            int increment = Mathf.Max(0, _settings.segmentHpIncrement);
            float spacing = Mathf.Max(0.3f, _settings.segmentSpacing);

            for (int i = 0; i < chainLength; i++)
            {
                int hp = baseHp + (increment * i);
                GameObject segmentObject = new GameObject($"Segment_{i + 1:00}");
                segmentObject.transform.SetParent(transform, false);
                segmentObject.transform.position = transform.position + (Vector3.up * spacing * (i + 1));

                CaravanSegment segment = segmentObject.AddComponent<CaravanSegment>();
                segment.Initialize(this, hp, i);
                _segments.Add(segment);
            }

            _trail.Add(_captain.transform.position);
        }

        private void TickCaptainMotion()
        {
            _elapsed += Time.deltaTime;
            Vector3 pos = _captain.transform.position;
            pos.y -= Mathf.Max(0.1f, _settings.moveSpeed) * Time.deltaTime;
            pos.x = _originX + Mathf.Sin(_elapsed * Mathf.Max(0f, _settings.swayFrequency)) * Mathf.Max(0f, _settings.swayAmplitude);
            _captain.transform.position = pos;

            float despawnY = -9f;
            if (pos.y < despawnY)
            {
                Destroy(gameObject);
            }
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
