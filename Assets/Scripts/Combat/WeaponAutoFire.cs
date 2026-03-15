using UnityEngine;
using KitchenCaravan.Core;

namespace KitchenCaravan.Combat
{
    // Fixed auto-fire weapon used by the bottom drone in the prototype scene.
    public sealed class WeaponAutoFire : MonoBehaviour
    {
        [SerializeField] private ProjectileBasic _projectilePrefab;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private float _fireRate = 5f;
        [SerializeField] private float _projectileSpeed = 8f;
        [SerializeField] private int _projectileDamage = 5;

        private float _nextFireTime;
        private GameManager _gameManager;

        private void Start()
        {
            _gameManager = FindFirstObjectByType<GameManager>();
        }

        private void Update()
        {
            if (_gameManager != null && !_gameManager.IsGameplayActive)
            {
                return;
            }

            if (Time.time < _nextFireTime)
            {
                return;
            }

            _nextFireTime = Time.time + (1f / Mathf.Max(0.01f, _fireRate));
            Fire();
        }

        private void Fire()
        {
            Vector3 spawnPosition = _firePoint != null ? _firePoint.position : transform.position + Vector3.up * 0.6f;
            ProjectileBasic projectile;
            if (_projectilePrefab != null)
            {
                projectile = Instantiate(_projectilePrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                GameObject go = new GameObject("ProjectileBasic");
                go.transform.position = spawnPosition;
                projectile = go.AddComponent<ProjectileBasic>();
            }

            projectile.Initialize(_projectileSpeed, _projectileDamage);
        }
    }
}
