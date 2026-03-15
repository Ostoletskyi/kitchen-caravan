#if UNITY_EDITOR
using System.IO;
using KitchenCaravan.Caravan;
using KitchenCaravan.Combat;
using KitchenCaravan.Core;
using KitchenCaravan.Route;
using KitchenCaravan.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using PrototypeRoutePath = KitchenCaravan.Route.RoutePath;

namespace KitchenCaravan.Editor
{
    // Builds a clean one-click Level 1 prototype scene, prefab set, asset folders, and docs.
    public static class PrototypeLevel01Builder
    {
        private const string ScenePath = "Assets/Scenes/Prototype_Level01.unity";
        private const string ConfigPath = "Assets/ScriptableObjects/Configs/PrototypeLevel01CaravanConfig.asset";
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/PlayerDrone.prefab";
        private const string CaptainPrefabPath = "Assets/Prefabs/Caravan/Captain.prefab";
        private const string SegmentPrefabPath = "Assets/Prefabs/Caravan/Segment.prefab";
        private const string ProjectilePrefabPath = "Assets/Prefabs/Weapons/ProjectileBasic.prefab";
        private const string HitNumberPrefabPath = "Assets/Prefabs/VFX/FloatingDamageNumber.prefab";
        private const string HitFlashPrefabPath = "Assets/Prefabs/VFX/TemporaryHitFlash.prefab";

        [MenuItem("KitchenCaravan/Prototype/Rebuild Level 01")]
        public static void BuildLevel01()
        {
            EnsureProjectFolders();
            EnsureDocs();

            CaravanConfig config = CreateOrLoadAsset<CaravanConfig>(ConfigPath);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            CaptainController captainPrefab = BuildCaptainPrefab();
            SegmentController segmentPrefab = BuildSegmentPrefab();
            ProjectileBasic projectilePrefab = BuildProjectilePrefab();
            BuildFloatingDamageNumberPrefab();
            BuildHitFlashPrefab();
            GameObject playerPrefab = BuildPlayerPrefab(projectilePrefab);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildSceneObjects(scene, config, playerPrefab, captainPrefab, segmentPrefab);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Kitchen Caravan Prototype Level 01 rebuilt.");
        }

        private static void BuildSceneObjects(Scene scene, CaravanConfig config, GameObject playerPrefab, CaptainController captainPrefab, SegmentController segmentPrefab)
        {
            Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.2f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.94f, 0.89f, 0.78f, 1f);
            camera.tag = "MainCamera";

            GameObject gameManagerObject = new GameObject("GameManager");
            GameManager gameManager = gameManagerObject.AddComponent<GameManager>();

            GameObject routeObject = new GameObject("RouteObject");
            KitchenCaravan.Route.RoutePath routePath = routeObject.AddComponent<KitchenCaravan.Route.RoutePath>();

            GameObject laneObject = new GameObject("BottomLane");
            laneObject.transform.position = new Vector3(0f, -4.9f, 0f);

            if (playerPrefab != null)
            {
                GameObject player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
                if (player != null)
                {
                    player.name = "PlayerDrone";
                    player.transform.position = laneObject.transform.position;
                }
            }

            GameObject caravanSpawnerObject = new GameObject("CaravanSpawner");
            CaravanSpawner spawner = caravanSpawnerObject.AddComponent<CaravanSpawner>();
            AssignObjectField(spawner, "_routePath", routePath);
            AssignObjectField(spawner, "_caravanConfig", config);
            AssignObjectField(spawner, "_gameManager", gameManager);

            GameObject caravanPrefabRoot = new GameObject("CaravanRuntimePrefab");
            CaravanController caravanController = caravanPrefabRoot.AddComponent<CaravanController>();
            AssignObjectField(caravanController, "_routePath", routePath);
            AssignObjectField(caravanController, "_config", config);
            AssignObjectField(caravanController, "_captainPrefab", captainPrefab);
            AssignObjectField(caravanController, "_segmentPrefab", segmentPrefab);
            AssignObjectField(spawner, "_caravanPrefab", caravanController);
            caravanPrefabRoot.hideFlags = HideFlags.HideInHierarchy;
        }

