using System;
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
        private readonly AbilityCardRewardService _abilityCardRewardService;
        private readonly DroneSkinDefinitionSO[] _skinDefinitions;
        private readonly TimerSystem _timerSystem;

        public MetaProgressionService(
            SaveModel saveModel,
            MetaProgressionConfigSO progressionConfig,
            RewardTableSO rewardTable,
            DroneUpgradeConfigSO droneUpgradeConfig,
            AbilityCardDefinitionSO[] abilityCards = null,
            AbilityCardDropTableSO abilityCardDropTable = null,
            DroneSkinDefinitionSO[] skinDefinitions = null,
            TimerSystem timerSystem = null)
        {
            _saveModel = saveModel ?? SaveModel.CreateNew();
            _saveModel.EnsureDefaults();
            _progressionConfig = progressionConfig;
            _rewardTable = rewardTable;
            _droneUpgradeConfig = droneUpgradeConfig;
            _abilityCardRewardService = new AbilityCardRewardService(abilityCards, abilityCardDropTable);
            _skinDefinitions = skinDefinitions ?? Array.Empty<DroneSkinDefinitionSO>();
            _timerSystem = timerSystem ?? new TimerSystem();
            EnsureOwnedDefaultSkins();
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

        public bool HasCompletedMap(MapConfigSO mapConfig, DifficultyTier tier)
        {
            if (mapConfig == null || string.IsNullOrWhiteSpace(mapConfig.mapId))
            {
                return false;
            }

            var completions = _saveModel.progression.mapCompletions;
            for (int i = 0; i < completions.Count; i++)
            {
                if (completions[i] == null || completions[i].mapId != mapConfig.mapId)
                {
                    continue;
                }

                switch (tier)
                {
                    case DifficultyTier.Hard:
                        return completions[i].hardCompleted;
                    case DifficultyTier.Insane:
                        return completions[i].insaneCompleted;
                    default:
                        return completions[i].normalCompleted;
                }
            }

            return false;
        }

        public bool CanStartRun(out int currentEnergy, out int requiredEnergy)
        {
            SyncEnergy();
            requiredEnergy = GetEnergyCostPerRun();
            currentEnergy = _saveModel.energy.currentEnergy;
            return currentEnergy >= requiredEnergy;
        }

        public int GetAvailableRunCountEstimate(bool assumeVictory)
        {
            SyncEnergy();
            int netCost = Mathf.Max(1, GetEnergyCostPerRun() - (assumeVictory ? GetVictoryEnergyRefund() : GetDefeatEnergyRefund()));
            return _saveModel.energy.currentEnergy / netCost;
        }

        public long GetNextEnergyRestoreUnix()
        {
            SyncEnergy();
            if (_saveModel.energy.currentEnergy >= GetMaximumEnergy())
            {
                return 0;
            }

            return _saveModel.energy.lastEnergyTickUnix + GetEnergyRegenSeconds();
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
            reward.grantedCards = _abilityCardRewardService.Roll(reward.chestReward);

            _saveModel.economy.coins += reward.coins;
            _saveModel.economy.mana += reward.mana;
            _saveModel.energy.currentEnergy = Mathf.Clamp(_saveModel.energy.currentEnergy + reward.energyRefund + reward.bonusEnergy, 0, GetMaximumEnergy());
            _saveModel.energy.lastEnergyTickUnix = _timerSystem.GetUnixTimeNow();

            GrantCards(reward.grantedCards);

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

        public bool TryUpgradeAbilityCard(AbilityCardDefinitionSO definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.cardId))
            {
                return false;
            }

            OwnedAbilityCardData card = GetOrCreateOwnedCard(definition.cardId);
            int currentLevel = Mathf.Max(1, card.level);
            if (currentLevel >= Mathf.Max(1, definition.maxLevel))
            {
                return false;
            }

            int levelIndex = currentLevel - 1;
            int requiredCopies = GetArrayValue(definition.copiesRequiredPerLevel, levelIndex, 1);
            int requiredChips = GetArrayValue(definition.chipCostPerLevel, levelIndex, 1);
            if (card.copies < requiredCopies || _saveModel.economy.upgradeChips < requiredChips)
            {
                return false;
            }

            card.copies -= requiredCopies;
            _saveModel.economy.upgradeChips -= requiredChips;
            card.level++;
            return true;
        }

        public bool TryEquipSkin(DroneSkinDefinitionSO definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.skinId))
            {
                return false;
            }

            OwnedSkinData ownedSkin = GetOwnedSkin(definition.skinId);
            if (ownedSkin == null || !ownedSkin.unlocked)
            {
                return false;
            }

            for (int i = 0; i < _saveModel.ownedSkins.Count; i++)
            {
                if (_saveModel.ownedSkins[i] != null)
                {
                    _saveModel.ownedSkins[i].equipped = _saveModel.ownedSkins[i].skinId == definition.skinId;
                }
            }

            _saveModel.equippedSkinId = definition.skinId;
            return true;
        }

        public SkinGameplayBonus[] GetEquippedSkinBonuses()
        {
            string equippedSkinId = _saveModel.equippedSkinId;
            for (int i = 0; i < _skinDefinitions.Length; i++)
            {
                if (_skinDefinitions[i] != null && _skinDefinitions[i].skinId == equippedSkinId)
                {
                    return _skinDefinitions[i].gameplayBonuses ?? Array.Empty<SkinGameplayBonus>();
                }
            }

            return Array.Empty<SkinGameplayBonus>();
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

        private int GetVictoryEnergyRefund()
        {
            return _progressionConfig != null ? Mathf.Max(0, _progressionConfig.victoryEnergyRefund) : 3;
        }

        private int GetDefeatEnergyRefund()
        {
            return _progressionConfig != null ? Mathf.Max(0, _progressionConfig.defeatEnergyRefund) : 1;
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

        private void GrantCards(CardRewardData[] rewards)
        {
            if (rewards == null)
            {
                return;
            }

            for (int i = 0; i < rewards.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(rewards[i].cardId) || rewards[i].copies <= 0)
                {
                    continue;
                }

                OwnedAbilityCardData card = GetOrCreateOwnedCard(rewards[i].cardId);
                card.copies += rewards[i].copies;
            }
        }

        private OwnedAbilityCardData GetOrCreateOwnedCard(string cardId)
        {
            for (int i = 0; i < _saveModel.abilityCards.Count; i++)
            {
                if (_saveModel.abilityCards[i] != null && _saveModel.abilityCards[i].cardId == cardId)
                {
                    return _saveModel.abilityCards[i];
                }
            }

            var card = new OwnedAbilityCardData
            {
                cardId = cardId,
                level = 1,
                copies = 0
            };
            _saveModel.abilityCards.Add(card);
            return card;
        }

        private OwnedSkinData GetOwnedSkin(string skinId)
        {
            for (int i = 0; i < _saveModel.ownedSkins.Count; i++)
            {
                if (_saveModel.ownedSkins[i] != null && _saveModel.ownedSkins[i].skinId == skinId)
                {
                    return _saveModel.ownedSkins[i];
                }
            }

            return null;
        }

        private void EnsureOwnedDefaultSkins()
        {
            bool equippedFound = false;
            for (int i = 0; i < _skinDefinitions.Length; i++)
            {
                var definition = _skinDefinitions[i];
                if (definition == null || string.IsNullOrWhiteSpace(definition.skinId))
                {
                    continue;
                }

                OwnedSkinData owned = GetOwnedSkin(definition.skinId);
                if (owned == null)
                {
                    owned = new OwnedSkinData
                    {
                        skinId = definition.skinId,
                        unlocked = definition.unlockedByDefault,
                        equipped = false
                    };
                    _saveModel.ownedSkins.Add(owned);
                }
                else if (definition.unlockedByDefault)
                {
                    owned.unlocked = true;
                }

                if (owned.equipped || _saveModel.equippedSkinId == definition.skinId)
                {
                    equippedFound = true;
                    _saveModel.equippedSkinId = definition.skinId;
                    owned.equipped = true;
                }
            }

            if (!equippedFound)
            {
                for (int i = 0; i < _saveModel.ownedSkins.Count; i++)
                {
                    if (_saveModel.ownedSkins[i] != null && _saveModel.ownedSkins[i].unlocked)
                    {
                        _saveModel.ownedSkins[i].equipped = true;
                        _saveModel.equippedSkinId = _saveModel.ownedSkins[i].skinId;
                        break;
                    }
                }
            }
        }

        private static int GetArrayValue(int[] values, int index, int fallback)
        {
            if (values == null || values.Length == 0)
            {
                return fallback;
            }

            return values[Mathf.Clamp(index, 0, values.Length - 1)];
        }
    }
}
