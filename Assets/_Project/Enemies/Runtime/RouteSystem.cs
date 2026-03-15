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
            var points = BuildSerpentinePoints();
            if (points.Count < 2)
            {
                Vector3 start = fallbackOrigin;
                points.Clear();
                points.Add(start);
                points.Add(start + Vector3.down * 8f);
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
                return layout.points[layout.points.Count - 1];
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

        private static List<Vector3> BuildSerpentinePoints()
        {
            Camera camera = Camera.main;
            float halfHeight = camera != null ? camera.orthographicSize : 5.5f;
            float halfWidth = camera != null ? halfHeight * camera.aspect : 5.2f;

            float routeHalfWidth = Mathf.Max(halfWidth * 0.74f, 2.4f);
            float leftX = -routeHalfWidth;
            float rightX = routeHalfWidth;
            float topY = halfHeight * 0.84f;
            float bottomY = -halfHeight * 0.78f;
            int rows = 7;
            float finalApproachY = Mathf.Lerp(topY, bottomY, 0.88f);
            float rowStep = (topY - finalApproachY) / Mathf.Max(1, rows - 1);

            var points = new List<Vector3>(rows * 2 + 2);
            float y = topY;
            bool goRight = true;
            points.Add(new Vector3(leftX, y, 0f));
            for (int row = 0; row < rows; row++)
            {
                float x = goRight ? rightX : leftX;
                points.Add(new Vector3(x, y, 0f));
                if (row == rows - 1)
                {
                    break;
                }

                y -= rowStep;
                points.Add(new Vector3(x, y, 0f));
                goRight = !goRight;
            }

            points.Add(new Vector3(0f, Mathf.Lerp(y, bottomY, 0.45f), 0f));
            points.Add(new Vector3(0f, bottomY, 0f));
            return points;
        }
    }
}
