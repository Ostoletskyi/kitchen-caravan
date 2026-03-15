using UnityEngine;

namespace KitchenCaravan.Combat
{
    // Tiny self-destroying hit flash placeholder.
    public sealed class TemporaryHitFlash : MonoBehaviour
    {
        [SerializeField] private float _lifeTime = 0.18f;
        private float _elapsed;
        private SpriteRenderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _lifeTime);
            transform.localScale = Vector3.one * (0.32f + t * 0.28f);
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
