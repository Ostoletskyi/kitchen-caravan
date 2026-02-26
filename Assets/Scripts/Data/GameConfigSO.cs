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
    }
}
