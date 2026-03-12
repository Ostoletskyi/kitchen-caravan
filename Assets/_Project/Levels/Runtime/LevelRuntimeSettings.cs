using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public static class LevelRuntimeSettings
    {
        public static int LevelNumber { get; private set; } = 1;
        public static int RouteId { get; private set; } = 1;
        public static EnemyRouteData RouteData { get; private set; }

        public static int ChainLength { get; private set; } = 24;
        public static int SegmentBaseHp { get; private set; } = 3;
        public static int SegmentHpIncrement { get; private set; } = 2;
        public static int CaptainHp { get; private set; } = 15;
        public static float ChainMoveSpeed { get; private set; } = 1.8f;
        public static float SpawnDelay { get; private set; } = 3f;
        public static float SegmentSpacing { get; private set; } = 0.85f;
        public static float FollowLerpSpeed { get; private set; } = 16f;
        public static float TrailStep { get; private set; } = 0.14f;
        public static float SwayAmplitude { get; private set; } = 1f;
        public static float SwayFrequency { get; private set; } = 1.2f;

        public static float PlayerMoveSpeed { get; private set; } = 8f;
        public static float PlayerFireRate { get; private set; } = 2f;
        public static int BulletDamage { get; private set; } = 1;

        public static void Apply(LevelConfig config)
        {
            if (config == null)
            {
                ApplyDefaults();
                return;
            }

            LevelNumber = config.LevelNumber;
            RouteId = config.RouteId;
            RouteData = config.RouteData;

            ChainLength = config.CaravanChainLength;
            SegmentBaseHp = config.SegmentBaseHp;
            SegmentHpIncrement = config.SegmentHpIncrement;
            CaptainHp = config.CaptainHp;
            ChainMoveSpeed = config.CaravanMovementSpeed;
            SpawnDelay = config.SpawnDelay;
            SegmentSpacing = config.SegmentSpacing;
            FollowLerpSpeed = config.FollowLerpSpeed;
            TrailStep = config.TrailStep;
            SwayAmplitude = config.SwayAmplitude;
            SwayFrequency = config.SwayFrequency;

            PlayerMoveSpeed = config.PlayerMoveSpeed;
            PlayerFireRate = config.PlayerFireRate;
            BulletDamage = config.BulletDamage;
        }

        private static void ApplyDefaults()
        {
            LevelNumber = 1;
            RouteId = 1;
            RouteData = null;
            ChainLength = 24;
            SegmentBaseHp = 3;
            SegmentHpIncrement = 2;
            CaptainHp = 15;
            ChainMoveSpeed = 1.8f;
            SpawnDelay = 3f;
            SegmentSpacing = 0.85f;
            FollowLerpSpeed = 16f;
            TrailStep = 0.14f;
            SwayAmplitude = 1f;
            SwayFrequency = 1.2f;
            PlayerMoveSpeed = 8f;
            PlayerFireRate = 2f;
            BulletDamage = 1;
        }
    }
}
