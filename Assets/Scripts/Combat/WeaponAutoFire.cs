using UnityEngine;
using KitchenCaravan.Caravan;
using KitchenCaravan.Core;
using KitchenCaravan.Utils;

namespace KitchenCaravan.Combat
{
    // Drives the bottom drone combat loop: target selection, auto-fire, and simple rotor animation.
    public sealed class WeaponAutoFire : MonoBehaviour
    {
        [SerializeField] private ProjectileBasic _projectilePrefab;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private Transform _leftRotor;
        [SerializeField] private Transform _rightRotor;
        [SerializeField] private float _fireRate = 6f;
        [SerializeField] private float _projectileSpeed = 10f;
        [SerializeField] private int _projectileDamage = 5;
        [SerializeField] private float _rotorSpinSpeed = 900f;
        [SerializeField] private float _aimBiasUp = 0.65f;

        private float _nextFireTime;
        private GameManager _gameManager;
        private CaravanController _caravan;

        private void Start()
        {
            _gameManager = FindFirstObjectByType<GameManager>();
            _caravan = FindFirstObjectByType<CaravanController>();
            EnsureVisuals();
        }

        private void Update()
        {
            SpinRotors();
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
            Vector3 spawnPosition = _firePoint != null ? _firePoint.position : transform.position + Vector3.up * 0.65f;
            Vector3 direction = Vector3.up;
            if (_caravan != null && _caravan.TryGetAimPoint(spawnPosition, out Vector3 targetPoint))
            {
                direction = (targetPoint - spawnPosition).normalized;
                direction = (direction + Vector3.up * _aimBiasUp).normalized;
            }

            ProjectileBasic projectile;
            if (_projectilePrefab != null)
            {
                projectile = Instantiate(_projectilePrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                GameObject go = new GameObject("ProjectileBasic");
                go.transform.position = spawnPosition;
                go.AddComponent<SpriteRenderer>();
                projectile = go.AddComponent<ProjectileBasic>();
            }

            projectile.Initialize(direction, _projectileSpeed, _projectileDamage);
            TemporaryHitFlash.Spawn(spawnPosition, new Color(0.9f, 1f, 0.7f, 0.55f), 0.18f);
        }

        private void EnsureVisuals()
        {
            SpriteRenderer rootRenderer = GetComponent<SpriteRenderer>();
            if (rootRenderer != null)
            {
                rootRenderer.sprite = PrototypeSpriteLibrary.WhiteSquare;
                rootRenderer.color = new Color(0.25f, 0.75f, 1f, 1f);
                transform.localScale = new Vector3(0.9f, 0.5f, 1f);
            }
        }

        private void SpinRotors()
        {
            float delta = _rotorSpinSpeed * Time.deltaTime;
            if (_leftRotor != null)
            {
                _leftRotor.Rotate(0f, 0f, delta);
            }

            if (_rightRotor != null)
            {
                _rightRotor.Rotate(0f, 0f, -delta);
            }
        }
    }
}