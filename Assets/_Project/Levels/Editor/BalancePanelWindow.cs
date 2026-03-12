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
            DrawProperty("_segmentHpIncrement");
            DrawProperty("_captainHp");
            DrawProperty("_caravanMovementSpeed");
            DrawProperty("_spawnDelay");
            DrawProperty("_segmentSpacing");
            DrawProperty("_followLerpSpeed");
            DrawProperty("_trailStep");
            DrawProperty("_swayAmplitude");
            DrawProperty("_swayFrequency");
            EditorGUILayout.Space(4f);

            EditorGUILayout.LabelField("Player", EditorStyles.boldLabel);
            DrawProperty("_playerMoveSpeed");
            DrawProperty("_playerFireRate");
            DrawProperty("_bulletDamage");
        }

        private void DrawProperty(string propertyName)
        {
            SerializedProperty property = _serializedConfig.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property);
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
