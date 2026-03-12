using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using KitchenCaravan.Core;
using KitchenCaravan.Data;
using KitchenCaravan.Run;
using KitchenCaravan.UI;

namespace KitchenCaravan.Editor
{
    public static class DevTestSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Dev_Test.unity";
        private const string ConfigFolder = "Assets/ScriptableObjects/Configs";
        private const string CatalogFolder = "Assets/ScriptableObjects/Catalogs";
        private const string LootFolder = "Assets/ScriptableObjects/Loot";

        [MenuItem("KitchenCaravan/Dev/Create Test Scene And Assets")]
        public static void CreateSceneAndAssets()
        {
            EnsureFolder("Assets/ScriptableObjects");
            EnsureFolder(ConfigFolder);
            EnsureFolder(CatalogFolder);
            EnsureFolder(LootFolder);
            EnsureFolder("Assets/Scenes");

            EnemyCatalogSO enemyCatalog = CreateOrLoadAsset<EnemyCatalogSO>(Path.Combine(CatalogFolder, "EnemyCatalogSO_Default.asset"));
            UpgradeCatalogSO upgradeCatalog = CreateOrLoadAsset<UpgradeCatalogSO>(Path.Combine(CatalogFolder, "UpgradeCatalogSO_Default.asset"));
            LootTableSO lootTable = CreateOrLoadAsset<LootTableSO>(Path.Combine(LootFolder, "LootTableSO_AntChapter.asset"));
            GameConfigSO gameConfig = CreateOrLoadAsset<GameConfigSO>(Path.Combine(ConfigFolder, "GameConfigSO_Default.asset"));

            ConfigureEnemyCatalog(enemyCatalog);
            ConfigureUpgradeCatalog(upgradeCatalog);
            ConfigureLootTable(lootTable);
            ConfigureGameConfig(gameConfig, lootTable, enemyCatalog, upgradeCatalog);

            EditorUtility.SetDirty(enemyCatalog);
            EditorUtility.SetDirty(upgradeCatalog);
            EditorUtility.SetDirty(lootTable);
            EditorUtility.SetDirty(gameConfig);
            AssetDatabase.SaveAssets();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateSceneObjects(lootTable);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            Debug.Log("Dev test scene and assets created.");
        }

        private static void CreateSceneObjects(LootTableSO lootTable)
        {
            GameObject gameManagerObject = new GameObject("GameManager");
            gameManagerObject.AddComponent<GameManager>();

            GameObject chainControllerObject = new GameObject("ChainController");
            ChainController chainController = chainControllerObject.AddComponent<ChainController>();
            chainControllerObject.transform.position = Vector3.zero;
            AssignSerializedField(chainController, "_lootTable", lootTable);

            GameObject combatSystemObject = new GameObject("CombatSystem");
            combatSystemObject.AddComponent<CombatSystem>();

            GameObject canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject hudObject = new GameObject("DebugHUD");
            hudObject.transform.SetParent(canvasObject.transform, false);
            DebugHUD hud = hudObject.AddComponent<DebugHUD>();

            GameObject textObject = new GameObject("HUD_Text");
            textObject.transform.SetParent(hudObject.transform, false);
            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.UpperLeft;
            text.fontSize = 14;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(10f, -10f);
            rect.sizeDelta = new Vector2(600f, 400f);

            AssignSerializedField(hud, "_gameManager", gameManagerObject.GetComponent<GameManager>());
            AssignSerializedField(hud, "_chainController", chainController);
            AssignSerializedField(hud, "_combatSystem", combatSystemObject.GetComponent<CombatSystem>());
            AssignSerializedField(hud, "_uiText", text);
        }

        private static void ConfigureEnemyCatalog(EnemyCatalogSO catalog)
        {
            catalog.enemies = new EnemyEntry[1];
            catalog.enemies[0] = new EnemyEntry
            {
                enemyId = "Ant",
                prefab = null,
                minLevel = 1,
                maxLevel = 10
            };
        }

        private static void ConfigureUpgradeCatalog(UpgradeCatalogSO catalog)
        {
            catalog.upgrades = new UpgradeEntry[3];
            catalog.upgrades[0] = new UpgradeEntry { upgradeId = "Damage", displayName = "Damage" };
            catalog.upgrades[1] = new UpgradeEntry { upgradeId = "FireRate", displayName = "Fire Rate" };
            catalog.upgrades[2] = new UpgradeEntry { upgradeId = "Range", displayName = "Range" };
        }

        private static void ConfigureLootTable(LootTableSO lootTable)
        {
            lootTable.totalSegments = 120;
            lootTable.defaultRule = new SegmentRule
            {
                ruleId = "Default_Combat",
                lootType = LootType.Enemy,
                role = SegmentRole.Combat,
                tier = SegmentTier.Common,
                hp = 10
            };

            lootTable.cadenceRules = new CadenceRule[3];
            lootTable.cadenceRules[0] = new CadenceRule
            {
                ruleId = "Chest_Every_6",
                everyN = 6,
                priority = 10,
                lootType = LootType.Chest,
                role = SegmentRole.Reward,
                tier = SegmentTier.Common,
                hp = 0
            };
            lootTable.cadenceRules[1] = new CadenceRule
            {
                ruleId = "Candy_Every_12",
                everyN = 12,
                priority = 20,
                lootType = LootType.Candy,
                role = SegmentRole.Reward,
                tier = SegmentTier.Rare,
                hp = 0
            };
            lootTable.cadenceRules[2] = new CadenceRule
            {
                ruleId = "Special_Every_30",
                everyN = 30,
                priority = 30,
                lootType = LootType.Special,
                role = SegmentRole.Event,
                tier = SegmentTier.Epic,
                hp = 0
            };
        }

        private static void ConfigureGameConfig(
            GameConfigSO config,
            LootTableSO lootTable,
            EnemyCatalogSO enemyCatalog,
            UpgradeCatalogSO upgradeCatalog)
        {
            config.startingChapter = null;
            config.defaultLootTable = lootTable;
            config.enemyCatalog = enemyCatalog;
            config.upgradeCatalog = upgradeCatalog;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath);
            string name = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }

        private static T CreateOrLoadAsset<T>(string assetPath) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        private static void AssignSerializedField(Object target, string fieldName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(fieldName);
            if (property != null)
            {
                property.objectReferenceValue = value;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
