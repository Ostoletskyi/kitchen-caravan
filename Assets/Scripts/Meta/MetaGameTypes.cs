using System;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenCaravan.Meta
{
    public enum DifficultyTier
    {
        Normal = 0,
        Hard = 1,
        Insane = 2
    }

    public enum ResourceType
    {
        Coins = 0,
        UpgradeChips = 1,
        Mana = 2
    }

    public enum AbilityCardRarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legendary = 3
    }

    public enum DroneStatType
    {
        FireFrequency = 0,
        WeaponDamage = 1,
        CriticalPower = 2
    }

    public enum ChestTier
    {
        Wooden = 0,
        Iron = 1,
        Gold = 2,
        Arcane = 3
    }

    public enum UIScreenId
    {
        Splash = 0,
        MainMenu = 1,
        MapSelect = 2,
        Gameplay = 3,
        Victory = 4,
        Defeat = 5,
        Settings = 6,
        DeveloperTuning = 7
    }

    public enum SkinBonusType
    {
        None = 0,
        FireIntervalMultiplier = 1,
        DamageMultiplier = 2,
        CritDamageMultiplier = 3
    }

    [Serializable]
    public class MetaProgressionData
    {
        public int highestUnlockedNormalMapIndex = 1;
        public int highestCompletedNormalMapIndex;
        public int highestCompletedHardMapIndex;
        public int highestCompletedInsaneMapIndex;
        public List<MapCompletionRecordData> mapCompletions = new List<MapCompletionRecordData>();

        public static MetaProgressionData CreateDefault()
        {
            return new MetaProgressionData();
        }

        public void EnsureDefaults()
        {
            if (highestUnlockedNormalMapIndex < 1)
            {
                highestUnlockedNormalMapIndex = 1;
            }

            mapCompletions ??= new List<MapCompletionRecordData>();
        }
    }

    [Serializable]
    public class EconomyStateData
    {
        public int coins;
        public int upgradeChips;
        public int mana;

        public static EconomyStateData CreateDefault()
        {
            return new EconomyStateData();
        }

        public void EnsureDefaults()
        {
            coins = Mathf.Max(0, coins);
            upgradeChips = Mathf.Max(0, upgradeChips);
            mana = Mathf.Max(0, mana);
        }
    }

    [Serializable]
    public class EnergyStateData
    {
        public int currentEnergy = 35;
        public long lastEnergyTickUnix;

        public static EnergyStateData CreateDefault()
        {
            return new EnergyStateData
            {
                currentEnergy = 35,
                lastEnergyTickUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }

        public void EnsureDefaults()
        {
            if (currentEnergy < 0)
            {
                currentEnergy = 35;
            }

            if (lastEnergyTickUnix <= 0)
            {
                lastEnergyTickUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
        }
    }

    [Serializable]
    public class DroneUpgradeProgressData
    {
        public int fireFrequencyLevel;
        public int weaponDamageLevel;
        public int criticalPowerLevel;

        public static DroneUpgradeProgressData CreateDefault()
        {
            return new DroneUpgradeProgressData();
        }

        public void EnsureDefaults()
        {
            fireFrequencyLevel = Mathf.Max(0, fireFrequencyLevel);
            weaponDamageLevel = Mathf.Max(0, weaponDamageLevel);
            criticalPowerLevel = Mathf.Max(0, criticalPowerLevel);
        }
    }

    [Serializable]
    public class MapCompletionRecordData
    {
        public string mapId;
        public bool normalCompleted;
        public bool hardCompleted;
        public bool insaneCompleted;
    }

    [Serializable]
    public class OwnedAbilityCardData
    {
        public string cardId;
        public int copies;
        public int level;
    }

    [Serializable]
    public class OwnedSkinData
    {
        public string skinId;
        public bool unlocked;
        public bool equipped;
    }

    [Serializable]
    public struct ResourceAmount
    {
        public ResourceType resourceType;
        public int amount;
    }

    [Serializable]
    public struct ChestRewardData
    {
        public ChestTier tier;
        public int chestCount;
        public float contentsMultiplier;
        public float cardDropChance;
    }

    [Serializable]
    public struct DroneCombatStats
    {
        public float fireIntervalSeconds;
        public int damagePerShot;
        public int critEveryNthShot;
        public float critDamageMultiplier;
    }

    [Serializable]
    public struct RunRewardResult
    {
        public bool victory;
        public DifficultyTier difficultyTier;
        public int coins;
        public int mana;
        public ChestRewardData chestReward;
        public int energyCost;
        public int energyRefund;
        public int bonusEnergy;
    }

    [Serializable]
    public struct SkinGameplayBonus
    {
        public SkinBonusType bonusType;
        public float value;
    }
}
