using KitchenCaravan.Save;

namespace KitchenCaravan.Meta
{
    public sealed class PlayerMetaState
    {
        public PlayerMetaState(SaveModel saveModel)
        {
            SaveModel = saveModel ?? SaveModel.CreateNew();
            SaveModel.EnsureDefaults();
        }

        public SaveModel SaveModel { get; }
        public MetaProgressionData Progression => SaveModel.progression;
        public EconomyStateData Economy => SaveModel.economy;
        public EnergyStateData Energy => SaveModel.energy;
        public DroneUpgradeProgressData DroneUpgrades => SaveModel.droneUpgrades;
    }
}
