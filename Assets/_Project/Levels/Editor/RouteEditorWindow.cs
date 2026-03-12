using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace KitchenCaravan.VerticalSlice.Editor
{
    public class RouteEditorWindow : EditorWindow
    {
        private const string DefaultRouteFolder = "Assets/_Project/Levels/Routes";

        private readonly List<Vector3> _points = new List<Vector3>();
        private EnemyRouteData _routeAsset;
        private int _routeId = 1;
        private bool _sceneDrawMode;
        private Vector2 _scroll;

        [MenuItem("Tools/KitchenCaravan/Route Editor")]
        public static void Open()
        {
            var window = GetWindow<RouteEditorWindow>("Route Editor");
            window.minSize = new Vector2(420f, 420f);
            window.Show();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Caravan Route Editor", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Toggle Draw Mode and click in Scene view to place points on XY plane.", MessageType.Info);
            EditorGUILayout.Space();

            _routeId = Mathf.Max(1, EditorGUILayout.IntField("Route ID", _routeId));
            _routeAsset = (EnemyRouteData)EditorGUILayout.ObjectField("Route Asset", _routeAsset, typeof(EnemyRouteData), false);
            _sceneDrawMode = EditorGUILayout.ToggleLeft("Draw Mode (Scene Click)", _sceneDrawMode);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("New Route"))
                {
                    NewRoute();
                }

                if (GUILayout.Button("Load Route"))
                {
                    LoadRoute();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Point"))
                {
                    AddPoint();
                }

                if (GUILayout.Button("Remove Last Point"))
                {
                    RemoveLastPoint();
                }
            }

            if (GUILayout.Button("Save Route"))
            {
                SaveRoute();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Point Count: {_points.Count}", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(180f));
            for (int i = 0; i < _points.Count; i++)
            {
                _points[i] = EditorGUILayout.Vector3Field($"P{i + 1}", _points[i]);
            }
            EditorGUILayout.EndScrollView();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            DrawPreview();
            if (!_sceneDrawMode)
            {
                return;
            }

            Event e = Event.current;
            if (e == null || e.type != EventType.MouseDown || e.button != 0 || e.alt)
            {
                return;
            }

            Vector3 worldPoint = ProjectMouseToXY(e.mousePosition);
            Undo.RecordObject(this, "Add Route Point");
            _points.Add(worldPoint);
            Repaint();
            e.Use();
        }

        private void DrawPreview()
        {
            if (_points.Count == 0)
            {
                return;
            }

            Handles.color = Color.cyan;
            for (int i = 0; i < _points.Count; i++)
            {
                Handles.SphereHandleCap(0, _points[i], Quaternion.identity, 0.15f, EventType.Repaint);
                Handles.Label(_points[i] + Vector3.up * 0.15f, (i + 1).ToString());
            }

            if (_points.Count > 1)
            {
                Handles.DrawAAPolyLine(4f, _points.ToArray());
            }
        }

        private static Vector3 ProjectMouseToXY(Vector2 mousePosition)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            Plane plane = new Plane(Vector3.forward, Vector3.zero);
            if (plane.Raycast(ray, out float enter))
            {
                return ray.GetPoint(enter);
            }

            return Vector3.zero;
        }

        private void NewRoute()
        {
            _points.Clear();
            _routeAsset = null;
            _routeId = Mathf.Max(1, _routeId);
        }

        private void AddPoint()
        {
            if (_points.Count == 0)
            {
                _points.Add(Vector3.zero);
                return;
            }

            _points.Add(_points[_points.Count - 1] + Vector3.down);
        }

        private void RemoveLastPoint()
        {
            if (_points.Count == 0)
            {
                return;
            }

            _points.RemoveAt(_points.Count - 1);
        }

        private void LoadRoute()
        {
            if (_routeAsset == null)
            {
                EditorUtility.DisplayDialog("Load Route", "Assign a route asset first.", "OK");
                return;
            }

            _routeId = _routeAsset.RouteId;
            _points.Clear();
            var sourcePoints = _routeAsset.Points;
            for (int i = 0; i < sourcePoints.Count; i++)
            {
                _points.Add(sourcePoints[i]);
            }

            Repaint();
        }

        private void SaveRoute()
        {
            if (_points.Count < 2)
            {
                EditorUtility.DisplayDialog("Save Route", "Add at least 2 route points.", "OK");
                return;
            }

            EnsureFolder(DefaultRouteFolder);
            if (_routeAsset == null)
            {
                string path = EditorUtility.SaveFilePanelInProject(
                    "Save Route",
                    $"EnemyRoute_{_routeId:000}",
                    "asset",
                    "Choose location for the route asset.",
                    DefaultRouteFolder);
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                _routeAsset = ScriptableObject.CreateInstance<EnemyRouteData>();
                AssetDatabase.CreateAsset(_routeAsset, path);
            }

            _routeAsset.SetRouteId(_routeId);
            _routeAsset.SetPoints(_points);
            EditorUtility.SetDirty(_routeAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = _routeAsset;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
            string name = Path.GetFileName(folderPath);

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
