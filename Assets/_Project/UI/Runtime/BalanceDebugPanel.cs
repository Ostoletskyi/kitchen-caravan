using System;
using UnityEngine;
using UnityEngine.UI;

namespace KitchenCaravan.VerticalSlice
{
    public class BalanceDebugPanel : MonoBehaviour
    {
        [SerializeField] private KeyCode _toggleKey = KeyCode.F1;

        private Canvas _canvas;
        private RectTransform _panel;
        private bool _visible = true;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            BalanceDebugSettings.EnsureDefaults();
            EnsurePanel();
            SetVisible(_visible);
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Input.GetKeyDown(_toggleKey))
            {
                SetVisible(!_visible);
            }
#endif
        }

        private void EnsurePanel()
        {
            if (_canvas != null)
            {
                return;
            }

            UIHudController.EnsureEventSystem();

            var canvasGO = new GameObject("BalanceDebugCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvas = canvasGO.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 250;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            DontDestroyOnLoad(canvasGO);

            var panelGO = new GameObject("BalancePanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            _panel = panelGO.GetComponent<RectTransform>();
            _panel.SetParent(canvasGO.transform, false);
            _panel.anchorMin = new Vector2(0f, 1f);
            _panel.anchorMax = new Vector2(0f, 1f);
            _panel.pivot = new Vector2(0f, 1f);
            _panel.anchoredPosition = new Vector2(20f, -20f);
            _panel.sizeDelta = new Vector2(430f, 0f);

            panelGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

            var layout = panelGO.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            var fitter = panelGO.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreateTitle("Balance Debug (F1)");
            CreateSlider("Player Fire Rate", 0.5f, 10f, BalanceDebugSettings.PlayerFireRate, BalanceDebugSettings.SetPlayerFireRate, "shots/s");
            CreateSlider("Bullet Damage", 1f, 25f, BalanceDebugSettings.BulletDamage, BalanceDebugSettings.SetBulletDamage, "");
            CreateSlider("Segment Base HP", 1f, 100f, BalanceDebugSettings.ChainSegmentBaseHp, BalanceDebugSettings.SetSegmentBaseHp, "");
            CreateSlider("HP Increment", 0f, 25f, BalanceDebugSettings.ChainSegmentHpIncrement, BalanceDebugSettings.SetSegmentIncrement, "");
            CreateSlider("Chain Length", 1f, 24f, BalanceDebugSettings.ChainLength, BalanceDebugSettings.SetChainLength, "");
            CreateSlider("Chain Speed", 0.2f, 8f, BalanceDebugSettings.ChainMoveSpeed, BalanceDebugSettings.SetChainMoveSpeed, "");
            CreateSlider("Spawn Delay", 0.3f, 8f, BalanceDebugSettings.ChainSpawnDelay, BalanceDebugSettings.SetSpawnDelay, "sec");
            CreateSlider("Player Move Speed", 1f, 20f, BalanceDebugSettings.PlayerMoveSpeed, BalanceDebugSettings.SetPlayerMoveSpeed, "");
        }

        private void CreateTitle(string text)
        {
            var row = new GameObject("Title", typeof(RectTransform), typeof(Text));
            row.transform.SetParent(_panel, false);

            var title = row.GetComponent<Text>();
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.fontSize = 24;
            title.alignment = TextAnchor.MiddleLeft;
            title.color = Color.white;
            title.text = text;
        }

        private void CreateSlider(string label, float min, float max, float initialValue, Action<float> setter, string suffix)
        {
            var row = new GameObject(label, typeof(RectTransform), typeof(VerticalLayoutGroup));
            row.transform.SetParent(_panel, false);

            var rowLayout = row.GetComponent<VerticalLayoutGroup>();
            rowLayout.spacing = 3f;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandHeight = false;

            var labelText = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            labelText.transform.SetParent(row.transform, false);
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 18;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.color = Color.white;

            var sliderGO = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            sliderGO.transform.SetParent(row.transform, false);
            var slider = sliderGO.GetComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = Mathf.Clamp(initialValue, min, max);

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(sliderGO.transform, false);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0f);
            bgRect.anchorMax = new Vector2(1f, 1f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGO.transform, false);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0f);
            fillAreaRect.anchorMax = new Vector2(1f, 1f);
            fillAreaRect.offsetMin = new Vector2(8f, 6f);
            fillAreaRect.offsetMax = new Vector2(-8f, -6f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            fill.GetComponent<Image>().color = new Color(0.35f, 0.85f, 0.45f, 1f);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(sliderGO.transform, false);
            var handleImage = handle.GetComponent<Image>();
            handleImage.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            handle.GetComponent<RectTransform>().sizeDelta = new Vector2(20f, 34f);

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;

            void RefreshLabel(float value)
            {
                string formatted = Mathf.Abs(value - Mathf.Round(value)) < 0.001f ? Mathf.RoundToInt(value).ToString() : value.ToString("0.00");
                labelText.text = string.IsNullOrEmpty(suffix) ? $"{label}: {formatted}" : $"{label}: {formatted} {suffix}";
            }

            slider.onValueChanged.AddListener(value =>
            {
                setter(value);
                RefreshLabel(value);
            });

            setter(slider.value);
            RefreshLabel(slider.value);
        }

        private void SetVisible(bool visible)
        {
            _visible = visible;
            if (_panel != null)
            {
                _panel.gameObject.SetActive(visible);
            }
        }
    }
}
