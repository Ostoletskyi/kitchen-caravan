using KitchenCaravan.Meta;
using UnityEngine;

namespace KitchenCaravan.Data
{
    [CreateAssetMenu(menuName = "KitchenCaravan/Config/Map Config", fileName = "MapConfig")]
    public class MapConfigSO : ScriptableObject
    {
        [Header("Identity")]
        public string mapId = "map_001";
        public string displayName = "Map 1";
        public int progressionIndex = 1;
        public string gameplaySceneName = "Level_01";

        [Header("Map Select Layout")]
        public float horizontalPosition;
        public Sprite nodeIcon;
        public Sprite bannerImage;

        [Header("Difficulty Settings")]
        public MapDifficultyTierSettings normal = MapDifficultyTierSettings.Create(DifficultyTier.Normal);
        public MapDifficultyTierSettings hard = MapDifficultyTierSettings.Create(DifficultyTier.Hard);
        public MapDifficultyTierSettings insane = MapDifficultyTierSettings.Create(DifficultyTier.Insane);

        public MapDifficultyTierSettings GetTierSettings(DifficultyTier tier)
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
    public class MapDifficultyTierSettings
    {
        public DifficultyTier difficultyTier;
        public float enemyHealthMultiplier = 1f;
        public float enemyDamageMultiplier = 1f;
        public float rewardMultiplier = 1f;
        public ChestTier guaranteedChestTier = ChestTier.Wooden;
        [Range(0f, 1f)] public float baseCardDropChance = 0.04f;
        [Range(0f, 1f)] public float bonusEnergyChance = 0.02f;

        public static MapDifficultyTierSettings Create(DifficultyTier tier)
        {
            return new MapDifficultyTierSettings
            {
                difficultyTier = tier,
                enemyHealthMultiplier = tier == DifficultyTier.Normal ? 1f : tier == DifficultyTier.Hard ? 1.35f : 1.8f,
                enemyDamageMultiplier = tier == DifficultyTier.Normal ? 1f : tier == DifficultyTier.Hard ? 1.25f : 1.65f,
                rewardMultiplier = tier == DifficultyTier.Normal ? 1f : tier == DifficultyTier.Hard ? 1.5f : 2.2f,
                guaranteedChestTier = tier == DifficultyTier.Normal ? ChestTier.Wooden : tier == DifficultyTier.Hard ? ChestTier.Iron : ChestTier.Gold,
                baseCardDropChance = tier == DifficultyTier.Normal ? 0.04f : tier == DifficultyTier.Hard ? 0.09f : 0.16f,
                bonusEnergyChance = tier == DifficultyTier.Normal ? 0.03f : tier == DifficultyTier.Hard ? 0.06f : 0.1f
            };
        }
    }
}
