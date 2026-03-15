using UnityEngine;

namespace KitchenCaravan.Caravan
{
    // Prototype Level 1 caravan settings. Keep this small and inspector-friendly.
    [CreateAssetMenu(menuName = "KitchenCaravan/Prototype/Caravan Config", fileName = "PrototypeLevel01CaravanConfig")]
    public sealed class CaravanConfig : ScriptableObject
    {
        [Min(0.1f)] public float moveSpeed = 2.5f;
        [Min(0.1f)] public float segmentSpacing = 1.2f;
        [Min(1f)] public float positionSmoothness = 12f;
        [Min(1f)] public float rotationSmoothness = 10f;
        [Min(1)] public int initialSegmentCount = 8;
        [Min(1)] public int baseHP = 20;
        [Min(0f)] public float positionGrowth = 0.25f;
        [Min(1)] public int captainHP = 60;
        [Min(0f)] public float initialCaptainDistance = 8.4f;
    }
}
