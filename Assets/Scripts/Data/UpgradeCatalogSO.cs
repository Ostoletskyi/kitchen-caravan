using UnityEngine;

namespace KitchenCaravan.Data
{
    [CreateAssetMenu(menuName = "KitchenCaravan/Catalog/Upgrade Catalog", fileName = "UpgradeCatalog")]
    public class UpgradeCatalogSO : ScriptableObject
    {
        public UpgradeEntry[] upgrades;
    }

    [System.Serializable]
    public class UpgradeEntry
    {
        public string upgradeId;
        public string displayName;
        public Sprite icon;
    }
}
