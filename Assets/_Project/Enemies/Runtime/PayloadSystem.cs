using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public static class PayloadSystem
    {
        public static float GetCargoFactor(CaravanRuntimeSettings settings, CaravanSegmentRuntimeData segmentData)
        {
            if (segmentData.isChestCarrier || segmentData.payloadType == CaravanPayloadType.ChestPayload)
            {
                return Mathf.Max(0.1f, settings.chestPayloadHpMultiplier);
            }

            if (segmentData.payloadType == CaravanPayloadType.HeavyPayload ||
                segmentData.payloadType == CaravanPayloadType.Bacon ||
                segmentData.payloadType == CaravanPayloadType.Meat ||
                segmentData.payloadType == CaravanPayloadType.Egg)
            {
                return Mathf.Max(0.1f, settings.heavyPayloadHpMultiplier);
            }

            return Mathf.Max(0.1f, settings.normalPayloadHpMultiplier);
        }
    }
}
