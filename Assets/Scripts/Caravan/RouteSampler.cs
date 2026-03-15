using System.Collections.Generic;
using UnityEngine;

namespace KitchenCaravan.Caravan
{
    // Caches route points and cumulative lengths so path sampling stays allocation-free at runtime.
    public sealed class RouteSampler
    {
        private readonly List<Vector3> _worldPoints = new List<Vector3>(64);
        private readonly List<float> _cumulativeDistances = new List<float>(64);
        private float _routeLength;
        private Vector3 _startDirection = Vector3.up;
        private Vector3 _endDirection = Vector3.up;

        public float GetRouteLength()
        {
            return _routeLength;
        }

        public void Rebuild(RoutePath routePath)
        {
            _worldPoints.Clear();
            _cumulativeDistances.Clear();
            _routeLength = 0f;
            _startDirection = Vector3.up;
            _endDirection = Vector3.up;

            if (routePath == null || routePath.PointCount <= 0)
            {
                return;
            }

            for (int i = 0; i < routePath.PointCount; i++)
            {
                _worldPoints.Add(routePath.GetWorldPoint(i));
            }

            if (_worldPoints.Count == 1)
            {
                _worldPoints.Add(_worldPoints[0] + Vector3.up);
            }

            _cumulativeDistances.Add(0f);
            for (int i = 1; i < _worldPoints.Count; i++)
            {
                _routeLength += Vector3.Distance(_worldPoints[i - 1], _worldPoints[i]);
                _cumulativeDistances.Add(_routeLength);
            }

            _startDirection = (_worldPoints[1] - _worldPoints[0]).normalized;
            if (_startDirection.sqrMagnitude <= 0.0001f)
            {
                _startDirection = Vector3.up;
            }

            _endDirection = (_worldPoints[_worldPoints.Count - 1] - _worldPoints[_worldPoints.Count - 2]).normalized;
            if (_endDirection.sqrMagnitude <= 0.0001f)
            {
                _endDirection = _startDirection;
            }
        }

        public Vector3 GetPointAtDistance(float distance)
        {
            if (_worldPoints.Count == 0)
            {
                return Vector3.zero;
            }

            if (distance <= 0f)
            {
                return _worldPoints[0];
            }

            if (distance >= _routeLength)
            {
                return _worldPoints[_worldPoints.Count - 1];
            }

            for (int i = 1; i < _cumulativeDistances.Count; i++)
            {
                if (distance > _cumulativeDistances[i])
                {
                    continue;
                }

                float segmentStartDistance = _cumulativeDistances[i - 1];
                float segmentLength = _cumulativeDistances[i] - segmentStartDistance;
                if (segmentLength <= 0.0001f)
                {
                    return _worldPoints[i];
                }

                float t = (distance - segmentStartDistance) / segmentLength;
                return Vector3.Lerp(_worldPoints[i - 1], _worldPoints[i], t);
            }

            return _worldPoints[_worldPoints.Count - 1];
        }

        public Vector3 GetDirectionAtDistance(float distance)
        {
            if (_worldPoints.Count == 0)
            {
                return Vector3.up;
            }

            if (distance <= 0f)
            {
                return _startDirection;
            }

            if (distance >= _routeLength)
            {
                return _endDirection;
            }

            for (int i = 1; i < _cumulativeDistances.Count; i++)
            {
                if (distance > _cumulativeDistances[i])
                {
                    continue;
                }

                Vector3 direction = (_worldPoints[i] - _worldPoints[i - 1]).normalized;
                return direction.sqrMagnitude > 0.0001f ? direction : _startDirection;
            }

            return _endDirection;
        }
    }
}
