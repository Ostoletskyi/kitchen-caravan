using System.Collections.Generic;
using UnityEngine;

namespace KitchenCaravan.Caravan
{
    // Authoring component that stores the fixed ordered route for a level.
    public sealed class RoutePath : MonoBehaviour
    {
        [SerializeField] private List<Vector3> _localPoints = new List<Vector3>
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 6f, 0f),
            new Vector3(4f, 10f, 0f)
        };
        [SerializeField] private bool _drawRoute = true;
        [SerializeField] private Color _routeColor = new Color(0.95f, 0.75f, 0.25f, 1f);
        [SerializeField] private Color _pointColor = new Color(0.25f, 0.95f, 0.85f, 1f);
        [SerializeField] private float _pointRadius = 0.18f;

        public IReadOnlyList<Vector3> LocalPoints => _localPoints;
        public int PointCount => _localPoints != null ? _localPoints.Count : 0;

        public Vector3 GetWorldPoint(int index)
        {
            if (_localPoints == null || _localPoints.Count == 0)
            {
                return transform.position;
            }

            index = Mathf.Clamp(index, 0, _localPoints.Count - 1);
            return transform.TransformPoint(_localPoints[index]);
        }

        private void OnDrawGizmos()
        {
            if (!_drawRoute || _localPoints == null || _localPoints.Count == 0)
            {
                return;
            }

            Gizmos.color = _pointColor;
            for (int i = 0; i < _localPoints.Count; i++)
            {
                Vector3 point = transform.TransformPoint(_localPoints[i]);
                Gizmos.DrawSphere(point, _pointRadius);
            }

            Gizmos.color = _routeColor;
            for (int i = 1; i < _localPoints.Count; i++)
            {
                Vector3 from = transform.TransformPoint(_localPoints[i - 1]);
                Vector3 to = transform.TransformPoint(_localPoints[i]);
                Gizmos.DrawLine(from, to);
            }
        }
    }
}
