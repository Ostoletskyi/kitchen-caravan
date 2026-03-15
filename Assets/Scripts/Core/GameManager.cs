using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using KitchenCaravan.Caravan;

namespace KitchenCaravan.Core
{
    // Owns the minimal run state, end-of-level UI, restart support, and simple prototype HUD.
    public sealed class GameManager : MonoBehaviour
    {
        public event Action<GameState> StateChanged;

        [SerializeField] private Text _statusLabel;
        [SerializeField] private Text _debugLabel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private KeyCode _restartKey = KeyCode.R;

        public GameState CurrentState { get; private set; } = GameState.Boot;
        public bool IsGameplayActive => CurrentState == GameState.Run;

        private CaravanController _caravan;
        private int _remainingTargets;
        private int _remainingSegments;

        private void Awake()
        {
            EnsurePrototypeUi();
            ChangeState(GameState.Run);
        }

        private void Update()
        {
            if ((CurrentState == GameState.Win || CurrentState == GameState.Lose) && Input.GetKeyDown(_restartKey))
            {
                RestartCurrentScene();
            }

            RefreshDebugHud();
        }

        public void RegisterCaravan(CaravanController caravan)
        {
            _caravan = caravan;
            RefreshDebugHud();
        }

        public void SetRemainingTargets(int remainingTargets, int remainingSegments)
        {
            _remainingTargets = Mathf.Max(0, remainingTargets);
            _remainingSegments = Mathf.Max(0, remainingSegments);
            RefreshDebugHud();
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
            Scene activeScene = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(activeScene.path))
            {
                SceneManager.LoadScene(activeScene.path);
            }
            else
            {
                SceneManager.LoadScene(activeScene.buildIndex);
            }
        }

        private void ChangeState(GameState newState)
        {
            if (CurrentState == newState)
            {
                return;
            }

            CurrentState = newState;
            RefreshStateUi();
            StateChanged?.Invoke(CurrentState);
        }

        private void EnsurePrototypeUi()
        {
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
                scaler.matchWidthOrHeight = 0.8f;
            }

            if (_statusLabel == null)
            {
                _statusLabel = CreateText(canvas.transform, "StatusLabel", new Vector2(0f, 160f), 76, TextAnchor.MiddleCenter);
            }

            if (_debugLabel == null)
            {
                _debugLabel = CreateText(canvas.transform, "DebugLabel", new Vector2(0f, 820f), 34, TextAnchor.UpperCenter);
            }

            if (_restartButton == null)
            {
                GameObject buttonObject = new GameObject("RestartButton", typeof(RectTransform), typeof(Image), typeof(Button));
                buttonObject.transform.SetParent(canvas.transform, false);
                RectTransform rect = buttonObject.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(340f, 94f);
                rect.anchoredPosition = new Vector2(0f, 30f);
                buttonObject.GetComponent<Image>().color = new Color(0.14f, 0.58f, 0.27f, 1f);
                _restartButton = buttonObject.GetComponent<Button>();
                _restartButton.onClick.AddListener(RestartCurrentScene);

                Text buttonText = CreateText(buttonObject.transform, "Label", Vector2.zero, 34, TextAnchor.MiddleCenter);
                RectTransform textRect = buttonText.rectTransform;
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                buttonText.text = "Restart";
            }

            RefreshStateUi();
            RefreshDebugHud();
        }

        private void RefreshStateUi()
        {
            if (_statusLabel == null || _restartButton == null)
            {
                return;
            }

            bool showEndState = CurrentState == GameState.Win || CurrentState == GameState.Lose;
            _statusLabel.gameObject.SetActive(showEndState);
            _restartButton.gameObject.SetActive(showEndState);
            _statusLabel.text = CurrentState == GameState.Win ? "YOU WIN" : CurrentState == GameState.Lose ? "YOU LOSE" : string.Empty;
        }

        private void RefreshDebugHud()
        {
            if (_debugLabel == null)
            {
                return;
            }

            _debugLabel.text = $"Targets: {_remainingTargets}   Segments: {_remainingSegments}   Restart: {_restartKey}";
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchoredPosition, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(900f, 140f);
            rect.anchoredPosition = anchoredPosition;
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            return text;
        }
    }
}