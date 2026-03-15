using KitchenCaravan.Meta;
using UnityEngine;

namespace KitchenCaravan.Data
{
    [CreateAssetMenu(menuName = "KitchenCaravan/Catalog/Ability Card Definition", fileName = "AbilityCardDefinition")]
    public class AbilityCardDefinitionSO : ScriptableObject
    {
        public string cardId = "card_ability";
        public string displayName = "Ability Card";
        [TextArea] public string description;
        public AbilityCardRarity rarity = AbilityCardRarity.Rare;
        public Sprite icon;
        public int maxLevel = 5;
        public int[] copiesRequiredPerLevel = { 1, 2, 4, 6, 8 };
        public int[] chipCostPerLevel = { 5, 10, 15, 25, 40 };
    }
}
