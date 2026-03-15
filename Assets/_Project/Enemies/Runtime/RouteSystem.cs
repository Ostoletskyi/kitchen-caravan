using System.Collections.Generic;
using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public struct RouteLayoutData
    {
        public List<Vector3> points;
        public List<float> cumulativeDistances;
        public float totalLength;
        public Vector3 startDirection;
        public Vector3 endDirection;
    }

    public static class RouteSystem
    {
        public static RouteLayoutData Build(EnemyRouteData routeData, Vector3 fallbackOrigin)
        {
            var points = new List<Vector3>();
            if (routeData != null && routeData.Points != null)
            {
                for (int i = 0; i < routeData.Points.Count; i++)
                {
                    points.Add(routeData.Points[i]);
                }
            }

            if (points.Count < 2)
            {
                Vector3 start = points.Count == 1 ? points[0] : fallbackOrigin;
                points.Clear();
                points.Add(start);
                points.Add(start + Vector3.right * 8f);
            }

            var cumulativeDistances = new List<float> { 0f };
            float totalLength = 0f;
            for (int i = 1; i < points.Count; i++)
            {
                totalLength += Vector3.Distance(points[i - 1], points[i]);
                cumulativeDistances.Add(totalLength);
            }

            Vector3 startDirection = (points[1] - points[0]).normalized;
            if (startDirection.sqrMagnitude <= 0.001f)
            {
                startDirection = Vector3.right;
            }

            Vector3 endDirection = (points[points.Count - 1] - points[points.Count - 2]).normalized;
            if (endDirection.sqrMagnitude <= 0.001f)
            {
                endDirection = startDirection;
            }

            return new RouteLayoutData
            {
                points = points,
                cumulativeDistances = cumulativeDistances,
                totalLength = totalLength,
                startDirection = startDirection,
                endDirection = endDirection
            };
        }

        public static Vector3 SamplePosition(RouteLayoutData layout, float distance, Vector3 fallbackOrigin)
        {
            if (layout.points == null || layout.points.Count == 0)
            {
                return fallbackOrigin;
            }

            if (distance <= 0f)
            {
                return layout.points[0] + layout.startDirection * distance;
            }

            if (distance >= layout.totalLength)
            {
                return layout.points[layout.points.Count - 1] + layout.endDirection * (distance - layout.totalLength);
            }

            for (int i = 1; i < layout.cumulativeDistances.Count; i++)
            {
                if (distance > layout.cumulativeDistances[i])
                {
                    continue;
                }

                float segmentStartDistance = layout.cumulativeDistances[i - 1];
                float segmentLength = layout.cumulativeDistances[i] - segmentStartDistance;
                if (segmentLength <= 0.0001f)
                {
                    return layout.points[i];
                }

                float t = (distance - segmentStartDistance) / segmentLength;
                return Vector3.Lerp(layout.points[i - 1], layout.points[i], t);
            }

            return layout.points[layout.points.Count - 1];
        }
    }
}
