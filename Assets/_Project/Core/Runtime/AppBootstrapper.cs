using UnityEngine;

namespace KitchenCaravan.Core
{
    public sealed class AppBootstrapper : MonoBehaviour
    {
        public static AppBootstrapper Instance { get; private set; }

        private Transform _systemsRoot;

        public Transform SystemsRoot => _systemsRoot;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSystemsRoot();
        }

        public static AppBootstrapper EnsureInScene()
        {
            if (Instance != null)
            {
                Instance.EnsureSystemsRoot();
                return Instance;
            }

            var existing = FindFirstObjectByType<AppBootstrapper>();
            if (existing != null)
            {
                Instance = existing;
                existing.EnsureSystemsRoot();
                return existing;
            }

            var appObject = new GameObject("App");
            var bootstrapper = appObject.AddComponent<AppBootstrapper>();
            bootstrapper.EnsureSystemsRoot();
            return bootstrapper;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoBootstrap()
        {
            EnsureInScene();
        }

        private void EnsureSystemsRoot()
        {
            if (_systemsRoot != null)
            {
                return;
            }

            var existing = transform.Find("Systems");
            if (existing != null)
            {
                _systemsRoot = existing;
                return;
            }

            var systems = new GameObject("Systems");
            systems.transform.SetParent(transform, false);
            _systemsRoot = systems.transform;
        }
    }
}
