using System.Collections.Generic;

namespace KitchenCaravan.Caravan
{
    // Centralizes deterministic reindexing after a segment is removed.
    public static class CaravanCollapseSystem
    {
        public static void RemoveSegment(List<SegmentController> segments, SegmentController removedSegment)
        {
            if (segments == null || removedSegment == null)
            {
                return;
            }

            segments.Remove(removedSegment);
            ReindexSegments(segments);
        }

        public static void ReindexSegments(List<SegmentController> segments)
        {
            if (segments == null)
            {
                return;
            }

            for (int i = 0; i < segments.Count; i++)
            {
                if (segments[i] == null)
                {
                    continue;
                }

                segments[i].SetSegmentIndex(i + 1);
            }
        }
    }
}
