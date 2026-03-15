namespace KitchenCaravan.VerticalSlice
{
    public enum CaravanPayloadType
    {
        NormalPayload = 0,
        ChestPayload = 1,
        HeavyPayload = 2,
        Bread = 3,
        Cheese = 4,
        Tomato = 5,
        Cucumber = 6,
        Bacon = 7,
        Meat = 8,
        Egg = 9
    }

    [System.Serializable]
    public struct CaravanSegmentRuntimeData
    {
        public CaravanPayloadType payloadType;
        public bool isChestCarrier;
    }

    public struct CaravanRuntimeSettings
    {
        public int levelNumber;
        public int chainLength;
        public int segmentBaseHp;
        public float segmentLevelGrowth;
        public float segmentPositionGrowth;
        public float normalPayloadHpMultiplier;
        public float chestPayloadHpMultiplier;
        public float heavyPayloadHpMultiplier;
        public int captainHp;
        public float moveSpeed;
        public float segmentSpacing;
        public EnemyRouteData routeData;
        public CaravanSegmentRuntimeData[] segmentData;
    }
}
