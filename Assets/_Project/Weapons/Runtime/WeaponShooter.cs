using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public class WeaponShooter : MonoBehaviour
    {
        [SerializeField] private Bullet _bulletPrefab;
        [SerializeField] private float _fireRate = 2f;
        [SerializeField] private float _bulletSpeed = 12f;
        [SerializeField] private Vector3 _muzzleOffset = new Vector3(0f, 0.8f, 0f);
        [SerializeField] private bool _autoFire = true;

        private float _nextShotTime;

        private void Awake()
        {
            BalanceDebugSettings.EnsureDefaults();
        }

        private void Update()
        {
            if (_autoFire)
            {
                TryShoot();
            }
        }

        public void TryShoot()
        {
            if (Time.time < _nextShotTime)
            {
                return;
            }

            float effectiveFireRate = BalanceDebugSettings.PlayerFireRate;
            if (effectiveFireRate <= 0f)
            {
                effectiveFireRate = Mathf.Max(0.01f, _fireRate);
            }

            _nextShotTime = Time.time + (1f / Mathf.Max(0.01f, effectiveFireRate));

            if (_bulletPrefab != null)
            {
                var bullet = Instantiate(_bulletPrefab, transform.position + _muzzleOffset, Quaternion.identity);
                bullet.Initialize(_bulletSpeed);
                return;
            }

            var go = new GameObject("Bullet_Runtime");
            go.transform.position = transform.position + _muzzleOffset;
            var runtimeBullet = go.AddComponent<Bullet>();
            runtimeBullet.Initialize(_bulletSpeed);
        }

        public void SetBulletPrefab(Bullet prefab)
        {
            _bulletPrefab = prefab;
        }

        public void SetAutoFire(bool enabled)
        {
            _autoFire = enabled;
        }
    }
}
