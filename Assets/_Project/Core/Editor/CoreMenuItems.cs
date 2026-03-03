using KitchenCaravan.Core;
using UnityEditor;

namespace KitchenCaravan.Core.Editor
{
    internal static class CoreMenuItems
    {
        [MenuItem("Tools/KitchenCaravan/Ensure Bootstrapper In Scene")]
        private static void EnsureBootstrapperInScene()
        {
            var bootstrapper = AppBootstrapper.EnsureInScene();
            Selection.activeGameObject = bootstrapper.gameObject;
        }
    }
}
