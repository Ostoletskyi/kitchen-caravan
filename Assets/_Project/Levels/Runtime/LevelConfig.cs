using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    [CreateAssetMenu(menuName = "KitchenCaravan/Levels/Level Config", fileName = "LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private int _levelNumber = 1;
        [SerializeField] private int _routeId = 1;
        [SerializeField] private EnemyRouteData _routeData;

        [Header("Caravan")]
        [SerializeField] private int _caravanChainLength = 24;
        [SerializeField] private int _segmentBaseHp = 3;
        [SerializeField] private int _segmentHpIncrement = 2;
        [SerializeField] private int _captainHp = 15;
        [SerializeField] private float _caravanMovementSpeed = 1.8f;
        [SerializeField] private float _spawnDelay = 3f;
        [SerializeField] private float _segmentSpacing = 0.85f;
        [SerializeField] private float _followLerpSpeed = 16f;
        [SerializeField] private float _trailStep = 0.14f;
        [SerializeField] private float _swayAmplitude = 1f;
        [SerializeField] private float _swayFrequency = 1.2f;

        [Header("Player")]
        [SerializeField] private float _playerMoveSpeed = 8f;
        [SerializeField] private float _playerFireRate = 2f;
        [SerializeField] private int _bulletDamage = 1;

        public int LevelNumber => Mathf.Max(1, _levelNumber);
        public int RouteId => Mathf.Max(1, _routeId);
        public EnemyRouteData RouteData => _routeData;

        public int CaravanChainLength => Mathf.Clamp(_caravanChainLength, 1, 100);
        public int SegmentBaseHp => Mathf.Max(1, _segmentBaseHp);
        public int SegmentHpIncrement => Mathf.Max(0, _segmentHpIncrement);
        public int CaptainHp => Mathf.Max(1, _captainHp);
        public float CaravanMovementSpeed => Mathf.Max(0.2f, _caravanMovementSpeed);
        public float SpawnDelay => Mathf.Max(0.2f, _spawnDelay);
        public float SegmentSpacing => Mathf.Max(0.2f, _segmentSpacing);
        public float FollowLerpSpeed => Mathf.Max(1f, _followLerpSpeed);
        public float TrailStep => Mathf.Max(0.02f, _trailStep);
        public float SwayAmplitude => Mathf.Max(0f, _swayAmplitude);
        public float SwayFrequency => Mathf.Max(0f, _swayFrequency);
        public float PlayerMoveSpeed => Mathf.Max(0.5f, _playerMoveSpeed);
        public float PlayerFireRate => Mathf.Max(0.1f, _playerFireRate);
        public int BulletDamage => Mathf.Max(1, _bulletDamage);
    }
}
