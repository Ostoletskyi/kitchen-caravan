using UnityEngine;

namespace KitchenCaravan.Data
{
    [CreateAssetMenu(menuName = "KitchenCaravan/Catalog/Enemy Catalog", fileName = "EnemyCatalog")]
    public class EnemyCatalogSO : ScriptableObject
    {
        public EnemyEntry[] enemies;
    }

    [System.Serializable]
    public class EnemyEntry
    {
        public string enemyId;
        public GameObject prefab;
        public int minLevel;
        public int maxLevel;
    }
}
