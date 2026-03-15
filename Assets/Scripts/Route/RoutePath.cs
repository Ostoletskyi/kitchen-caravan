using System.Collections.Generic;
using UnityEngine;

namespace KitchenCaravan.Route
{
    // Stores the fixed ordered route points for Prototype Level 1 and draws them in Scene view.
    public sealed class RoutePath : MonoBehaviour
    {
        [SerializeField] private bool _autoGenerateSerpentine = true;
        [SerializeField] private int _zigZagRows = 6;
        [SerializeField] private float _leftX = -4.6f;
        [SerializeField] private float _rightX = 4.4f;
        [SerializeField] private float _topY = 6.4f;
        [SerializeField] private float _rowStep = 1.8f;
        [SerializeField] private float _goalY = -5.6f;
        [SerializeField] private List<Vector3> _localPoints = new List<Vector3>();
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
            EnsureRoute();
            if (_localPoints == null || _localPoints.Count == 0)
            {
                return transform.position;
            }

            index = Mathf.Clamp(index, 0, _localPoints.Count - 1);
            return transform.TransformPoint(_localPoints[index]);
        }

        private void Awake()
        {
            EnsureRoute();
        }

        private void Reset()
        {
            transform.position = Vector3.zero;
            GenerateSerpentineRoute();
        }

        private void OnValidate()
        {
            if (_autoGenerateSerpentine)
            {
                GenerateSerpentineRoute();
            }
        }

        private void OnDrawGizmos()
        {
            EnsureRoute();
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

        private void EnsureRoute()
        {
            if ((_localPoints == null || _localPoints.Count < 2) && _autoGenerateSerpentine)
            {
                GenerateSerpentineRoute();
            }
        }

        private void GenerateSerpentineRoute()
        {
            if (_localPoints == null)
            {
                _localPoints = new List<Vector3>();
            }
            else
            {
                _localPoints.Clear();
            }

            int rows = Mathf.Clamp(_zigZagRows, 6, 8);
            float currentY = _topY;
            _localPoints.Add(new Vector3(_leftX, currentY, 0f));
            bool moveRight = true;
            for (int row = 0; row < rows; row++)
            {
                float targetX = moveRight ? _rightX : _leftX;
                _localPoints.Add(new Vector3(targetX, currentY, 0f));
                if (row == rows - 1)
                {
                    break;
                }

                currentY -= _rowStep;
                _localPoints.Add(new Vector3(targetX, currentY, 0f));
                moveRight = !moveRight;
            }

            float finalApproachY = Mathf.Min(currentY - _rowStep, -2.6f);
            float finalApproachX = moveRight ? _leftX : _rightX;
            _localPoints.Add(new Vector3(finalApproachX * 0.5f, finalApproachY, 0f));
            _localPoints.Add(new Vector3(0f, _goalY, 0f));
        }
    }
}