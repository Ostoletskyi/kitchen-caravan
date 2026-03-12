using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KitchenCaravan.VerticalSlice
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string _levelSceneName = "Level_01";

        private void Start()
        {
            EnsureMenuCanvas();
        }

        public void OnPlayPressed()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(_levelSceneName);
        }

        public void OnQuitPressed()
        {
#if UNITY_EDITOR
            Debug.Log("Quit pressed (Editor): stopping play mode.");
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void EnsureMenuCanvas()
        {
            if (FindAnyObjectByType<Canvas>() != null)
            {
                return;
            }

            UIHudController.EnsureEventSystem();

            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            CreateLabel(canvas.transform, "Title", "Kitchen Caravan", new Vector2(0f, 280f), 96);
            CreateButton(canvas.transform, "PlayButton", "Play", new Vector2(0f, 40f), OnPlayPressed);
            CreateButton(canvas.transform, "QuitButton", "Quit", new Vector2(0f, -80f), OnQuitPressed);
        }

        private static void CreateLabel(Transform parent, string name, string text, Vector2 pos, int size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(900f, 150f);
            rt.anchoredPosition = pos;

            var label = go.GetComponent<Text>();
            label.text = text;
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = size;
            label.color = Color.white;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void CreateButton(Transform parent, string name, string text, Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(parent, false);

            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(300f, 80f);
            rt.anchoredPosition = pos;

            var image = root.GetComponent<Image>();
            image.color = new Color(0.2f, 0.35f, 0.2f, 1f);

            var button = root.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(root.transform, false);
            var labelRT = label.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;

            var textComp = label.GetComponent<Text>();
            textComp.text = text;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.fontSize = 36;
            textComp.color = Color.white;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
