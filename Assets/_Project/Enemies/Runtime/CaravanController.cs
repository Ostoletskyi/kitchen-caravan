using System;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public class CaravanController : MonoBehaviour
    {
        private readonly List<CaravanSegment> _segments = new List<CaravanSegment>();

        private CaptainHead _captain;
        private CaravanRuntimeSettings _settings;
        private RouteLayoutData _routeLayout;
        private float _captainDistance;
        private bool _configured;
        private bool _destroyedRaised;
        private bool _captainKilled;

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
                    levelNumber = 1,
                    chainLength = 10,
                    segmentBaseHp = 20,
                    segmentLevelGrowth = 0.10f,
                    segmentPositionGrowth = 0.15f,
                    normalPayloadHpMultiplier = 1f,
                    chestPayloadHpMultiplier = 1.35f,
                    heavyPayloadHpMultiplier = 1.6f,
                    captainHp = 12,
                    moveSpeed = 1.8f,
                    segmentSpacing = 0.9f,
                    routeData = null,
                    segmentData = null
                });
            }
        }

        private void Update()
        {
            if (_captain == null)
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

            _captainDistance = CaravanCollapseSystem.CollapseCaptainDistance(_captainDistance, _settings.segmentSpacing);
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
            _captainDistance = 0f;
            _routeLayout = RouteSystem.Build(_settings.routeData, transform.position);

            GameObject captainObject = new GameObject("CaptainHead");
            captainObject.transform.SetParent(transform, false);
            _captain = captainObject.AddComponent<CaptainHead>();
            _captain.Initialize(this, Mathf.Max(1, _settings.captainHp));

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

        private void AdvanceCaravan()
        {
            _captainDistance += Mathf.Max(0.1f, _settings.moveSpeed) * Time.deltaTime;
            if (_captainDistance > _routeLayout.totalLength)
            {
                Destroy(gameObject);
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

            float spacing = Mathf.Max(0.2f, _settings.segmentSpacing);
            for (int i = 0; i < _segments.Count; i++)
            {
                CaravanSegment segment = _segments[i];
                if (segment == null)
                {
                    continue;
                }

                float routeDistance = _captainDistance - spacing * (i + 1);
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
