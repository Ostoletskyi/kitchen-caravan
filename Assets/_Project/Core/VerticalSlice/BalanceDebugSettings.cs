using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public static class BalanceDebugSettings
    {
        private static bool s_initialized;

        public static float PlayerFireRate { get; private set; }
        public static int BulletDamage { get; private set; }
        public static int ChainSegmentBaseHp { get; private set; }
        public static int ChainSegmentHpIncrement { get; private set; }
        public static int ChainLength { get; private set; }
        public static float ChainMoveSpeed { get; private set; }
        public static float ChainSpawnDelay { get; private set; }
        public static float PlayerMoveSpeed { get; private set; }

        public static void EnsureDefaults()
        {
            if (s_initialized)
            {
                return;
            }

            s_initialized = true;
            PlayerFireRate = 2f;
            BulletDamage = 1;
            ChainSegmentBaseHp = 3;
            ChainSegmentHpIncrement = 2;
            ChainLength = 6;
            ChainMoveSpeed = 1.8f;
            ChainSpawnDelay = 3f;
            PlayerMoveSpeed = 8f;
        }

        public static void SetPlayerFireRate(float value)
        {
            PlayerFireRate = Mathf.Max(0.1f, value);
        }

        public static void SetBulletDamage(float value)
        {
            BulletDamage = Mathf.Max(1, Mathf.RoundToInt(value));
        }

        public static void SetSegmentBaseHp(float value)
        {
            ChainSegmentBaseHp = Mathf.Max(1, Mathf.RoundToInt(value));
        }

        public static void SetSegmentIncrement(float value)
        {
            ChainSegmentHpIncrement = Mathf.Max(0, Mathf.RoundToInt(value));
        }

        public static void SetChainLength(float value)
        {
            ChainLength = Mathf.Max(1, Mathf.RoundToInt(value));
        }

        public static void SetChainMoveSpeed(float value)
        {
            ChainMoveSpeed = Mathf.Max(0.2f, value);
        }

        public static void SetSpawnDelay(float value)
        {
            ChainSpawnDelay = Mathf.Max(0.2f, value);
        }

        public static void SetPlayerMoveSpeed(float value)
        {
            PlayerMoveSpeed = Mathf.Max(0.5f, value);
        }
    }
}
