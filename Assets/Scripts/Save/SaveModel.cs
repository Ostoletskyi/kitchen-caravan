using System;
using System.Collections.Generic;
using KitchenCaravan.Meta;

namespace KitchenCaravan.Save
{
    [Serializable]
    public class SaveModel
    {
        public const int Version = 2;

        public int version = Version;
        public string playerId;
        public int highScore;
        public long lastSaveUnix;
        public MetaProgressionData progression = MetaProgressionData.CreateDefault();
        public EconomyStateData economy = EconomyStateData.CreateDefault();
        public EnergyStateData energy = EnergyStateData.CreateDefault();
        public DroneUpgradeProgressData droneUpgrades = DroneUpgradeProgressData.CreateDefault();
        public List<OwnedAbilityCardData> abilityCards = new List<OwnedAbilityCardData>();
        public List<OwnedSkinData> ownedSkins = new List<OwnedSkinData>();

        public static SaveModel CreateNew()
        {
            var model = new SaveModel
            {
                playerId = Guid.NewGuid().ToString("N")
            };

            model.EnsureDefaults();
            return model;
        }

        public void EnsureDefaults()
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                playerId = Guid.NewGuid().ToString("N");
            }

            progression ??= MetaProgressionData.CreateDefault();
            progression.EnsureDefaults();

            economy ??= EconomyStateData.CreateDefault();
            economy.EnsureDefaults();

            energy ??= EnergyStateData.CreateDefault();
            energy.EnsureDefaults();

            droneUpgrades ??= DroneUpgradeProgressData.CreateDefault();
            droneUpgrades.EnsureDefaults();

            abilityCards ??= new List<OwnedAbilityCardData>();
            ownedSkins ??= new List<OwnedSkinData>();
        }
    }
}
