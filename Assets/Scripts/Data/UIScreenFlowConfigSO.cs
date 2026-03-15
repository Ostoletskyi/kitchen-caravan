using KitchenCaravan.Meta;
using UnityEngine;

namespace KitchenCaravan.Data
{
    [CreateAssetMenu(menuName = "KitchenCaravan/Config/UI Screen Flow", fileName = "UIScreenFlowConfig")]
    public class UIScreenFlowConfigSO : ScriptableObject
    {
        public UIScreenEntry[] screens;
    }

    [System.Serializable]
    public struct UIScreenEntry
    {
        public UIScreenId screenId;
        public string sceneName;
        public bool gameplayScene;
        public bool editorOnly;
    }
}
