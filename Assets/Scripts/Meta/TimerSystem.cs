using System;

namespace KitchenCaravan.Meta
{
    public sealed class TimerSystem
    {
        public long GetUnixTimeNow()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public int GetElapsedWholeIntervals(long fromUnix, long toUnix, int intervalSeconds)
        {
            if (intervalSeconds <= 0 || toUnix <= fromUnix)
            {
                return 0;
            }

            return (int)((toUnix - fromUnix) / intervalSeconds);
        }
    }
}
