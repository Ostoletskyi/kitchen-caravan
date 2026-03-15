using System.Collections.Generic;
using UnityEngine;

namespace KitchenCaravan.Route
{
    // Stores the fixed ordered route points for Prototype Level 1 and draws them in Scene view.
    public sealed class RoutePath : MonoBehaviour
    {
        [SerializeField] private List<Vector3> _localPoints = new List<Vector3>
        {
            new Vector3(-2.5f, 6.5f, 0f),
            new Vector3(-2.5f, 3f, 0f),
            new Vector3(2.25f, 1.5f, 0f),
            new Vector3(2.25f, -1.5f, 0f),
            new Vector3(-1.5f, -3f, 0f)
        };
        [SerializeField] private bool _drawGizmos = true;
        [SerializeField] private Color _routeColor = new Color(0.95f, 0.75f, 0.25f, 1f);
        [SerializeField] private Color _pointColor = new Color(0.2f, 0.9f, 1f, 1f);

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
            if (!_drawGizmos || _localPoints == null || _localPoints.Count == 0)
            {
                return;
            }

            Gizmos.color = _pointColor;
            for (int i = 0; i < _localPoints.Count; i++)
            {
                Gizmos.DrawSphere(transform.TransformPoint(_localPoints[i]), 0.12f);
            }

            Gizmos.color = _routeColor;
            for (int i = 1; i < _localPoints.Count; i++)
            {
                Gizmos.DrawLine(transform.TransformPoint(_localPoints[i - 1]), transform.TransformPoint(_localPoints[i]));
            }
        }
    }
}