        private static GameObject BuildPlayerPrefab(ProjectileBasic projectilePrefab)
        {
            GameObject root = new GameObject("PlayerDrone");
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = PrototypeSpriteLibrary.WhiteSquare;
            renderer.color = new Color(0.22f, 0.76f, 1f, 1f);
            root.transform.localScale = new Vector3(0.92f, 0.5f, 1f);

            CreateSpriteChild(root.transform, "Shadow", new Vector3(0f, -0.16f, 0.1f), new Vector3(1.1f, 0.24f, 1f), new Color(0f, 0f, 0f, 0.18f));
            CreateSpriteChild(root.transform, "Body", Vector3.zero, new Vector3(0.9f, 0.5f, 1f), new Color(0.22f, 0.76f, 1f, 1f));
            Transform leftRotor = CreateSpriteChild(root.transform, "RotorLeft", new Vector3(-0.38f, 0.26f, 0f), new Vector3(0.16f, 0.62f, 1f), new Color(0.94f, 0.96f, 1f, 1f)).transform;
            Transform rightRotor = CreateSpriteChild(root.transform, "RotorRight", new Vector3(0.38f, 0.26f, 0f), new Vector3(0.16f, 0.62f, 1f), new Color(0.94f, 0.96f, 1f, 1f)).transform;
            CreateSpriteChild(root.transform, "ManipulatorLeft", new Vector3(-0.22f, -0.14f, 0f), new Vector3(0.08f, 0.26f, 1f), new Color(0.78f, 0.88f, 0.96f, 1f));
            CreateSpriteChild(root.transform, "ManipulatorRight", new Vector3(0.22f, -0.14f, 0f), new Vector3(0.08f, 0.26f, 1f), new Color(0.78f, 0.88f, 0.96f, 1f));

            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(root.transform, false);
            firePoint.transform.localPosition = new Vector3(0f, 0.56f, 0f);

            WeaponAutoFire autoFire = root.AddComponent<WeaponAutoFire>();
            AssignObjectField(autoFire, "_projectilePrefab", projectilePrefab);
            AssignObjectField(autoFire, "_firePoint", firePoint.transform);
            AssignObjectField(autoFire, "_leftRotor", leftRotor);
            AssignObjectField(autoFire, "_rightRotor", rightRotor);

            return SavePrefab(root, PlayerPrefabPath);
        }

        private static CaptainController BuildCaptainPrefab()
        {
            GameObject root = new GameObject("Captain");
            root.AddComponent<SpriteRenderer>();
            root.AddComponent<CircleCollider2D>();
            CaptainController controller = root.AddComponent<CaptainController>();
            GameObject damageAnchor = new GameObject("DamageAnchor");
            damageAnchor.transform.SetParent(root.transform, false);
            damageAnchor.transform.localPosition = new Vector3(0f, 0.34f, 0f);
            AssignObjectField(controller, "_damageAnchor", damageAnchor.transform);
            return SavePrefab(root, CaptainPrefabPath).GetComponent<CaptainController>();
        }

        private static SegmentController BuildSegmentPrefab()
        {
            GameObject root = new GameObject("Segment");
            root.AddComponent<SpriteRenderer>();
            root.AddComponent<CircleCollider2D>();
            root.AddComponent<SegmentHealth>();
            SegmentController controller = root.AddComponent<SegmentController>();
            GameObject payload = CreateSpriteChild(root.transform, "PayloadPlaceholder", new Vector3(0f, 0.22f, -0.05f), new Vector3(0.28f, 0.18f, 1f), new Color(0.95f, 0.9f, 0.7f, 1f));
            GameObject hpAnchor = new GameObject("HPAnchor");
            hpAnchor.transform.SetParent(root.transform, false);
            hpAnchor.transform.localPosition = new Vector3(0f, 0.74f, 0f);
            GameObject damageAnchor = new GameObject("DamageAnchor");
            damageAnchor.transform.SetParent(root.transform, false);
            damageAnchor.transform.localPosition = new Vector3(0f, 0.2f, 0f);

            AssignObjectField(controller, "_payloadPlaceholder", payload.transform);
            AssignObjectField(controller, "_hpAnchor", hpAnchor.transform);
            AssignObjectField(controller, "_damageAnchor", damageAnchor.transform);
            return SavePrefab(root, SegmentPrefabPath).GetComponent<SegmentController>();
        }

