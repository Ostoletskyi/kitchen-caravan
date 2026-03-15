using System.Collections.Generic;
using KitchenCaravan.Data;
using KitchenCaravan.Save;
using UnityEngine;

namespace KitchenCaravan.Meta
{
    public sealed class MetaProgressionService
    {
        private readonly SaveModel _saveModel;
        private readonly MetaProgressionConfigSO _progressionConfig;
        private readonly RewardTableSO _rewardTable;
        private readonly DroneUpgradeConfigSO _droneUpgradeConfig;
        private readonly TimerSystem _timerSystem;

        public MetaProgressionService(
            SaveModel saveModel,
            MetaProgressionConfigSO progressionConfig,
            RewardTableSO rewardTable,
            DroneUpgradeConfigSO droneUpgradeConfig,
            TimerSystem timerSystem = null)
        {
            _saveModel = saveModel ?? SaveModel.CreateNew();
            _saveModel.EnsureDefaults();
            _progressionConfig = progressionConfig;
            _rewardTable = rewardTable;
            _droneUpgradeConfig = droneUpgradeConfig;
            _timerSystem = timerSystem ?? new TimerSystem();
            SyncEnergy();
        }

        public SaveModel SaveModel => _saveModel;

        public int CurrentEnergy
        {
            get
            {
                SyncEnergy();
                return _saveModel.energy.currentEnergy;
            }
        }

        public DroneCombatStats GetDroneCombatStats()
        {
            if (_droneUpgradeConfig == null)
            {
                return new DroneCombatStats
                {
                    fireIntervalSeconds = 1f,
                    damagePerShot = 1,
                    critEveryNthShot = 4,
                    critDamageMultiplier = 2f
                };
            }

            return _droneUpgradeConfig.Evaluate(_saveModel.droneUpgrades);
        }

        public bool IsDifficultyGloballyUnlocked(DifficultyTier tier)
        {
            switch (tier)
            {
                case DifficultyTier.Hard:
                    return _saveModel.progression.highestUnlockedNormalMapIndex >= GetHardUnlockMapIndex();
                case DifficultyTier.Insane:
                    return _saveModel.progression.highestUnlockedNormalMapIndex >= GetInsaneUnlockMapIndex();
                default:
                    return true;
            }
        }

        public bool IsMapUnlocked(MapConfigSO mapConfig, DifficultyTier tier)
        {
            if (mapConfig == null)
            {
                return false;
            }

            if (mapConfig.progressionIndex > _saveModel.progression.highestUnlockedNormalMapIndex)
            {
                return false;
            }

            return IsDifficultyGloballyUnlocked(tier);
        }

        public bool CanStartRun(out int currentEnergy, out int requiredEnergy)
        {
            SyncEnergy();
            requiredEnergy = GetEnergyCostPerRun();
            currentEnergy = _saveModel.energy.currentEnergy;
            return currentEnergy >= requiredEnergy;
        }

        public bool TryBeginRun()
        {
            SyncEnergy();
            int cost = GetEnergyCostPerRun();
            if (_saveModel.energy.currentEnergy < cost)
            {
                return false;
            }

            _saveModel.energy.currentEnergy -= cost;
            _saveModel.energy.lastEnergyTickUnix = _timerSystem.GetUnixTimeNow();
            return true;
        }

        public RunRewardResult CompleteRun(MapConfigSO mapConfig, DifficultyTier tier, bool victory)
        {
            SyncEnergy();

            RunRewardResult reward = RewardCalculator.Evaluate(mapConfig, tier, victory, _rewardTable, _progressionConfig);
            _saveModel.economy.coins += reward.coins;
            _saveModel.economy.mana += reward.mana;
            _saveModel.energy.currentEnergy = Mathf.Clamp(_saveModel.energy.currentEnergy + reward.energyRefund + reward.bonusEnergy, 0, GetMaximumEnergy());
            _saveModel.energy.lastEnergyTickUnix = _timerSystem.GetUnixTimeNow();

            if (victory)
            {
                ApplyVictoryProgression(mapConfig, tier);
            }

            return reward;
        }

        public int GetUpgradeCost(DroneStatType statType)
        {
            return _droneUpgradeConfig == null ? 0 : _droneUpgradeConfig.GetUpgradeCost(statType, _saveModel.droneUpgrades);
        }

