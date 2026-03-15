using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public static class CaravanCollapseSystem
    {
        public static float CollapseCaptainDistance(float currentCaptainDistance, float spacing)
        {
            return Mathf.Max(0f, currentCaptainDistance - Mathf.Max(0.2f, spacing));
        }
    }
}
