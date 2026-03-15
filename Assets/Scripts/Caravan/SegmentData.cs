using System;

namespace KitchenCaravan.Caravan
{
    public enum SegmentPayloadType
    {
        Bread = 0,
        Cheese = 1,
        Tomato = 2,
        Cucumber = 3,
        Bacon = 4,
        Egg = 5
    }

    // Shared gameplay data for one prototype caravan segment.
    [Serializable]
    public sealed class SegmentData
    {
        public int SegmentIndex;
        public int CurrentHP;
        public int MaxHP;
        public SegmentPayloadType PayloadType;
        public bool IsChestCarrier;
        public float DistanceOnPath;

        public SegmentData Clone()
        {
            return new SegmentData
            {
                SegmentIndex = SegmentIndex,
                CurrentHP = CurrentHP,
                MaxHP = MaxHP,
                PayloadType = PayloadType,
                IsChestCarrier = IsChestCarrier,
                DistanceOnPath = DistanceOnPath
            };
        }
    }
}
