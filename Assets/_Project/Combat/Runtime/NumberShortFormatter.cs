using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public static class NumberShortFormatter
    {
        private static readonly string[] Suffixes = { "", "K", "M", "B", "T" };

        public static string Format(int value)
        {
            long absolute = Mathf.Abs(value);
            if (absolute < 1000)
            {
                return value.ToString();
            }

            double shortValue = value;
            int suffixIndex = 0;
            while (Mathf.Abs((float)shortValue) >= 1000f && suffixIndex < Suffixes.Length - 1)
            {
                shortValue /= 1000d;
                suffixIndex++;
            }

            string format = Mathf.Abs((float)shortValue) >= 10f ? "0.#" : "0.#";
            return shortValue.ToString(format) + Suffixes[suffixIndex];
        }
    }
}
