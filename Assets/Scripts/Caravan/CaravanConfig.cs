using UnityEngine;

namespace KitchenCaravan.Caravan
{
    // Inspector-tuned config for the Level 1 caravan prototype.
    [CreateAssetMenu(menuName = "KitchenCaravan/Prototype/Caravan Config", fileName = "PrototypeLevel01CaravanConfig")]
    public sealed class CaravanConfig : ScriptableObject
    {
        [Header("Movement")]
        [Min(0.25f)] public float moveSpeed = 2f;
        [Min(0.2f)] public float segmentSpacing = 0.95f;
        [Min(1f)] public float positionSmoothness = 14f;
        [Min(1f)] public float rotationSmoothness = 12f;
        [Min(0f)] public float initialCaptainDistance = 0f;
        [Min(0.1f)] public float rageInterval = 10f;
        [Min(0.1f)] public float rageDuration = 2f;
        [Min(1f)] public float rageSpeedMultiplier = 2f;

        [Header("Composition")]
        [Range(8, 10)] public int initialSegmentCount = 8;
        [Min(1)] public int captainHP = 70;
        [Min(1)] public int baseHP = 20;
        [Min(0f)] public float positionGrowth = 0.25f;

        [Header("Combat Feel")]
        [Min(0f)] public float destructionPause = 0.02f;
        [Min(0f)] public float deathFlashScale = 0.85f;

        public int GetSegmentHp(int segmentIndex)
        {
            float factor = 1f + Mathf.Max(0, segmentIndex - 1) * positionGrowth;
            return Mathf.Max(1, Mathf.RoundToInt(baseHP * factor));
        }
    }
}