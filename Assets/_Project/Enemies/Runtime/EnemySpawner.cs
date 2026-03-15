using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public class EnemySpawner : MonoBehaviour
    {
        private GameFlowController _flow;
        private CaravanController _activeCaravan;
        private bool _spawned;

        public void Configure(GameFlowController flow)
        {
            _flow = flow;
            SpawnIfNeeded();
        }

        private void Start()
        {
            SpawnIfNeeded();
        }

        private void Update()
        {
            if (_flow != null && _flow.State != GameFlowController.FlowState.Playing)
            {
                return;
            }

            SpawnIfNeeded();
        }

        private void SpawnIfNeeded()
        {
            if (_spawned)
            {
                return;
            }

            _spawned = true;
            SpawnOne();
        }

        private void SpawnOne()
        {
            Vector3 pos = GetSpawnPosition();
            var caravanObject = new GameObject("KitchenCaravan_MainCaravan");
            caravanObject.transform.position = pos;
            caravanObject.transform.SetParent(transform, false);

            var caravan = caravanObject.AddComponent<CaravanController>();
            caravan.Configure(new CaravanRuntimeSettings
            {
                levelNumber = LevelRuntimeSettings.LevelNumber,
                chainLength = 10,
                segmentBaseHp = 20,
                segmentLevelGrowth = LevelRuntimeSettings.SegmentLevelGrowth,
                segmentPositionGrowth = 0.25f,
                normalPayloadHpMultiplier = LevelRuntimeSettings.NormalPayloadHpMultiplier,
                chestPayloadHpMultiplier = LevelRuntimeSettings.ChestPayloadHpMultiplier,
                heavyPayloadHpMultiplier = LevelRuntimeSettings.HeavyPayloadHpMultiplier,
                captainHp = 100,
                moveSpeed = 2f,
                segmentSpacing = 0.9f,
                routeData = LevelRuntimeSettings.RouteData,
                segmentData = LevelRuntimeSettings.SegmentDefinitions
            }, _flow);

            caravan.Destroyed += OnCaravanDestroyed;
            _activeCaravan = caravan;
        }

        private void OnCaravanDestroyed(CaravanController caravan, bool countedAsDefeated)
        {
            if (caravan != null)
            {
                caravan.Destroyed -= OnCaravanDestroyed;
                if (_activeCaravan == caravan)
                {
                    _activeCaravan = null;
                }
            }

            if (countedAsDefeated)
            {
                _flow?.RegisterEnemyDefeated();
            }
        }

        private Vector3 GetSpawnPosition()
        {
            var route = RouteSystem.Build(LevelRuntimeSettings.RouteData, transform.position);
            if (route.points != null && route.points.Count > 0)
            {
                return route.points[0];
            }

            return transform.position;
        }
    }
}