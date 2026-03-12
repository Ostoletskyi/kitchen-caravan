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

        private float _nextSpawn;
        private GameFlowController _flow;
        private readonly List<CaravanController> _activeCaravans = new List<CaravanController>();

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

            float delay = LevelRuntimeSettings.SpawnDelay > 0f ? LevelRuntimeSettings.SpawnDelay : _spawnInterval;
            _nextSpawn = Time.time + Mathf.Max(0.1f, delay);
            SpawnOne();
        }

        private void SpawnOne()
        {
            Vector3 pos = GetSpawnPosition();

            var caravanObject = new GameObject($"Caravan_{Time.frameCount}");
            caravanObject.transform.position = pos;
            caravanObject.transform.SetParent(transform, false);

            var caravan = caravanObject.AddComponent<CaravanController>();
            caravan.Configure(new CaravanRuntimeSettings
            {
                chainLength = Mathf.Clamp(LevelRuntimeSettings.ChainLength, 1, 100),
                segmentBaseHp = LevelRuntimeSettings.SegmentBaseHp,
                segmentHpIncrement = LevelRuntimeSettings.SegmentHpIncrement,
                captainHp = LevelRuntimeSettings.CaptainHp,
                moveSpeed = LevelRuntimeSettings.ChainMoveSpeed,
                segmentSpacing = LevelRuntimeSettings.SegmentSpacing,
                swayAmplitude = LevelRuntimeSettings.SwayAmplitude,
                swayFrequency = LevelRuntimeSettings.SwayFrequency,
                followLerpSpeed = LevelRuntimeSettings.FollowLerpSpeed,
                trailStep = LevelRuntimeSettings.TrailStep,
                routeData = LevelRuntimeSettings.RouteData
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

        private Vector3 GetSpawnPosition()
        {
            var route = LevelRuntimeSettings.RouteData;
            if (route != null && route.Points != null && route.Points.Count > 0)
            {
                return route.Points[0];
            }

            float x = Random.Range(_minX, _maxX);
            return new Vector3(x, _spawnY, 0f);
        }
    }
}
