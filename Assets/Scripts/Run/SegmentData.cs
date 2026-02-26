using KitchenCaravan.Data;

namespace KitchenCaravan.Run
{
    public struct SegmentData
    {
        public int segmentIndex;
        public string ruleId;
        public LootType lootType;
        public SegmentRole role;
        public SegmentTier tier;
        public int hp;
        public int cadenceEveryN;
        public bool isDefaultRule;
    }
}
