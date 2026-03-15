using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public static class SegmentHealthSystem
    {
        public static int Evaluate(CaravanRuntimeSettings settings, int segmentIndex, CaravanSegmentRuntimeData segmentData)
        {
            float levelFactor = 1f + (Mathf.Max(1, settings.levelNumber) - 1) * Mathf.Max(0f, settings.segmentLevelGrowth);
            float positionFactor = 1f + (Mathf.Max(1, segmentIndex) - 1) * Mathf.Max(0f, settings.segmentPositionGrowth);
            float cargoFactor = PayloadSystem.GetCargoFactor(settings, segmentData);
            float hp = Mathf.Max(1f, settings.segmentBaseHp) * levelFactor * positionFactor * cargoFactor;
            return Mathf.Max(1, Mathf.RoundToInt(hp));
        }
    }
}
