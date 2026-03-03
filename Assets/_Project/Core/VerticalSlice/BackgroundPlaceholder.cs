using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BackgroundPlaceholder : MonoBehaviour
    {
        [SerializeField] private Color _color = new Color(0.15f, 0.2f, 0.15f, 1f);
        [SerializeField] private Vector2 _size = new Vector2(30f, 20f);

        private void Awake()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = gameObject.AddComponent<SpriteRenderer>();
            }

            sr.sprite = RuntimeSpriteFactory.WhiteSquare;
            sr.color = _color;
            transform.position = new Vector3(0f, 0f, 5f);
            transform.localScale = new Vector3(_size.x, _size.y, 1f);
        }
    }
}
