using UnityEngine;
using KitchenCaravan.Utils;

namespace KitchenCaravan.Combat
{
    // Small self-cleaning placeholder flash used for hit and destruction feedback.
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class TemporaryHitFlash : MonoBehaviour
    {
        [SerializeField] private float _lifeTime = 0.16f;
        private float _elapsed;
        private SpriteRenderer _renderer;

        public static void Spawn(Vector3 position, Color color, float scale)
        {
            GameObject go = new GameObject("TemporaryHitFlash");
            go.transform.position = position;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = PrototypeSpriteLibrary.WhiteSquare;
            renderer.color = color;
            renderer.sortingOrder = 40;
            go.AddComponent<TemporaryHitFlash>();
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer != null && _renderer.sprite == null)
            {
                _renderer.sprite = PrototypeSpriteLibrary.WhiteSquare;
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _lifeTime);
            transform.localScale *= 1f + Time.deltaTime * 2.2f;
            if (_renderer != null)
            {
                Color color = _renderer.color;
                color.a = 1f - t;
                _renderer.color = color;
            }

            if (_elapsed >= _lifeTime)
            {
                Destroy(gameObject);
            }
        }
    }
}