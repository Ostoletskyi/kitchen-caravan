using UnityEngine;

namespace KitchenCaravan.Data
{
    [CreateAssetMenu(menuName = "KitchenCaravan/Config/Loot Table", fileName = "LootTable")]
    public class LootTableSO : ScriptableObject
    {
        public int totalSegments = 120;
        public SegmentRule defaultRule;
        public CadenceRule[] cadenceRules;
    }

    [System.Serializable]
    public struct SegmentRule
    {
        public string ruleId;
        public LootType lootType;
        public SegmentRole role;
        public SegmentTier tier;
        public int hp;
    }

    [System.Serializable]
    public struct CadenceRule
    {
        public string ruleId;
        public int everyN;
        public int priority;
        public LootType lootType;
        public SegmentRole role;
        public SegmentTier tier;
        public int hp;
    }

    public enum LootType
    {
        None,
        Enemy,
        Upgrade,
        Event,
        Chest,
        Candy,
        Special
    }

    public enum SegmentRole
    {
        None,
        Combat,
        Reward,
        Event,
        Boss
    }

    public enum SegmentTier
    {
        Common,
        Rare,
        Epic
    }
}
