using System.Collections.Generic;
using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    [CreateAssetMenu(menuName = "KitchenCaravan/Levels/Enemy Route", fileName = "EnemyRouteData")]
    public class EnemyRouteData : ScriptableObject
    {
        [SerializeField] private int _routeId = 1;
        [SerializeField] private List<Vector3> _points = new List<Vector3>();

        public int RouteId => _routeId;
        public IReadOnlyList<Vector3> Points => _points;

        public void SetRouteId(int routeId)
        {
            _routeId = Mathf.Max(1, routeId);
        }

        public void SetPoints(IReadOnlyList<Vector3> points)
        {
            _points.Clear();
            if (points == null)
            {
                return;
            }

            for (int i = 0; i < points.Count; i++)
            {
                _points.Add(points[i]);
            }
        }
    }
}
