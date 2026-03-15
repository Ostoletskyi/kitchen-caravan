#if UNITY_EDITOR
using System.IO;
using KitchenCaravan.Caravan;
using KitchenCaravan.Combat;
using KitchenCaravan.Core;
using KitchenCaravan.Route;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KitchenCaravan.Editor
{
    // Builds the full Level 1 prototype scene, prefabs, and config assets in one editor action.
    public static class PrototypeLevel01Builder
    {
        private const string ScenePath = "Assets/Scenes/Prototype_Level01.unity";
        private const string ConfigPath = "Assets/ScriptableObjects/Configs/PrototypeLevel01CaravanConfig.asset";
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/PlayerDrone.prefab";
        private const string CaptainPrefabPath = "Assets/Prefabs/Caravan/Captain.prefab";
        private const string SegmentPrefabPath = "Assets/Prefabs/Caravan/Segment.prefab";
        private const string ProjectilePrefabPath = "Assets/Prefabs/Weapons/ProjectileBasic.prefab";
        private const string HitNumberPrefabPath = "Assets/Prefabs/VFX/HitNumber.prefab";
        private const string HitFlashPrefabPath = "Assets/Prefabs/VFX/HitFlash.prefab";

        [MenuItem("KitchenCaravan/Prototype/Build Level 01")]
        public static void BuildLevel01()
        {
            EnsureFolder("Assets/ScriptableObjects");
            EnsureFolder("Assets/ScriptableObjects/Configs");
            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Prefabs/Player");
            EnsureFolder("Assets/Prefabs/Caravan");
            EnsureFolder("Assets/Prefabs/Weapons");
            EnsureFolder("Assets/Prefabs/VFX");

            CaravanConfig config = CreateOrLoadAsset<CaravanConfig>(ConfigPath);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            CaptainController captainPrefab = BuildCaptainPrefab();
            SegmentController segmentPrefab = BuildSegmentPrefab();
            ProjectileBasic projectilePrefab = BuildProjectilePrefab();
            BuildHitNumberPrefab();
            BuildHitFlashPrefab();
            GameObject playerPrefab = BuildPlayerPrefab(projectilePrefab);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildSceneObjects(scene, config, playerPrefab, captainPrefab, segmentPrefab);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log("Prototype Level 01 built.");
        }

        private static void BuildSceneObjects(Scene scene, CaravanConfig config, GameObject playerPrefab, CaptainController captainPrefab, SegmentController segmentPrefab)
        {
            Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.91f, 0.88f, 0.78f, 1f);
            camera.tag = "MainCamera";

            GameObject gameManagerObject = new GameObject("GameManager");
            GameManager gameManager = gameManagerObject.AddComponent<GameManager>();

            GameObject routeObject = new GameObject("RouteObject");
            KitchenCaravan.Route.RoutePath routePath = routeObject.AddComponent<KitchenCaravan.Route.RoutePath>();

            if (playerPrefab != null)
            {
                GameObject player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
                player.name = "PlayerDrone";
                player.transform.position = new Vector3(0f, -4.8f, 0f);
            }

            GameObject caravanSpawnerObject = new GameObject("CaravanSpawner");
            CaravanSpawner spawner = caravanSpawnerObject.AddComponent<CaravanSpawner>();
            AssignObjectField(spawner, "_routePath", routePath);
            AssignObjectField(spawner, "_caravanConfig", config);
            AssignObjectField(spawner, "_gameManager", gameManager);

            GameObject caravanPrefabRoot = new GameObject("PrototypeCaravanPrefabRuntime");
            CaravanController caravanController = caravanPrefabRoot.AddComponent<CaravanController>();
            AssignObjectField(caravanController, "_captainPrefab", captainPrefab);
            AssignObjectField(caravanController, "_segmentPrefab", segmentPrefab);
            AssignObjectField(spawner, "_caravanPrefab", caravanController);
            caravanPrefabRoot.hideFlags = HideFlags.HideInHierarchy;
        }

        private static GameObject BuildPlayerPrefab(ProjectileBasic projectilePrefab)
        {
            GameObject root = new GameObject("PlayerDrone");
            SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
            sr.color = new Color(0f, 0f, 0f, 0f);

            GameObject body = CreateSpriteChild(root.transform, "Body", new Vector3(0f, 0f, 0f), new Vector3(0.9f, 0.55f, 1f), new Color(0.35f, 0.85f, 1f, 1f));
            CreateSpriteChild(root.transform, "RotorLeft", new Vector3(-0.38f, 0.32f, 0f), new Vector3(0.14f, 0.58f, 1f), new Color(0.95f, 0.95f, 1f, 1f));
            CreateSpriteChild(root.transform, "RotorRight", new Vector3(0.38f, 0.32f, 0f), new Vector3(0.14f, 0.58f, 1f), new Color(0.95f, 0.95f, 1f, 1f));
            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(root.transform, false);
            firePoint.transform.localPosition = new Vector3(0f, 0.55f, 0f);

            WeaponAutoFire autoFire = root.AddComponent<WeaponAutoFire>();
            AssignObjectField(autoFire, "_projectilePrefab", projectilePrefab);
            AssignObjectField(autoFire, "_firePoint", firePoint.transform);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static CaptainController BuildCaptainPrefab()
        {
            GameObject root = new GameObject("Captain");
            CaptainController controller = root.AddComponent<CaptainController>();
            root.AddComponent<CircleCollider2D>();
            GameObject visual = CreateSpriteChild(root.transform, "VisualRoot", Vector3.zero, new Vector3(0.95f, 0.7f, 1f), new Color(0.95f, 0.75f, 0.25f, 1f));
            GameObject damageAnchor = new GameObject("DamageAnchor");
            damageAnchor.transform.SetParent(root.transform, false);
            damageAnchor.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            AssignObjectField(controller, "_visualRoot", visual.transform);
            AssignObjectField(controller, "_damageAnchor", damageAnchor.transform);
            CaptainController prefab = PrefabUtility.SaveAsPrefabAsset(root, CaptainPrefabPath).GetComponent<CaptainController>();
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static SegmentController BuildSegmentPrefab()
        {
            GameObject root = new GameObject("Segment");
            root.AddComponent<CircleCollider2D>();
            root.AddComponent<SegmentHealth>();
            SegmentController controller = root.AddComponent<SegmentController>();
            GameObject visualRoot = CreateSpriteChild(root.transform, "VisualRoot", Vector3.zero, new Vector3(0.7f, 0.54f, 1f), new Color(0.45f, 0.88f, 0.45f, 1f));
            GameObject payload = CreateSpriteChild(root.transform, "PayloadPlaceholder", new Vector3(0f, 0.18f, -0.05f), new Vector3(0.28f, 0.18f, 1f), new Color(0.95f, 0.95f, 0.95f, 0.9f));
            GameObject hpAnchor = new GameObject("HPAnchor");
            hpAnchor.transform.SetParent(root.transform, false);
            hpAnchor.transform.localPosition = new Vector3(0f, 0.72f, 0f);
            GameObject damageAnchor = new GameObject("DamageAnchor");
            damageAnchor.transform.SetParent(root.transform, false);
            damageAnchor.transform.localPosition = new Vector3(0f, 0.2f, 0f);

            AssignObjectField(controller, "_visualRoot", visualRoot.transform);
            AssignObjectField(controller, "_payloadPlaceholder", payload.transform);
            AssignObjectField(controller, "_hpAnchor", hpAnchor.transform);
            AssignObjectField(controller, "_damageAnchor", damageAnchor.transform);

            SegmentController prefab = PrefabUtility.SaveAsPrefabAsset(root, SegmentPrefabPath).GetComponent<SegmentController>();
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static ProjectileBasic BuildProjectilePrefab()
        {
            GameObject root = new GameObject("ProjectileBasic");
            root.AddComponent<SpriteRenderer>();
            ProjectileBasic projectile = root.AddComponent<ProjectileBasic>();
            ProjectileBasic prefab = PrefabUtility.SaveAsPrefabAsset(root, ProjectilePrefabPath).GetComponent<ProjectileBasic>();
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void BuildHitNumberPrefab()
        {
            GameObject root = new GameObject("HitNumber");
            root.AddComponent<KitchenCaravan.UI.FloatingDamageNumber>();
            PrefabUtility.SaveAsPrefabAsset(root, HitNumberPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void BuildHitFlashPrefab()
        {
            GameObject root = new GameObject("HitFlash");
            root.AddComponent<SpriteRenderer>();
            root.AddComponent<TemporaryHitFlash>();
            PrefabUtility.SaveAsPrefabAsset(root, HitFlashPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static GameObject CreateSpriteChild(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            child.transform.localScale = localScale;
            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = KitchenCaravan.VerticalSlice.RuntimeSpriteFactory.WhiteSquare;
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
