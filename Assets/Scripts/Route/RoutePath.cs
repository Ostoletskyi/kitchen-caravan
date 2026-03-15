using System.Collections.Generic;
using UnityEngine;

namespace KitchenCaravan.Route
{
    // Stores the fixed ordered route points for Prototype Level 1 and draws them in Scene view.
    public sealed class RoutePath : MonoBehaviour
    {
        [SerializeField] private List<Vector3> _localPoints = new List<Vector3>
        {
            new Vector3(-4.6f, 6.4f, 0f),
            new Vector3(4.4f, 6.4f, 0f),
            new Vector3(4.4f, 4.6f, 0f),
            new Vector3(-4.4f, 4.6f, 0f),
            new Vector3(-4.4f, 2.8f, 0f),
            new Vector3(4.4f, 2.8f, 0f),
            new Vector3(4.4f, 1.0f, 0f),
            new Vector3(-4.4f, 1.0f, 0f),
            new Vector3(-4.4f, -0.8f, 0f),
            new Vector3(4.4f, -0.8f, 0f),
            new Vector3(4.4f, -2.6f, 0f),
            new Vector3(-2.2f, -2.6f, 0f),
            new Vector3(0f, -5.6f, 0f)
        };
        [SerializeField] private bool _drawGizmos = true;
        [SerializeField] private Color _routeColor = new Color(1f, 0.75f, 0.22f, 1f);
        [SerializeField] private Color _pointColor = new Color(0.25f, 0.9f, 1f, 1f);
        [SerializeField] private Color _startColor = new Color(0.35f, 1f, 0.45f, 1f);
        [SerializeField] private Color _goalColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private float _gizmoPointRadius = 0.12f;

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

        private void Reset()
        {
            transform.position = Vector3.zero;
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos || _localPoints == null || _localPoints.Count == 0)
            {
                return;
            }

            Gizmos.color = _pointColor;
            for (int i = 0; i < _localPoints.Count; i++)
            {
                Gizmos.DrawSphere(transform.TransformPoint(_localPoints[i]), _gizmoPointRadius);
            }

            Gizmos.color = _routeColor;
            for (int i = 1; i < _localPoints.Count; i++)
            {
                Gizmos.DrawLine(transform.TransformPoint(_localPoints[i - 1]), transform.TransformPoint(_localPoints[i]));
            }

            Gizmos.color = _startColor;
            Gizmos.DrawSphere(transform.TransformPoint(_localPoints[0]), _gizmoPointRadius * 1.8f);
            Gizmos.color = _goalColor;
            Gizmos.DrawSphere(transform.TransformPoint(_localPoints[_localPoints.Count - 1]), _gizmoPointRadius * 2.1f);
        }
    }
}