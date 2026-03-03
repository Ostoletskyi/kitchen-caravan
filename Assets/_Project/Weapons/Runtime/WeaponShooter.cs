using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public class WeaponShooter : MonoBehaviour
    {
        [SerializeField] private Bullet _bulletPrefab;
        [SerializeField] private float _fireRate = 5f;
        [SerializeField] private float _bulletSpeed = 12f;
        [SerializeField] private Vector3 _muzzleOffset = new Vector3(0f, 0.8f, 0f);

        private float _nextShotTime;

        public void TryShoot()
        {
            if (Time.time < _nextShotTime)
            {
                return;
            }

            _nextShotTime = Time.time + (1f / Mathf.Max(0.01f, _fireRate));

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
    }
}
