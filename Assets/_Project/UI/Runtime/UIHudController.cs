using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KitchenCaravan.VerticalSlice
{
    public class UIHudController : MonoBehaviour
    {
        [SerializeField] private Text _counterText;
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private GameObject _losePanel;

        private GameFlowController _flow;
        private CaravanController _caravan;

        private void Start()
        {
            if (_flow == null)
            {
                _flow = FindFirstObjectByType<GameFlowController>();
            }

            if (_flow != null)
            {
                Bind(_flow);
            }
        }

        private void Update()
        {
            if (_caravan == null)
            {
                _caravan = FindFirstObjectByType<CaravanController>();
            }

            RefreshCounter();
        }

        public void Bind(GameFlowController flow)
        {
            _flow = flow;
            EnsureHudExists(flow);

            flow.DefeatedChanged -= OnDefeatedChanged;
            flow.WinTriggered -= OnWinTriggered;
            flow.LoseTriggered -= OnLoseTriggered;
            flow.DefeatedChanged += OnDefeatedChanged;
            flow.WinTriggered += OnWinTriggered;
            flow.LoseTriggered += OnLoseTriggered;

            RefreshCounter();
            _winPanel.SetActive(flow.State == GameFlowController.FlowState.Win);
            _losePanel.SetActive(flow.State == GameFlowController.FlowState.Lose);
        }

        private void OnDestroy()
        {
            if (_flow == null)
            {
                return;
            }

            _flow.DefeatedChanged -= OnDefeatedChanged;
            _flow.WinTriggered -= OnWinTriggered;
            _flow.LoseTriggered -= OnLoseTriggered;
        }

        private void OnDefeatedChanged(int current, int target)
        {
            RefreshCounter();
        }

        private void OnWinTriggered()
        {
            if (_winPanel != null)
            {
                _winPanel.SetActive(true);
            }
        }

        private void OnLoseTriggered()
        {
            if (_losePanel != null)
            {
                _losePanel.SetActive(true);
            }
        }

        private void RefreshCounter()
        {
            if (_counterText == null)
            {
                return;
            }

            if (_caravan == null)
            {
                _counterText.text = "Segments Left: --";
                return;
            }

            int segmentsLeft = _caravan.LivingSegmentCount;
            _counterText.text = _caravan.IsRaging
                ? $"Segments Left: {segmentsLeft}   CAPTAIN RAGE"
                : $"Segments Left: {segmentsLeft}";
        }

        private void EnsureHudExists(GameFlowController flow)
        {
            if (_counterText != null && _winPanel != null && _losePanel != null)
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
            scaler.matchWidthOrHeight = 0.75f;

            _counterText = CreateText(
                canvas.transform,
                "Counter",
                "Segments Left: 10",
                new Vector2(0f, -26f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(980f, 80f),
                40,
                TextAnchor.MiddleCenter);

            _winPanel = CreateStatePanel(canvas.transform, "YOU WIN", flow.RestartLevel, flow.GoToMainMenu, "WinPanel");
            _losePanel = CreateStatePanel(canvas.transform, "YOU LOSE", flow.RestartLevel, flow.GoToMainMenu, "LosePanel");
            _winPanel.SetActive(false);
            _losePanel.SetActive(false);
        }

        private static GameObject CreateStatePanel(Transform canvas, string title, UnityEngine.Events.UnityAction restart, UnityEngine.Events.UnityAction mainMenu, string objectName)
        {
            var panel = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas, false);
            var panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(700f, 420f);
            panelRT.anchoredPosition = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);

            CreateText(
                panel.transform,
                "Title",
                title,
                new Vector2(0f, 120f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(620f, 110f),
                58,
                TextAnchor.MiddleCenter);
            CreateButton(panel.transform, "RestartButton", "Restart", new Vector2(0f, 10f), restart);
            CreateButton(panel.transform, "MainMenuButton", "Main Menu", new Vector2(0f, -105f), mainMenu);
            return panel;
        }

        private static Text CreateText(
            Transform parent,
            string name,
            string text,
            Vector2 pos,
            Vector2 anchor,
            Vector2 pivot,
            Vector2 size,
            int fontSize,
            TextAnchor textAnchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            var textComp = go.GetComponent<Text>();
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = fontSize;
            textComp.alignment = textAnchor;
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

            var label = CreateText(
                go.transform,
                "Label",
                text,
                Vector2.zero,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(320f, 70f),
                34,
                TextAnchor.MiddleCenter);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
        }

        public static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventGO = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(eventGO);
        }
    }
}
