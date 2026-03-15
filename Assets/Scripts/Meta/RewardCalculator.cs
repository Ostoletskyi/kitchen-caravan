using KitchenCaravan.Data;
using UnityEngine;

namespace KitchenCaravan.Meta
{
    public static class RewardCalculator
    {
        public static RunRewardResult Evaluate(
            MapConfigSO mapConfig,
            DifficultyTier tier,
            bool victory,
            RewardTableSO rewardTable,
            MetaProgressionConfigSO progressionConfig)
        {
            int mapIndex = mapConfig != null ? Mathf.Max(1, mapConfig.progressionIndex) : 1;
            var tierSettings = mapConfig != null ? mapConfig.GetTierSettings(tier) : MapDifficultyTierSettings.Create(tier);
            var modifier = rewardTable != null ? rewardTable.GetModifier(tier) : DifficultyRewardModifier.Create(tier);

            int baseCoins = rewardTable != null ? rewardTable.baseCoins + (mapIndex - 1) * rewardTable.coinsPerMapStep : 50 + (mapIndex - 1) * 18;
            int baseMana = rewardTable != null ? rewardTable.baseMana + (mapIndex - 1) * rewardTable.manaPerMapStep : 12 + (mapIndex - 1) * 5;
            float outcomeMultiplier = victory || rewardTable == null ? 1f : rewardTable.defeatCurrencyMultiplier;
            int coins = Mathf.Max(0, Mathf.RoundToInt(baseCoins * modifier.coinMultiplier * tierSettings.rewardMultiplier * outcomeMultiplier));
            int mana = Mathf.Max(0, Mathf.RoundToInt(baseMana * modifier.manaMultiplier * tierSettings.rewardMultiplier * outcomeMultiplier));

            float chestContentsMultiplier = victory ? 1f : progressionConfig != null ? progressionConfig.defeatChestContentsMultiplier : 0.5f;
            float baseCardChance = tierSettings.baseCardDropChance * modifier.cardChanceMultiplier;
            int bonusEnergy = 0;
            if (victory && Random.value <= tierSettings.bonusEnergyChance)
            {
                bonusEnergy = 1;
            }

            return new RunRewardResult
            {
                victory = victory,
                difficultyTier = tier,
                coins = coins,
                mana = mana,
                chestReward = new ChestRewardData
                {
                    tier = tierSettings.guaranteedChestTier,
                    chestCount = 1,
                    contentsMultiplier = chestContentsMultiplier,
                    cardDropChance = Mathf.Clamp01(baseCardChance * chestContentsMultiplier),
                    reducedForDefeat = !victory
                },
                grantedCards = System.Array.Empty<CardRewardData>(),
                energyCost = progressionConfig != null ? progressionConfig.energyCostPerRun : 5,
                energyRefund = progressionConfig != null ? (victory ? progressionConfig.victoryEnergyRefund : progressionConfig.defeatEnergyRefund) : (victory ? 3 : 1),
                bonusEnergy = bonusEnergy
            };
        }
    }
}
