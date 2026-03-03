using UnityEngine;

namespace KitchenCaravan.Enemies.Worm
{
    public sealed class WormFloatingText : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float _duration = 0.7f;
        [SerializeField] private Vector3 _velocity = new Vector3(0f, 1.3f, 0f);

        private TextMesh _textMesh;
        private Color _startColor;
        private float _elapsed;

        public static void Spawn(Vector3 position, string text, Color color)
        {
            GameObject go = new GameObject("WormFloatingText");
            go.transform.position = position;

            TextMesh mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.characterSize = 0.1f;
            mesh.fontSize = 32;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color;

            WormFloatingText floating = go.AddComponent<WormFloatingText>();
            floating._textMesh = mesh;
            floating._startColor = color;
        }

        private void Awake()
        {
            if (_textMesh == null)
            {
                _textMesh = GetComponent<TextMesh>();
                if (_textMesh != null)
                {
                    _startColor = _textMesh.color;
                }
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            transform.position += _velocity * Time.deltaTime;

            if (_textMesh != null)
            {
                Color color = _startColor;
                color.a = 1f - t;
                _textMesh.color = color;
            }

            if (_elapsed >= _duration)
            {
                Destroy(gameObject);
            }
        }
    }
}
