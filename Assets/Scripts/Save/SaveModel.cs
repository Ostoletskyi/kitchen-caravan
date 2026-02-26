using System;

namespace KitchenCaravan.Save
{
    [Serializable]
    public class SaveModel
    {
        public const int Version = 1;

        public int version = Version;
        public string playerId;
        public int highScore;
        public long lastSaveUnix;
    }
}
