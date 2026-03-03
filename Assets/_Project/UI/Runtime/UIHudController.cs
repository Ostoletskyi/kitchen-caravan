using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KitchenCaravan.VerticalSlice
{
    public class UIHudController : MonoBehaviour
    {
        [SerializeField] private Text _counterText;
        [SerializeField] private GameObject _winPanel;

        private GameFlowController _flow;

        private void Start()
        {
            if (_flow == null)
            {
                _flow = FindObjectOfType<GameFlowController>();
            }

            if (_flow != null)
            {
                Bind(_flow);
            }
        }

        public void Bind(GameFlowController flow)
        {
            _flow = flow;
            EnsureHudExists(flow);

            flow.DefeatedChanged -= OnDefeatedChanged;
            flow.WinTriggered -= OnWinTriggered;
            flow.DefeatedChanged += OnDefeatedChanged;
            flow.WinTriggered += OnWinTriggered;

            OnDefeatedChanged(flow.DefeatedCount, flow.TargetDefeats);
            _winPanel.SetActive(flow.State == GameFlowController.FlowState.Win);
        }

        private void OnDestroy()
        {
            if (_flow == null)
            {
                return;
            }

            _flow.DefeatedChanged -= OnDefeatedChanged;
            _flow.WinTriggered -= OnWinTriggered;
        }

        private void OnDefeatedChanged(int current, int target)
        {
            if (_counterText != null)
            {
                _counterText.text = $"Defeated: {current} / {target}";
            }
        }

        private void OnWinTriggered()
        {
            if (_winPanel != null)
            {
                _winPanel.SetActive(true);
            }
        }

        private void EnsureHudExists(GameFlowController flow)
        {
            if (_counterText != null && _winPanel != null)
            {
                return;
            }

            EnsureEventSystem();

            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            _counterText = CreateText(canvas.transform, "Counter", "Defeated: 0 / 0", new Vector2(170f, -40f), 42, TextAnchor.MiddleLeft);

            _winPanel = new GameObject("LevelCompletePanel", typeof(RectTransform), typeof(Image));
            _winPanel.transform.SetParent(canvas.transform, false);
            var panelRT = _winPanel.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(700f, 420f);
            panelRT.anchoredPosition = Vector2.zero;
            _winPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);

            CreateText(_winPanel.transform, "CompleteLabel", "Level Complete", new Vector2(0f, 120f), 58, TextAnchor.MiddleCenter);
            CreateButton(_winPanel.transform, "RestartButton", "Restart", new Vector2(0f, 10f), flow.RestartLevel);
            CreateButton(_winPanel.transform, "MainMenuButton", "Main Menu", new Vector2(0f, -105f), flow.GoToMainMenu);
            _winPanel.SetActive(false);
        }

        private static Text CreateText(Transform parent, string name, string text, Vector2 pos, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(1000f, 120f);
            rt.anchoredPosition = pos;

            var textComp = go.GetComponent<Text>();
            textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComp.fontSize = fontSize;
            textComp.alignment = anchor;
            textComp.color = Color.white;
            textComp.text = text;
            return textComp;
        }

        private static void CreateButton(Transform parent, string name, string text, Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(360f, 80f);
            rt.anchoredPosition = pos;

            var image = go.GetComponent<Image>();
            image.color = new Color(0.2f, 0.35f, 0.2f, 1f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            var label = CreateText(go.transform, "Label", text, Vector2.zero, 34, TextAnchor.MiddleCenter);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
        }

        public static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            var eventGO = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(eventGO);
        }
    }
}