        public bool TryUpgradeDroneStat(DroneStatType statType)
        {
            if (_droneUpgradeConfig == null)
            {
                return false;
            }

            int cost = GetUpgradeCost(statType);
            if (_saveModel.economy.mana < cost)
            {
                return false;
            }

            _saveModel.economy.mana -= cost;
            switch (statType)
            {
                case DroneStatType.WeaponDamage:
                    _saveModel.droneUpgrades.weaponDamageLevel++;
                    break;
                case DroneStatType.CriticalPower:
                    _saveModel.droneUpgrades.criticalPowerLevel++;
                    break;
                default:
                    _saveModel.droneUpgrades.fireFrequencyLevel++;
                    break;
            }

            return true;
        }

        public bool TryBuyUpgradeChips(int chipCount)
        {
            chipCount = Mathf.Max(1, chipCount);
            int costPerChip = _progressionConfig != null ? Mathf.Max(1, _progressionConfig.coinCostPerUpgradeChip) : 100;
            int totalCost = chipCount * costPerChip;
            if (_saveModel.economy.coins < totalCost)
            {
                return false;
            }

            _saveModel.economy.coins -= totalCost;
            _saveModel.economy.upgradeChips += chipCount;
            return true;
        }

        public void SyncEnergy()
        {
            var energy = _saveModel.energy;
            int maxEnergy = GetMaximumEnergy();
            int regenIntervalSeconds = GetEnergyRegenSeconds();
            if (regenIntervalSeconds <= 0 || energy.currentEnergy >= maxEnergy)
            {
                energy.currentEnergy = Mathf.Clamp(energy.currentEnergy, 0, maxEnergy);
                return;
            }

            long now = _timerSystem.GetUnixTimeNow();
            int restored = _timerSystem.GetElapsedWholeIntervals(energy.lastEnergyTickUnix, now, regenIntervalSeconds);
            if (restored <= 0)
            {
                return;
            }

            energy.currentEnergy = Mathf.Clamp(energy.currentEnergy + restored, 0, maxEnergy);
            energy.lastEnergyTickUnix = energy.currentEnergy >= maxEnergy ? now : energy.lastEnergyTickUnix + restored * regenIntervalSeconds;
        }

        private void ApplyVictoryProgression(MapConfigSO mapConfig, DifficultyTier tier)
        {
            if (mapConfig == null)
            {
                return;
            }

            var progression = _saveModel.progression;
            var record = GetOrCreateMapRecord(mapConfig.mapId);
            switch (tier)
            {
                case DifficultyTier.Hard:
                    record.hardCompleted = true;
                    progression.highestCompletedHardMapIndex = Mathf.Max(progression.highestCompletedHardMapIndex, mapConfig.progressionIndex);
                    break;
                case DifficultyTier.Insane:
                    record.insaneCompleted = true;
                    progression.highestCompletedInsaneMapIndex = Mathf.Max(progression.highestCompletedInsaneMapIndex, mapConfig.progressionIndex);
                    break;
                default:
                    record.normalCompleted = true;
                    progression.highestCompletedNormalMapIndex = Mathf.Max(progression.highestCompletedNormalMapIndex, mapConfig.progressionIndex);
                    progression.highestUnlockedNormalMapIndex = Mathf.Max(progression.highestUnlockedNormalMapIndex, mapConfig.progressionIndex + 1);
                    break;
            }
        }

        private MapCompletionRecordData GetOrCreateMapRecord(string mapId)
        {
            var completions = _saveModel.progression.mapCompletions ??= new List<MapCompletionRecordData>();
            for (int i = 0; i < completions.Count; i++)
            {
                if (completions[i].mapId == mapId)
                {
                    return completions[i];
                }
            }

            var record = new MapCompletionRecordData { mapId = mapId };
            completions.Add(record);
            return record;
        }

        private int GetMaximumEnergy()
        {
            return _progressionConfig != null ? Mathf.Max(1, _progressionConfig.maximumEnergy) : 35;
        }

        private int GetEnergyCostPerRun()
        {
            return _progressionConfig != null ? Mathf.Max(1, _progressionConfig.energyCostPerRun) : 5;
        }

        private int GetEnergyRegenSeconds()
        {
            return _progressionConfig != null ? Mathf.Max(1, _progressionConfig.energyRegenerationMinutes) * 60 : 1800;
        }

        private int GetHardUnlockMapIndex()
        {
            return _progressionConfig != null ? Mathf.Max(1, _progressionConfig.hardGlobalUnlockAtNormalMap) : 10;
        }

        private int GetInsaneUnlockMapIndex()
        {
            return _progressionConfig != null ? Mathf.Max(1, _progressionConfig.insaneGlobalUnlockAtNormalMap) : 20;
        }
    }
}
