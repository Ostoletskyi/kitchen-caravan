using UnityEngine;

namespace KitchenCaravan.Data
{
    [CreateAssetMenu(menuName = "KitchenCaravan/Config/Game Config", fileName = "GameConfig")]
    public class GameConfigSO : ScriptableObject
    {
        public ChapterConfigSO startingChapter;
        public LootTableSO defaultLootTable;
        public EnemyCatalogSO enemyCatalog;
        public UpgradeCatalogSO upgradeCatalog;
        public MetaProgressionConfigSO metaProgressionConfig;
        public RewardTableSO rewardTable;
        public DroneUpgradeConfigSO droneUpgradeConfig;
        public AbilityCardDropTableSO abilityCardDropTable;
        public UIScreenFlowConfigSO uiScreenFlow;
        public MapConfigSO[] maps;
        public AbilityCardDefinitionSO[] abilityCards;
        public DroneSkinDefinitionSO[] droneSkins;
    }
}
