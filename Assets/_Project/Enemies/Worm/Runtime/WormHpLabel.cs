using UnityEngine;

namespace KitchenCaravan.Enemies.Worm
{
    public sealed class WormHpLabel : MonoBehaviour
    {
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0.8f, 0f);

        private TextMesh _textMesh;

        private void Awake()
        {
            EnsureTextMesh();
            ApplyOffset();
        }

        private void LateUpdate()
        {
            ApplyOffset();
        }

        public void SetValue(float value)
        {
            EnsureTextMesh();
            _textMesh.text = Mathf.CeilToInt(value).ToString();
        }

        public void SetOffset(Vector3 offset)
        {
            _offset = offset;
            ApplyOffset();
        }

        public void SetTextColor(Color color)
        {
            EnsureTextMesh();
            _textMesh.color = color;
        }

        private void EnsureTextMesh()
        {
            if (_textMesh != null)
            {
                return;
            }

            Transform existing = transform.Find("HpLabel");
            if (existing != null)
            {
                _textMesh = existing.GetComponent<TextMesh>();
            }

            if (_textMesh == null)
            {
                GameObject textObject = new GameObject("HpLabel");
                textObject.transform.SetParent(transform, false);
                _textMesh = textObject.AddComponent<TextMesh>();
            }

            _textMesh.characterSize = 0.12f;
            _textMesh.fontSize = 32;
            _textMesh.anchor = TextAnchor.MiddleCenter;
            _textMesh.alignment = TextAlignment.Center;
            _textMesh.text = "0";
            _textMesh.color = Color.white;
        }

        private void ApplyOffset()
        {
            if (_textMesh == null)
            {
                return;
            }

            _textMesh.transform.localPosition = _offset;
            _textMesh.transform.localRotation = Quaternion.identity;
        }
    }
}
