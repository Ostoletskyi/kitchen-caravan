namespace KitchenCaravan.VerticalSlice
{
    public enum ChestRewardType
    {
        WeaponUpgrade = 0,
        TemporaryBuff = 1,
        InGameCurrency = 2,
        RareModule = 3
    }

    public struct ChestRewardData
    {
        public ChestRewardType rewardType;
        public int amount;
    }

    public static class ChestRewardSystem
    {
        public static ChestRewardData BuildDefaultReward(CaravanSegment segment)
        {
            return new ChestRewardData
            {
                rewardType = ChestRewardType.InGameCurrency,
                amount = segment != null && segment.IsChestCarrier ? 1 : 0
            };
        }
    }
}
