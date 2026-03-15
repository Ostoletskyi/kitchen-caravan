using KitchenCaravan.Meta;
using UnityEngine;

namespace KitchenCaravan.Data
{
    [CreateAssetMenu(menuName = "KitchenCaravan/Config/Ability Card Drop Table", fileName = "AbilityCardDropTable")]
    public class AbilityCardDropTableSO : ScriptableObject
    {
        public ChestTierCardDropSettings wooden = ChestTierCardDropSettings.Create(ChestTier.Wooden);
        public ChestTierCardDropSettings iron = ChestTierCardDropSettings.Create(ChestTier.Iron);
        public ChestTierCardDropSettings gold = ChestTierCardDropSettings.Create(ChestTier.Gold);
        public ChestTierCardDropSettings arcane = ChestTierCardDropSettings.Create(ChestTier.Arcane);

        public ChestTierCardDropSettings GetSettings(ChestTier chestTier)
        {
            switch (chestTier)
            {
                case ChestTier.Iron:
                    return iron;
                case ChestTier.Gold:
                    return gold;
                case ChestTier.Arcane:
                    return arcane;
                default:
                    return wooden;
            }
        }
    }

    [System.Serializable]
    public class ChestTierCardDropSettings
    {
        public ChestTier chestTier;
        public int copiesGranted = 1;
        public int commonWeight = 75;
        public int rareWeight = 20;
        public int epicWeight = 4;
        public int legendaryWeight = 1;

        public static ChestTierCardDropSettings Create(ChestTier chestTier)
        {
            switch (chestTier)
            {
                case ChestTier.Iron:
                    return new ChestTierCardDropSettings { chestTier = chestTier, copiesGranted = 1, commonWeight = 60, rareWeight = 28, epicWeight = 10, legendaryWeight = 2 };
                case ChestTier.Gold:
                    return new ChestTierCardDropSettings { chestTier = chestTier, copiesGranted = 1, commonWeight = 42, rareWeight = 34, epicWeight = 18, legendaryWeight = 6 };
                case ChestTier.Arcane:
                    return new ChestTierCardDropSettings { chestTier = chestTier, copiesGranted = 2, commonWeight = 20, rareWeight = 38, epicWeight = 28, legendaryWeight = 14 };
                default:
                    return new ChestTierCardDropSettings { chestTier = chestTier, copiesGranted = 1, commonWeight = 75, rareWeight = 20, epicWeight = 4, legendaryWeight = 1 };
            }
        }

        public int GetWeight(AbilityCardRarity rarity)
        {
            switch (rarity)
            {
                case AbilityCardRarity.Rare:
                    return Mathf.Max(0, rareWeight);
                case AbilityCardRarity.Epic:
                    return Mathf.Max(0, epicWeight);
                case AbilityCardRarity.Legendary:
                    return Mathf.Max(0, legendaryWeight);
                default:
                    return Mathf.Max(0, commonWeight);
            }
        }
    }
}
