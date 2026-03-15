using UnityEngine;
using KitchenCaravan.Utils;

namespace KitchenCaravan.UI
{
    // World-space HP text that stays above a segment and updates in real time.
    public sealed class SegmentHpView : MonoBehaviour
    {
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0.7f, 0f);

        private TextMesh _mainText;
        private TextMesh _shadowText;

        private void Awake()
        {
            EnsureMeshes();
        }

        public void SetValue(int hp)
        {
            EnsureMeshes();
            string text = NumberFormatUtil.Format(hp);
            _mainText.text = text;
            _shadowText.text = text;
        }

        private void LateUpdate()
        {
            transform.localPosition = _offset;
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
            textMesh.fontSize = 36;
            textMesh.fontStyle = FontStyle.Bold;
            textMesh.color = color;
            return textMesh;
        }
    }
}
