using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Enemy _enemyPrefab;
        [SerializeField] private float _spawnInterval = 0.7f;
        [SerializeField] private float _spawnY = 6f;
        [SerializeField] private float _minX = -7.5f;
        [SerializeField] private float _maxX = 7.5f;

        private float _nextSpawn;
        private GameFlowController _flow;

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

            _nextSpawn = Time.time + _spawnInterval;
            SpawnOne();
        }

        private void SpawnOne()
        {
            float x = Random.Range(_minX, _maxX);
            Vector3 pos = new Vector3(x, _spawnY, 0f);

            Enemy enemy;
            if (_enemyPrefab != null)
            {
                enemy = Instantiate(_enemyPrefab, pos, Quaternion.identity);
            }
            else
            {
                var go = new GameObject("Enemy_Runtime");
                go.transform.position = pos;
                enemy = go.AddComponent<Enemy>();
            }

            enemy.SetFlow(_flow);
        }

        public void SetEnemyPrefab(Enemy prefab)
        {
            _enemyPrefab = prefab;
        }
    }
}
