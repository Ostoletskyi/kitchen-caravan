using UnityEngine;
using System.Collections.Generic;

namespace KitchenCaravan.VerticalSlice
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private float _spawnInterval = 3f;
        [SerializeField] private float _spawnY = 6f;
        [SerializeField] private float _minX = -7.5f;
        [SerializeField] private float _maxX = 7.5f;
        [Header("Caravan Defaults")]
        [SerializeField] private int _defaultChainLength = 6;
        [SerializeField] private int _defaultSegmentBaseHp = 3;
        [SerializeField] private int _defaultSegmentHpIncrement = 2;
        [SerializeField] private int _defaultCaptainHp = 12;
        [SerializeField] private float _defaultChainSpeed = 1.8f;
        [SerializeField] private float _defaultSpacing = 0.9f;
        [SerializeField] private float _defaultSwayAmplitude = 1f;
        [SerializeField] private float _defaultSwayFrequency = 1.2f;
        [SerializeField] private float _defaultFollowLerpSpeed = 16f;
        [SerializeField] private float _defaultTrailStep = 0.14f;

        private float _nextSpawn;
        private GameFlowController _flow;
        private readonly List<CaravanController> _activeCaravans = new List<CaravanController>();

        private void Awake()
        {
            BalanceDebugSettings.EnsureDefaults();
        }

        public void Configure(GameFlowController flow)
        {
            _flow = flow;
        }

        private void Update()
        {
            if (_flow != null && _flow.State != GameFlowController.FlowState.Playing)
            {
                return;
            }

            if (Time.time < _nextSpawn)
            {
                return;
            }

            float delay = BalanceDebugSettings.ChainSpawnDelay > 0f ? BalanceDebugSettings.ChainSpawnDelay : _spawnInterval;
            _nextSpawn = Time.time + Mathf.Max(0.1f, delay);
            SpawnOne();
        }

        private void SpawnOne()
        {
            float x = Random.Range(_minX, _maxX);
            Vector3 pos = new Vector3(x, _spawnY, 0f);

            var caravanObject = new GameObject($"Caravan_{Time.frameCount}");
            caravanObject.transform.position = pos;
            caravanObject.transform.SetParent(transform, false);

            var caravan = caravanObject.AddComponent<CaravanController>();
            caravan.Configure(new CaravanRuntimeSettings
            {
                chainLength = BalanceDebugSettings.ChainLength > 0 ? BalanceDebugSettings.ChainLength : _defaultChainLength,
                segmentBaseHp = BalanceDebugSettings.ChainSegmentBaseHp > 0 ? BalanceDebugSettings.ChainSegmentBaseHp : _defaultSegmentBaseHp,
                segmentHpIncrement = BalanceDebugSettings.ChainSegmentHpIncrement >= 0 ? BalanceDebugSettings.ChainSegmentHpIncrement : _defaultSegmentHpIncrement,
                captainHp = Mathf.Max(1, _defaultCaptainHp),
                moveSpeed = BalanceDebugSettings.ChainMoveSpeed > 0f ? BalanceDebugSettings.ChainMoveSpeed : _defaultChainSpeed,
                segmentSpacing = _defaultSpacing,
                swayAmplitude = _defaultSwayAmplitude,
                swayFrequency = _defaultSwayFrequency,
                followLerpSpeed = _defaultFollowLerpSpeed,
                trailStep = _defaultTrailStep
            });

            caravan.Destroyed += OnCaravanDestroyed;
            _activeCaravans.Add(caravan);
        }

        private void OnCaravanDestroyed(CaravanController caravan, bool countedAsDefeated)
        {
            if (caravan != null)
            {
                caravan.Destroyed -= OnCaravanDestroyed;
                _activeCaravans.Remove(caravan);
            }

            if (countedAsDefeated)
            {
                _flow?.RegisterEnemyDefeated();
            }
        }

        private void LateUpdate()
        {
            for (int i = _activeCaravans.Count - 1; i >= 0; i--)
            {
                if (_activeCaravans[i] == null)
                {
                    _activeCaravans.RemoveAt(i);
                }
            }
        }
    }
}
