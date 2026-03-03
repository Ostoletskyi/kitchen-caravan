using UnityEngine;

namespace KitchenCaravan.Enemies.Worm
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class WormVisual2D : MonoBehaviour
    {
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private Vector2 _size = new Vector2(0.8f, 0.8f);

        private SpriteRenderer _renderer;
        private static Sprite _sharedSprite;

        private void Awake()
        {
            EnsureRenderer();
            Apply();
        }

        public void SetVisual(Color color, Vector2 size)
        {
            _color = color;
            _size = size;
            Apply();
        }

        public void SetColor(Color color)
        {
            _color = color;
            Apply();
        }

        private void EnsureRenderer()
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<SpriteRenderer>();
            }

            if (_renderer == null)
            {
                _renderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (_renderer.sprite == null)
            {
                _renderer.sprite = GetSharedSprite();
            }
        }

        private void Apply()
        {
            EnsureRenderer();
            _renderer.color = _color;
            transform.localScale = new Vector3(_size.x, _size.y, 1f);
        }

        private static Sprite GetSharedSprite()
        {
            if (_sharedSprite != null)
            {
                return _sharedSprite;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            _sharedSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            _sharedSprite.hideFlags = HideFlags.HideAndDontSave;
            return _sharedSprite;
        }
    }
}