        private static ProjectileBasic BuildProjectilePrefab()
        {
            GameObject root = new GameObject("ProjectileBasic");
            root.AddComponent<SpriteRenderer>();
            root.AddComponent<ProjectileBasic>();
            return SavePrefab(root, ProjectilePrefabPath).GetComponent<ProjectileBasic>();
        }

        private static void BuildFloatingDamageNumberPrefab()
        {
            GameObject root = new GameObject("FloatingDamageNumber");
            root.AddComponent<KitchenCaravan.UI.FloatingDamageNumber>();
            SavePrefab(root, HitNumberPrefabPath);
        }

        private static void BuildHitFlashPrefab()
        {
            GameObject root = new GameObject("TemporaryHitFlash");
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = PrototypeSpriteLibrary.WhiteSquare;
            root.AddComponent<TemporaryHitFlash>();
            SavePrefab(root, HitFlashPrefabPath);
        }

        private static GameObject SavePrefab(GameObject root, string path)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateSpriteChild(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            child.transform.localScale = localScale;
            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = PrototypeSpriteLibrary.WhiteSquare;
            renderer.color = color;
            return child;
        }

        private static void AssignObjectField(Object target, string fieldName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(fieldName);
            if (property != null)
            {
                property.objectReferenceValue = value;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
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

        private static void EnsureProjectFolders()
        {
            EnsureFolder("Assets/Docs");
            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Prefabs/Player");
            EnsureFolder("Assets/Prefabs/Caravan");
            EnsureFolder("Assets/Prefabs/Weapons");
            EnsureFolder("Assets/Prefabs/VFX");
            EnsureFolder("Assets/ScriptableObjects");
            EnsureFolder("Assets/ScriptableObjects/Configs");
            EnsureFolder("Assets/Art");
            EnsureFolder("Assets/Art/Characters");
            EnsureFolder("Assets/Art/Characters/PlayerDrone");
            EnsureFolder("Assets/Art/Characters/Ants");
            EnsureFolder("Assets/Art/Characters/Ants/Captain");
            EnsureFolder("Assets/Art/Characters/Ants/Carrier");
            EnsureFolder("Assets/Art/Payloads");
            EnsureFolder("Assets/Art/Effects");
            EnsureFolder("Assets/Art/Effects/Hit");
            EnsureFolder("Assets/Art/Effects/Destruction");
            EnsureFolder("Assets/Art/UI");
            EnsureFolder("Assets/Art/UI/CombatNumbers");
        }

        private static void EnsureDocs()
        {
            WriteTextAssetIfDifferent("Assets/Docs/ART_GUIDE.md", "# ART GUIDE\n\nPlaceholder folders are prepared for PlayerDrone, Ant Captain, Ant Carrier, Payloads, Hit Effects, Destruction Effects, and Combat Numbers.\n\nRequired named placeholders:\n- drone_body_placeholder\n- drone_rotor_placeholder\n- drone_manipulator_placeholder\n- captain_placeholder\n- carrier_segment_placeholder\n- projectile_basic_placeholder\n- hit_flash_placeholder\n- destruction_flash_placeholder\n- floating_damage_number_placeholder\n- hp_label_placeholder\n");
            WriteTextAssetIfDifferent("Assets/Docs/PROTOTYPE_LEVEL01_SCOPE.md", "# PROTOTYPE LEVEL 01 SCOPE\n\nThis prototype contains one playable scene, one fixed route, one player drone, one captain, eight caravan segments, HP labels, floating damage numbers, auto-fire combat, deterministic collapse, win/lose flow, and placeholder visuals.\n");
            WriteTextAssetIfDifferent("Assets/Docs/ARCHITECTURE_NOTES.md", "# ARCHITECTURE NOTES\n\nThe prototype runtime is organized into Core, Route, Caravan, Combat, UI, and Utils.\n\nRefactor summary:\n- unified route-following around RoutePath plus RouteSampler\n- caravan owns target selection and collapse after segment removal\n- player auto-fire aims from the bottom lane toward the frontmost valid target\n- segment and captain feedback are self-contained and no longer depend on the older _Project runtime layer\n- scene and prefabs are rebuilt from PrototypeLevel01Builder\n");
        }

        private static void WriteTextAssetIfDifferent(string assetPath, string content)
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath.Replace('/', Path.DirectorySeparatorChar));
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(fullPath) || File.ReadAllText(fullPath) != content)
            {
                File.WriteAllText(fullPath, content);
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string name = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif