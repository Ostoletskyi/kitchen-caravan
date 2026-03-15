namespace KitchenCaravan.VerticalSlice
{
    public static class LevelRuntimeSettings
    {
        public static int LevelNumber { get; private set; } = 1;
        public static int RouteId { get; private set; } = 1;
        public static EnemyRouteData RouteData { get; private set; }

        public static int ChainLength { get; private set; } = 10;
        public static int SegmentBaseHp { get; private set; } = 20;
        public static float SegmentLevelGrowth { get; private set; } = 0.10f;
        public static float SegmentPositionGrowth { get; private set; } = 0.15f;
        public static float NormalPayloadHpMultiplier { get; private set; } = 1f;
        public static float ChestPayloadHpMultiplier { get; private set; } = 1.35f;
        public static float HeavyPayloadHpMultiplier { get; private set; } = 1.6f;
        public static int CaptainHp { get; private set; } = 15;
        public static float ChainMoveSpeed { get; private set; } = 1.8f;
        public static float SpawnDelay { get; private set; } = 3f;
        public static float SegmentSpacing { get; private set; } = 0.85f;
        public static CaravanSegmentRuntimeData[] SegmentDefinitions { get; private set; }

        public static float PlayerMoveSpeed { get; private set; } = 8f;
        public static float PlayerFireRate { get; private set; } = 2f;
        public static int WeaponPower { get; private set; } = 1;
        public static float NormalBuffPercent { get; private set; } = 0f;
        public static float CritBuffPercent { get; private set; } = 0f;
        public static float UpgradePercent { get; private set; } = 0f;
        public static int PurchasedBonus { get; private set; } = 0;
        public static float CriticalChance { get; private set; } = 0.10f;
        public static float CriticalMultiplier { get; private set; } = 2f;

        public static void Apply(LevelConfig config)
        {
            if (config == null)
            {
                ApplyDefaults();
                return;
            }

            LevelNumber = config.LevelNumber;
            RouteId = config.RouteId;
            RouteData = config.RouteData;

            ChainLength = config.CaravanChainLength;
            SegmentBaseHp = config.SegmentBaseHp;
            SegmentLevelGrowth = config.SegmentLevelGrowth;
            SegmentPositionGrowth = config.SegmentPositionGrowth;
            NormalPayloadHpMultiplier = config.NormalPayloadHpMultiplier;
            ChestPayloadHpMultiplier = config.ChestPayloadHpMultiplier;
            HeavyPayloadHpMultiplier = config.HeavyPayloadHpMultiplier;
            CaptainHp = config.CaptainHp;
            ChainMoveSpeed = config.CaravanMovementSpeed;
            SpawnDelay = config.SpawnDelay;
            SegmentSpacing = config.SegmentSpacing;
            SegmentDefinitions = config.SegmentDefinitions;

            PlayerMoveSpeed = config.PlayerMoveSpeed;
            PlayerFireRate = config.PlayerFireRate;
            WeaponPower = config.WeaponPower;
            NormalBuffPercent = config.NormalBuffPercent;
            CritBuffPercent = config.CritBuffPercent;
            UpgradePercent = config.UpgradePercent;
            PurchasedBonus = config.PurchasedBonus;
            CriticalChance = config.CriticalChance;
            CriticalMultiplier = config.CriticalMultiplier;
        }

        private static void ApplyDefaults()
        {
            LevelNumber = 1;
            RouteId = 1;
            RouteData = null;
            ChainLength = 10;
            SegmentBaseHp = 20;
            SegmentLevelGrowth = 0.10f;
            SegmentPositionGrowth = 0.15f;
            NormalPayloadHpMultiplier = 1f;
            ChestPayloadHpMultiplier = 1.35f;
            HeavyPayloadHpMultiplier = 1.6f;
            CaptainHp = 15;
            ChainMoveSpeed = 1.8f;
            SpawnDelay = 3f;
            SegmentSpacing = 0.85f;
            SegmentDefinitions = null;
            PlayerMoveSpeed = 8f;
            PlayerFireRate = 2f;
            WeaponPower = 1;
            NormalBuffPercent = 0f;
            CritBuffPercent = 0f;
            UpgradePercent = 0f;
            PurchasedBonus = 0;
            CriticalChance = 0.10f;
            CriticalMultiplier = 2f;
        }
    }
}
