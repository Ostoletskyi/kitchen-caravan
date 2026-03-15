using UnityEngine;
using KitchenCaravan.Core;
using KitchenCaravan.Route;

namespace KitchenCaravan.Caravan
{
    // Spawns and configures the single Level 1 caravan at scene start.
    public sealed class CaravanSpawner : MonoBehaviour
    {
        [SerializeField] private RoutePath _routePath;
        [SerializeField] private CaravanConfig _caravanConfig;
        [SerializeField] private CaravanController _caravanPrefab;
        [SerializeField] private GameManager _gameManager;

        private CaravanController _spawnedCaravan;

        private void Start()
        {
            Spawn();
        }

        public CaravanController Spawn()
        {
            if (_spawnedCaravan != null)
            {
                return _spawnedCaravan;
            }

            if (_gameManager == null)
            {
                _gameManager = FindFirstObjectByType<GameManager>();
            }

            if (_caravanPrefab != null)
            {
                _spawnedCaravan = Instantiate(_caravanPrefab, transform.position, Quaternion.identity, transform);
            }
            else
            {
                GameObject go = new GameObject("PrototypeCaravan");
                go.transform.SetParent(transform, false);
                _spawnedCaravan = go.AddComponent<CaravanController>();
            }

            _spawnedCaravan.Initialize(_routePath, _caravanConfig, _gameManager);
            if (_gameManager != null)
            {
                _gameManager.RegisterCaravan(_spawnedCaravan);
            }

            return _spawnedCaravan;
        }
    }
}