using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public static class RuntimeSpriteFactory
    {
        private static Sprite s_cached;

        public static Sprite WhiteSquare
        {
            get
            {
                if (s_cached != null)
                {
                    return s_cached;
                }

                var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    name = "VS_White_1x1"
                };
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();

                s_cached = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
                s_cached.name = "VS_WhiteSprite";
                return s_cached;
            }
        }
    }
}
