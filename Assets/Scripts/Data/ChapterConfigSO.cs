using UnityEngine;

namespace KitchenCaravan.Data
{
    [CreateAssetMenu(menuName = "KitchenCaravan/Config/Chapter Config", fileName = "ChapterConfig")]
    public class ChapterConfigSO : ScriptableObject
    {
        public string chapterId;
        public int chapterIndex;
        public LootTableSO lootTable;
    }
}
