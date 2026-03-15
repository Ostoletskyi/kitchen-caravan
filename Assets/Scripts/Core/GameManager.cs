using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KitchenCaravan.Core
{
    // Lightweight prototype game manager that owns win/lose flow, simple UI, and restart support.
    public class GameManager : MonoBehaviour
    {
        public event Action<GameState> StateChanged;
        public event Action<GameState, GameState> StateTransitioned;

        [SerializeField] private GameState _initialState = GameState.Boot;
        [SerializeField] private bool _autoEnterRunOnStart = true;
        [SerializeField] private Text _statusLabel;
        [SerializeField] private Button _restartButton;

        public GameState CurrentState { get; private set; }
        public bool IsGameplayActive => CurrentState == GameState.Run;

        private void Awake()
        {
            CurrentState = _initialState;
            EnsurePrototypeUi();
            RefreshUi();
            StateChanged?.Invoke(CurrentState);
        }

        private void Start()
        {
            if (_autoEnterRunOnStart && CurrentState == GameState.Boot)
            {
                ChangeState(GameState.Run);
            }
        }

        public void ChangeState(GameState newState)
        {
            if (newState == CurrentState)
            {
                return;
            }

            GameState previous = CurrentState;
            CurrentState = newState;
            RefreshUi();
            StateTransitioned?.Invoke(previous, newState);
            StateChanged?.Invoke(CurrentState);
        }

        public void TriggerWin()
        {
            if (!IsGameplayActive)
            {
                return;
            }

            ChangeState(GameState.Win);
        }

        public void TriggerLose()
        {
            if (!IsGameplayActive)
            {
                return;
            }

            ChangeState(GameState.Lose);
        }

        public void RestartCurrentScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void EnsurePrototypeUi()
        {
            if (_statusLabel != null && _restartButton != null)
            {
                return;
            }

            Canvas existingCanvas = FindFirstObjectByType<Canvas>();
            Canvas canvas = existingCanvas;
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("PrototypeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.matchWidthOrHeight = 0.75f;
            }

            if (_statusLabel == null)
            {
                GameObject labelObject = new GameObject("StatusLabel", typeof(RectTransform), typeof(Text));
                labelObject.transform.SetParent(canvas.transform, false);
                RectTransform rect = labelObject.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(700f, 180f);
                _statusLabel = labelObject.GetComponent<Text>();
                _statusLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                _statusLabel.fontSize = 72;
                _statusLabel.alignment = TextAnchor.MiddleCenter;
                _statusLabel.color = Color.white;
            }

            if (_restartButton == null)
            {
                GameObject buttonObject = new GameObject("RestartButton", typeof(RectTransform), typeof(Image), typeof(Button));
                buttonObject.transform.SetParent(canvas.transform, false);
                RectTransform rect = buttonObject.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(320f, 90f);
                rect.anchoredPosition = new Vector2(0f, -120f);
                buttonObject.GetComponent<Image>().color = new Color(0.2f, 0.55f, 0.24f, 1f);
                _restartButton = buttonObject.GetComponent<Button>();
                _restartButton.onClick.AddListener(RestartCurrentScene);

                GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
                textObject.transform.SetParent(buttonObject.transform, false);
                RectTransform textRect = textObject.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                Text buttonText = textObject.GetComponent<Text>();
                buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                buttonText.fontSize = 34;
                buttonText.alignment = TextAnchor.MiddleCenter;
                buttonText.color = Color.white;
                buttonText.text = "Restart";
            }
        }

        private void RefreshUi()
        {
            if (_statusLabel == null || _restartButton == null)
            {
                return;
            }

            _statusLabel.gameObject.SetActive(CurrentState == GameState.Win || CurrentState == GameState.Lose);
            _restartButton.gameObject.SetActive(CurrentState == GameState.Win || CurrentState == GameState.Lose);
            _statusLabel.text = CurrentState == GameState.Win ? "YOU WIN" : CurrentState == GameState.Lose ? "YOU LOSE" : string.Empty;
        }
    }
}
