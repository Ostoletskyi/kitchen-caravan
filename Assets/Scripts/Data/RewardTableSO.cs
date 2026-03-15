using KitchenCaravan.Meta;
using UnityEngine;

namespace KitchenCaravan.Data
{
    [CreateAssetMenu(menuName = "KitchenCaravan/Config/Reward Table", fileName = "RewardTable")]
    public class RewardTableSO : ScriptableObject
    {
        [Header("Base Curve")]
        public int baseCoins = 50;
        public int baseMana = 12;
        public int coinsPerMapStep = 18;
        public int manaPerMapStep = 5;

        [Header("Outcome Multipliers")]
        [Range(0f, 1f)] public float defeatCurrencyMultiplier = 0.55f;

        [Header("Difficulty Modifiers")]
        public DifficultyRewardModifier normal = DifficultyRewardModifier.Create(DifficultyTier.Normal);
        public DifficultyRewardModifier hard = DifficultyRewardModifier.Create(DifficultyTier.Hard);
        public DifficultyRewardModifier insane = DifficultyRewardModifier.Create(DifficultyTier.Insane);

        public DifficultyRewardModifier GetModifier(DifficultyTier tier)
        {
            switch (tier)
            {
                case DifficultyTier.Hard:
                    return hard;
                case DifficultyTier.Insane:
                    return insane;
                default:
                    return normal;
            }
        }
    }

    [System.Serializable]
    public class DifficultyRewardModifier
    {
        public DifficultyTier difficultyTier;
        public float coinMultiplier = 1f;
        public float manaMultiplier = 1f;
        public float cardChanceMultiplier = 1f;

        public static DifficultyRewardModifier Create(DifficultyTier tier)
        {
            return new DifficultyRewardModifier
            {
                difficultyTier = tier,
                coinMultiplier = tier == DifficultyTier.Normal ? 1f : tier == DifficultyTier.Hard ? 1.55f : 2.35f,
                manaMultiplier = tier == DifficultyTier.Normal ? 1f : tier == DifficultyTier.Hard ? 1.45f : 2.1f,
                cardChanceMultiplier = tier == DifficultyTier.Normal ? 1f : tier == DifficultyTier.Hard ? 1.5f : 2.2f
            };
        }
    }
}
