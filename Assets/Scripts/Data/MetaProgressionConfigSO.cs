using UnityEngine;

namespace KitchenCaravan.Data
{
    [CreateAssetMenu(menuName = "KitchenCaravan/Config/Meta Progression Config", fileName = "MetaProgressionConfig")]
    public class MetaProgressionConfigSO : ScriptableObject
    {
        [Header("Map Unlocks")]
        public int hardGlobalUnlockAtNormalMap = 10;
        public int insaneGlobalUnlockAtNormalMap = 20;

        [Header("Energy")]
        public int startingEnergy = 35;
        public int maximumEnergy = 35;
        public int energyCostPerRun = 5;
        public int energyRegenerationMinutes = 30;
        public int victoryEnergyRefund = 3;
        public int defeatEnergyRefund = 1;

        [Header("Economy")]
        public int coinCostPerUpgradeChip = 100;

        [Header("Chest Tuning")]
        public float victoryChestContentsMultiplier = 1f;
        public float defeatChestContentsMultiplier = 0.5f;
    }
}
