using System;
using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public sealed class DamageNumberView : MonoBehaviour
    {
        [SerializeField] private float _duration = 1f;
        [SerializeField] private Vector3 _normalVelocity = new Vector3(0f, 1.2f, 0f);
        [SerializeField] private Vector3 _criticalVelocity = new Vector3(0f, 1.45f, 0f);

        private TextMesh _mainText;
        private TextMesh _shadowText;
        private Action<DamageNumberView> _release;
        private float _elapsed;
        private Vector3 _velocity;
        private Color _mainColor;
        private Color _shadowColor;
        private bool _critical;

        private void Awake()
        {
            EnsureMeshes();
            gameObject.SetActive(false);
        }

        public void Show(DamageResult result, Action<DamageNumberView> release)
        {
            EnsureMeshes();
            _release = release;
            _elapsed = 0f;
            _critical = result.isCritical;
            _velocity = _critical ? _criticalVelocity : _normalVelocity;
            _mainColor = _critical ? new Color(1f, 0.2f, 0.2f, 1f) : Color.white;
            _shadowColor = _critical ? new Color(0.4f, 0.05f, 0.05f, 1f) : new Color(0.2f, 0.2f, 0.2f, 1f);

            string text = NumberShortFormatter.Format(result.finalDamage);
            _mainText.text = text;
            _shadowText.text = text;
            _mainText.color = _mainColor;
            _shadowText.color = _shadowColor;
            transform.position = result.hitPosition;
            transform.localScale = Vector3.one * (_critical ? 1.22f : 1f);
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            transform.position += _velocity * Time.deltaTime;

            float scalePulse = _critical ? 1f + Mathf.Sin(t * Mathf.PI * 4f) * 0.08f * (1f - t) : 1f + Mathf.Sin(t * Mathf.PI) * 0.03f;
            transform.localScale = Vector3.one * ((_critical ? 1.22f : 1f) * scalePulse);

            Color main = _mainColor;
            main.a = 1f - t;
            _mainText.color = main;

            Color shadow = _shadowColor;
            shadow.a = 1f - t;
            _shadowText.color = shadow;

            if (_elapsed >= _duration)
            {
                _release?.Invoke(this);
            }
        }

        private void EnsureMeshes()
        {
            if (_mainText == null)
            {
                _mainText = CreateTextMesh("Main", Vector3.zero, Color.white);
            }

            if (_shadowText == null)
            {
                _shadowText = CreateTextMesh("Shadow", new Vector3(0.035f, -0.035f, 0.05f), Color.black);
            }
        }

        private TextMesh CreateTextMesh(string name, Vector3 localPosition, Color color)
        {
            Transform child = transform.Find(name);
            TextMesh mesh;
            if (child == null)
            {
                GameObject go = new GameObject(name);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = localPosition;
                mesh = go.AddComponent<TextMesh>();
            }
            else
            {
                child.localPosition = localPosition;
                mesh = child.GetComponent<TextMesh>();
                if (mesh == null)
                {
                    mesh = child.gameObject.AddComponent<TextMesh>();
                }
            }

            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.characterSize = 0.12f;
            mesh.fontSize = 42;
            mesh.color = color;
            mesh.text = "0";
            return mesh;
        }
    }
}
