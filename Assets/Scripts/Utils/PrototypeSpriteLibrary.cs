using UnityEngine;

namespace KitchenCaravan.Utils
{
    // Creates tiny runtime sprites so the prototype does not depend on imported art to stay playable.
    public static class PrototypeSpriteLibrary
    {
        private static Sprite _whiteSquare;

        public static Sprite WhiteSquare
        {
            get
            {
                if (_whiteSquare == null)
                {
                    Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                    {
                        name = "prototype_white_square_texture",
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp
                    };
                    texture.SetPixel(0, 0, Color.white);
                    texture.Apply();
                    _whiteSquare = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
                    _whiteSquare.name = "prototype_white_square_sprite";
                }

                return _whiteSquare;
            }
        }
    }
}