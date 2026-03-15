using System.Collections.Generic;
using UnityEngine;

namespace KitchenCaravan.Route
{
    // Samples a fixed route by distance so caravan movement stays deterministic and physics-free.
    public sealed class RouteSampler
    {
        private readonly List<Vector3> _points = new List<Vector3>(16);
        private readonly List<float> _cumulativeDistances = new List<float>(16);
        private float _routeLength;
        private Vector3 _startDirection = Vector3.down;
        private Vector3 _endDirection = Vector3.down;

        public void Rebuild(RoutePath routePath)
        {
            _points.Clear();
            _cumulativeDistances.Clear();
            _routeLength = 0f;
            _startDirection = Vector3.down;
            _endDirection = Vector3.down;

            if (routePath == null || routePath.PointCount <= 0)
            {
                return;
            }

            for (int i = 0; i < routePath.PointCount; i++)
            {
                _points.Add(routePath.GetWorldPoint(i));
            }

            if (_points.Count == 1)
            {
                _points.Add(_points[0] + Vector3.down);
            }

            _cumulativeDistances.Add(0f);
            for (int i = 1; i < _points.Count; i++)
            {
                _routeLength += Vector3.Distance(_points[i - 1], _points[i]);
                _cumulativeDistances.Add(_routeLength);
            }

            _startDirection = (_points[1] - _points[0]).normalized;
            if (_startDirection.sqrMagnitude <= 0.0001f)
            {
                _startDirection = Vector3.down;
            }

            _endDirection = (_points[_points.Count - 1] - _points[_points.Count - 2]).normalized;
            if (_endDirection.sqrMagnitude <= 0.0001f)
            {
                _endDirection = _startDirection;
            }
        }

        public float GetRouteLength()
        {
            return _routeLength;
        }

        public Vector3 GetPointAtDistance(float distance)
        {
            if (_points.Count == 0)
            {
                return Vector3.zero;
            }

            distance = Mathf.Clamp(distance, 0f, _routeLength);
            if (distance <= 0f)
            {
                return _points[0];
            }

            if (distance >= _routeLength)
            {
                return _points[_points.Count - 1];
            }

            int index = FindSegmentIndex(distance);
            float segmentStart = _cumulativeDistances[index - 1];
            float segmentLength = _cumulativeDistances[index] - segmentStart;
            if (segmentLength <= 0.0001f)
            {
                return _points[index];
            }

            float t = (distance - segmentStart) / segmentLength;
            return Vector3.Lerp(_points[index - 1], _points[index], t);
        }

        public Vector3 GetDirectionAtDistance(float distance)
        {
            if (_points.Count == 0)
            {
                return Vector3.down;
            }

            distance = Mathf.Clamp(distance, 0f, _routeLength);
            if (distance <= 0f)
            {
                return _startDirection;
            }

            if (distance >= _routeLength)
            {
                return _endDirection;
            }

            int index = FindSegmentIndex(distance);
            Vector3 direction = (_points[index] - _points[index - 1]).normalized;
            return direction.sqrMagnitude > 0.0001f ? direction : _startDirection;
        }

        private int FindSegmentIndex(float distance)
        {
            int low = 1;
            int high = _cumulativeDistances.Count - 1;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (distance <= _cumulativeDistances[mid])
                {
                    if (mid == 0 || distance > _cumulativeDistances[mid - 1])
                    {
                        return mid;
                    }

                    high = mid - 1;
                }
                else
                {
                    low = mid + 1;
                }
            }

            return _cumulativeDistances.Count - 1;
        }
    }
}