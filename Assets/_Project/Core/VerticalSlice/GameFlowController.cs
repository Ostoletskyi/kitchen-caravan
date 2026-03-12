using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KitchenCaravan.VerticalSlice
{
    public class GameFlowController : MonoBehaviour
    {
        public enum FlowState
        {
            Playing,
            Win
        }

        [SerializeField] private int _targetDefeats = 20;
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private UIHudController _hud;
        [SerializeField] private string _mainMenuScene = "MainMenu";

        public FlowState State { get; private set; } = FlowState.Playing;
        public int DefeatedCount { get; private set; }
        public int TargetDefeats => _targetDefeats;

        public event Action<int, int> DefeatedChanged;
        public event Action WinTriggered;

        private void Awake()
        {
            BalanceDebugSettings.EnsureDefaults();
            Time.timeScale = 1f;
            State = FlowState.Playing;
            DefeatedCount = 0;

            if (_enemySpawner == null)
            {
                _enemySpawner = FindObjectOfType<EnemySpawner>();
            }

            if (_enemySpawner != null)
            {
                _enemySpawner.Configure(this);
            }

            if (_hud == null)
            {
                _hud = FindObjectOfType<UIHudController>();
            }

            if (_hud != null)
            {
                _hud.Bind(this);
            }

            EnsureDebugPanel();
        }

        public void RegisterEnemyDefeated()
        {
            if (State != FlowState.Playing)
            {
                return;
            }

            DefeatedCount++;
            DefeatedChanged?.Invoke(DefeatedCount, _targetDefeats);

            if (DefeatedCount >= _targetDefeats)
            {
                TriggerWin();
            }
        }

        public void TriggerWin()
        {
            if (State != FlowState.Playing)
            {
                return;
            }

            State = FlowState.Win;
            Time.timeScale = 0.2f;

            if (_enemySpawner != null)
            {
                _enemySpawner.enabled = false;
            }

            WinTriggered?.Invoke();
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(_mainMenuScene);
        }

        public void SetDependencies(EnemySpawner spawner, UIHudController hud)
        {
            _enemySpawner = spawner;
            _hud = hud;
        }

        private static void EnsureDebugPanel()
        {
            if (FindObjectOfType<BalanceDebugPanel>() != null)
            {
                return;
            }

            var go = new GameObject("BalanceDebugPanel");
            go.AddComponent<BalanceDebugPanel>();
        }
    }
}
