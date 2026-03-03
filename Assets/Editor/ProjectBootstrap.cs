#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using KitchenCaravan.Core;
using KitchenCaravan.Run;
using KitchenCaravan.Data;

public static class ProjectBootstrap
{
    [MenuItem("KitchenCaravan/Bootstrap/Build Minimal Scene")]
    public static void BuildMinimalScene()
    {
        // 1) Ensure folders
        EnsureFolder("Assets", "Configs");
        EnsureFolder("Assets/Configs", "SO");
        EnsureFolder("Assets", "Scenes");

        // 2) Create ScriptableObject assets (пример)
        CreateOrLoadSO<GameConfigSO>("Assets/Configs/SO/GameConfig.asset");

        // 3) Create scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 4) Create root objects
        var root = new GameObject("=== KitchenCaravan ===");

        var gmGO = new GameObject("GameManager");
        gmGO.transform.SetParent(root.transform);
        gmGO.AddComponent<GameManager>();

        var chainGO = new GameObject("ChainController");
        chainGO.transform.SetParent(root.transform);
        chainGO.AddComponent<ChainController>();

        var combatGO = new GameObject("CombatSystem");
        combatGO.transform.SetParent(root.transform);
        combatGO.AddComponent<CombatSystem>();

        // 5) Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Bootstrap.unity");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Bootstrap: scene + config created ✅");
    }

    private static void EnsureFolder(string parent, string child)
    {
        var path = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }

    private static T CreateOrLoadSO<T>(string assetPath) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        if (asset != null) return asset;

        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, assetPath);
        return asset;
    }
}
#endif
