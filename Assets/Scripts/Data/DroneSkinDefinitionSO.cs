using KitchenCaravan.Meta;
using UnityEngine;

namespace KitchenCaravan.Data
{
    [CreateAssetMenu(menuName = "KitchenCaravan/Catalog/Drone Skin Definition", fileName = "DroneSkinDefinition")]
    public class DroneSkinDefinitionSO : ScriptableObject
    {
        public string skinId = "skin_default";
        public string displayName = "Default Skin";
        public Sprite previewSprite;
        public Material materialOverride;
        public bool unlockedByDefault;
        public SkinGameplayBonus[] gameplayBonuses;
    }
}
