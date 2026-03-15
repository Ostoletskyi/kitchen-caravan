using UnityEngine;

namespace KitchenCaravan.Utils
{
    // Formats combat numbers and future larger values using K/M/B/T suffixes.
    public static class NumberFormatUtil
    {
        private static readonly string[] Suffixes = { "", "K", "M", "B", "T" };

        public static string Format(int value)
        {
            long absolute = System.Math.Abs((long)value);
            if (absolute < 1000)
            {
                return value.ToString();
            }

            double scaled = value;
            int suffixIndex = 0;
            while (System.Math.Abs(scaled) >= 1000d && suffixIndex < Suffixes.Length - 1)
            {
                scaled /= 1000d;
                suffixIndex++;
            }

            return scaled.ToString(System.Math.Abs(scaled) >= 10d ? "0.#" : "0.#") + Suffixes[suffixIndex];
        }
    }
}
