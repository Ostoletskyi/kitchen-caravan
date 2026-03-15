using UnityEngine;
using KitchenCaravan.Utils;

namespace KitchenCaravan.UI
{
    // Lightweight floating combat text for prototype hits.
    public sealed class FloatingDamageNumber : MonoBehaviour
    {
        [SerializeField] private float _lifetime = 0.9f;
        [SerializeField] private Vector3 _velocity = new Vector3(0f, 0.9f, 0f);

        private TextMesh _mainText;
        private TextMesh _shadowText;
        private float _elapsed;

        public void Show(int value)
        {
            EnsureMeshes();
            string text = NumberFormatUtil.Format(value);
            _mainText.text = text;
            _shadowText.text = text;
            _mainText.color = Color.white;
            _shadowText.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            _elapsed = 0f;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _lifetime);
            transform.position += _velocity * Time.deltaTime;
            transform.localScale = Vector3.one * (1f + Mathf.Sin(t * Mathf.PI) * 0.12f);

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
                _mainText = CreateMesh("Main", Vector3.zero, Color.white);
            }

            if (_shadowText == null)
            {
                _shadowText = CreateMesh("Shadow", new Vector3(0.03f, -0.03f, 0.05f), new Color(0.15f, 0.15f, 0.15f, 1f));
            }
        }

        private TextMesh CreateMesh(string childName, Vector3 localPosition, Color color)
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
            textMesh.characterSize = 0.12f;
            textMesh.fontSize = 38;
            textMesh.fontStyle = FontStyle.Bold;
            textMesh.color = color;
            return textMesh;
        }
    }
}
