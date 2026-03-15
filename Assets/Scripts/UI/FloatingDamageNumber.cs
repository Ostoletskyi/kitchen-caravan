using UnityEngine;
using KitchenCaravan.Utils;

namespace KitchenCaravan.UI
{
    // Floating combat text with a fast arcade-style pop, upward drift, and fade.
    public sealed class FloatingDamageNumber : MonoBehaviour
    {
        [SerializeField] private float _lifetime = 0.85f;
        [SerializeField] private Vector3 _velocity = new Vector3(0f, 1.15f, 0f);

        private TextMesh _mainText;
        private TextMesh _shadowText;
        private float _elapsed;
        private Camera _camera;

        public static void Spawn(Vector3 position, int value, bool isCritical)
        {
            GameObject go = new GameObject(isCritical ? "CriticalDamageNumber" : "FloatingDamageNumber");
            go.transform.position = position;
            FloatingDamageNumber number = go.AddComponent<FloatingDamageNumber>();
            number.Show(value, isCritical);
        }

        public void Show(int value, bool isCritical)
        {
            EnsureMeshes();
            string text = NumberFormatUtil.Format(value);
            _mainText.text = text;
            _shadowText.text = text;
            _mainText.color = isCritical ? new Color(1f, 0.28f, 0.28f, 1f) : Color.white;
            _shadowText.color = isCritical ? new Color(0.35f, 0.08f, 0.08f, 1f) : new Color(0.15f, 0.15f, 0.15f, 1f);
            transform.localScale = Vector3.one * (isCritical ? 1.25f : 1f);
            _elapsed = 0f;
        }

        private void Awake()
        {
            _camera = Camera.main;
            EnsureMeshes();
        }

        private void LateUpdate()
        {
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _lifetime);
            transform.position += _velocity * Time.deltaTime;
            transform.localScale = Vector3.one * (1f + Mathf.Sin(t * Mathf.PI) * 0.18f);

            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_camera != null)
            {
                transform.forward = _camera.transform.forward;
            }

            if (_mainText != null)
            {
                Color main = _mainText.color;
                main.a = 1f - t;
                _mainText.color = main;
            }

            if (_shadowText != null)
            {
                Color shadow = _shadowText.color;
                shadow.a = 1f - t;
                _shadowText.color = shadow;
            }

            if (_elapsed >= _lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void EnsureMeshes()
        {
            if (_mainText == null)
            {
                _mainText = CreateMesh("Main", Vector3.zero, Color.white, 42);
            }

            if (_shadowText == null)
            {
                _shadowText = CreateMesh("Shadow", new Vector3(0.035f, -0.035f, 0.05f), new Color(0.15f, 0.15f, 0.15f, 1f), 42);
            }
        }

        private TextMesh CreateMesh(string childName, Vector3 localPosition, Color color, int fontSize)
        {
            Transform child = transform.Find(childName);
            TextMesh textMesh;
            if (child == null)
            {
                GameObject go = new GameObject(childName);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = localPosition;
                textMesh = go.AddComponent<TextMesh>();
            }
            else
            {
                child.localPosition = localPosition;
                textMesh = child.GetComponent<TextMesh>() ?? child.gameObject.AddComponent<TextMesh>();
            }

            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = 0.11f;
            textMesh.fontSize = fontSize;
            textMesh.fontStyle = FontStyle.Bold;
            textMesh.color = color;
            return textMesh;
        }
    }
}