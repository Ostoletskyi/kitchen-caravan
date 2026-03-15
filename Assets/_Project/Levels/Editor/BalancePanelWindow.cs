using System.IO;
using UnityEditor;
using UnityEngine;

namespace KitchenCaravan.VerticalSlice.Editor
{
    public class BalancePanelWindow : EditorWindow
    {
        private const string DefaultConfigFolder = "Assets/_Project/Levels/Configs";

        private LevelConfig _selectedConfig;
        private SerializedObject _serializedConfig;
        private Vector2 _scroll;

        [MenuItem("Tools/KitchenCaravan/Balance Panel")]
        public static void Open()
        {
            var window = GetWindow<BalancePanelWindow>("Balance Panel");
            window.minSize = new Vector2(420f, 500f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Kitchen Caravan Level Balance", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                var nextConfig = (LevelConfig)EditorGUILayout.ObjectField("Level Config", _selectedConfig, typeof(LevelConfig), false);
                if (nextConfig != _selectedConfig)
                {
                    SetSelectedConfig(nextConfig);
                }

                if (GUILayout.Button("New", GUILayout.Width(70f)))
                {
                    CreateNewConfigAsset();
                }
            }

            if (_selectedConfig == null)
            {
                EditorGUILayout.HelpBox("Select a LevelConfig asset or create a new one.", MessageType.Info);
                return;
            }

            if (_serializedConfig == null)
            {
                _serializedConfig = new SerializedObject(_selectedConfig);
            }

            _serializedConfig.Update();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawConfigFields();
            EditorGUILayout.EndScrollView();

            if (_serializedConfig.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_selectedConfig);
            }
        }

        private void DrawConfigFields()
        {
            DrawProperty("_levelNumber");
            DrawProperty("_routeId");
            DrawProperty("_routeData");
            EditorGUILayout.Space(4f);

            EditorGUILayout.LabelField("Caravan", EditorStyles.boldLabel);
            DrawProperty("_caravanChainLength");
            DrawProperty("_segmentBaseHp");
            DrawProperty("_segmentLevelGrowth");
            DrawProperty("_segmentPositionGrowth");
            DrawProperty("_normalPayloadHpMultiplier");
            DrawProperty("_chestPayloadHpMultiplier");
            DrawProperty("_heavyPayloadHpMultiplier");
            DrawProperty("_captainHp");
            DrawProperty("_caravanMovementSpeed");
            DrawProperty("_spawnDelay");
            DrawProperty("_segmentSpacing");
            DrawProperty("_segmentDefinitions");
            EditorGUILayout.Space(4f);

            EditorGUILayout.LabelField("Player", EditorStyles.boldLabel);
            DrawProperty("_playerMoveSpeed");
            DrawProperty("_playerFireRate");
            EditorGUILayout.Space(4f);

            EditorGUILayout.LabelField("Damage", EditorStyles.boldLabel);
            DrawProperty("_weaponPower");
            DrawProperty("_normalBuffPercent");
            DrawProperty("_critBuffPercent");
            DrawProperty("_upgradePercent");
            DrawProperty("_purchasedBonus");
            DrawProperty("_criticalChance");
            DrawProperty("_criticalMultiplier");
        }

        private void DrawProperty(string propertyName)
        {
            SerializedProperty property = _serializedConfig.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, true);
            }
        }

        private void SetSelectedConfig(LevelConfig config)
        {
            _selectedConfig = config;
            _serializedConfig = config != null ? new SerializedObject(config) : null;
        }

        private void CreateNewConfigAsset()
        {
            EnsureFolder(DefaultConfigFolder);
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Level Config",
                "LevelConfig_New",
                "asset",
                "Choose location for the new LevelConfig asset.",
                DefaultConfigFolder);

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var asset = ScriptableObject.CreateInstance<LevelConfig>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = asset;
            SetSelectedConfig(asset);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
            string name = Path.GetFileName(folderPath);

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
