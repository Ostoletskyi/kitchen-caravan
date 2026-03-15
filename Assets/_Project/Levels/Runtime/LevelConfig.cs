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
        [SerializeField] private int _caravanChainLength = 10;
        [SerializeField] private int _segmentBaseHp = 20;
        [SerializeField] private float _segmentLevelGrowth = 0.10f;
        [SerializeField] private float _segmentPositionGrowth = 0.15f;
        [SerializeField] private float _normalPayloadHpMultiplier = 1f;
        [SerializeField] private float _chestPayloadHpMultiplier = 1.35f;
        [SerializeField] private float _heavyPayloadHpMultiplier = 1.6f;
        [SerializeField] private int _captainHp = 15;
        [SerializeField] private float _caravanMovementSpeed = 1.8f;
        [SerializeField] private float _spawnDelay = 3f;
        [SerializeField] private float _segmentSpacing = 0.85f;
        [SerializeField] private CaravanSegmentRuntimeData[] _segmentDefinitions;

        [Header("Player")]
        [SerializeField] private float _playerMoveSpeed = 8f;
        [SerializeField] private float _playerFireRate = 2f;

        [Header("Damage")]
        [SerializeField] private int _weaponPower = 1;
        [SerializeField] private float _normalBuffPercent = 0f;
        [SerializeField] private float _critBuffPercent = 0f;
        [SerializeField] private float _upgradePercent = 0f;
        [SerializeField] private int _purchasedBonus = 0;
        [SerializeField, Range(0f, 1f)] private float _criticalChance = 0.10f;
        [SerializeField] private float _criticalMultiplier = 2f;

        public int LevelNumber => Mathf.Max(1, _levelNumber);
        public int RouteId => Mathf.Max(1, _routeId);
        public EnemyRouteData RouteData => _routeData;

        public int CaravanChainLength => Mathf.Clamp(_caravanChainLength, 1, 100);
        public int SegmentBaseHp => Mathf.Max(1, _segmentBaseHp);
        public float SegmentLevelGrowth => Mathf.Max(0f, _segmentLevelGrowth);
        public float SegmentPositionGrowth => Mathf.Max(0f, _segmentPositionGrowth);
        public float NormalPayloadHpMultiplier => Mathf.Max(0.1f, _normalPayloadHpMultiplier);
        public float ChestPayloadHpMultiplier => Mathf.Max(0.1f, _chestPayloadHpMultiplier);
        public float HeavyPayloadHpMultiplier => Mathf.Max(0.1f, _heavyPayloadHpMultiplier);
        public int CaptainHp => Mathf.Max(1, _captainHp);
        public float CaravanMovementSpeed => Mathf.Max(0.2f, _caravanMovementSpeed);
        public float SpawnDelay => Mathf.Max(0.2f, _spawnDelay);
        public float SegmentSpacing => Mathf.Max(0.2f, _segmentSpacing);
        public CaravanSegmentRuntimeData[] SegmentDefinitions => _segmentDefinitions;
        public float PlayerMoveSpeed => Mathf.Max(0.5f, _playerMoveSpeed);
        public float PlayerFireRate => Mathf.Max(0.1f, _playerFireRate);
        public int WeaponPower => Mathf.Max(1, _weaponPower);
        public float NormalBuffPercent => Mathf.Max(0f, _normalBuffPercent);
        public float CritBuffPercent => Mathf.Max(0f, _critBuffPercent);
        public float UpgradePercent => Mathf.Max(0f, _upgradePercent);
        public int PurchasedBonus => Mathf.Max(0, _purchasedBonus);
        public float CriticalChance => Mathf.Clamp01(_criticalChance);
        public float CriticalMultiplier => Mathf.Max(1f, _criticalMultiplier);
    }
}
