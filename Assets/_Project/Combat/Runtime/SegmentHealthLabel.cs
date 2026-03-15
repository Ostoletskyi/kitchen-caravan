using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public sealed class SegmentHealthLabel : MonoBehaviour
    {
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0.95f, 0f);
        [SerializeField] private Color _textColor = Color.white;
        [SerializeField] private Color _shadowColor = new Color(0.15f, 0.15f, 0.15f, 1f);

        private TextMesh _textMesh;
        private TextMesh _shadowMesh;

        private void Awake()
        {
            EnsureMeshes();
            ApplyOffset();
        }

        private void LateUpdate()
        {
            ApplyOffset();
        }

        public void SetValue(int value)
        {
            EnsureMeshes();
            string formatted = NumberShortFormatter.Format(value);
            _textMesh.text = formatted;
            _shadowMesh.text = formatted;
        }

        private void EnsureMeshes()
        {
            if (_textMesh == null)
            {
                _textMesh = GetOrCreateTextMesh("HealthText", Vector3.zero, _textColor, 0.11f, 36);
            }

            if (_shadowMesh == null)
            {
                _shadowMesh = GetOrCreateTextMesh("HealthTextShadow", new Vector3(0.03f, -0.03f, 0.05f), _shadowColor, 0.11f, 36);
            }
        }

        private TextMesh GetOrCreateTextMesh(string childName, Vector3 localOffset, Color color, float characterSize, int fontSize)
        {
            Transform child = transform.Find(childName);
            TextMesh textMesh;
            if (child == null)
            {
                GameObject go = new GameObject(childName);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = localOffset;
                textMesh = go.AddComponent<TextMesh>();
            }
            else
            {
                child.localPosition = localOffset;
                textMesh = child.GetComponent<TextMesh>();
                if (textMesh == null)
                {
                    textMesh = child.gameObject.AddComponent<TextMesh>();
                }
            }

            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = characterSize;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            textMesh.text = "0";
            return textMesh;
        }

        private void ApplyOffset()
        {
            transform.localPosition = _offset;
            transform.localRotation = Quaternion.identity;
        }
    }
}
