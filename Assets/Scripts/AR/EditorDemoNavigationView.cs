using AirportAR.Map;
using AirportAR.Navigation;
using UnityEngine;

namespace AirportAR.AR
{
    /// <summary>
    /// Editor fallback visualization of the simulated airport graph and active route.
    /// </summary>
    public class EditorDemoNavigationView : MonoBehaviour
    {
        [SerializeField] ARNavigationManager navigationManager;
        [SerializeField] Material nodeMaterial;
        [SerializeField] Material lineMaterial;
        [SerializeField] Material pathMaterial;
        [SerializeField] float nodeRadius = 0.35f;

        Transform graphRoot;
        LineRenderer[] connectionLines;
        GameObject[] nodeObjects;

        void Start()
        {
            graphRoot = new GameObject("DemoGraphView").transform;
            graphRoot.SetParent(transform, false);

            if (AirportMapLoader.Instance != null)
            {
                AirportMapLoader.Instance.MapChanged += RebuildGraphView;
            }

            RebuildGraphView();
        }

        void OnDestroy()
        {
            if (AirportMapLoader.Instance != null)
            {
                AirportMapLoader.Instance.MapChanged -= RebuildGraphView;
            }
        }

        void Update()
        {
            HighlightActivePath();
        }

        public void RebuildGraphView()
        {
            if (graphRoot == null)
            {
                return;
            }

            foreach (Transform child in graphRoot)
            {
                Destroy(child.gameObject);
            }

            AirportMapData map = AirportMapLoader.Instance?.CurrentMap;
            if (map?.zones == null)
            {
                return;
            }

            nodeObjects = new GameObject[map.zones.Length];
            for (int i = 0; i < map.zones.Length; i++)
            {
                AirportZone zone = map.zones[i];
                Vector3 pos = zone.position.ToVector3();
                var node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                node.name = $"Node_{zone.id}";
                node.transform.SetParent(graphRoot, false);
                node.transform.position = pos + Vector3.up * 0.2f;
                node.transform.localScale = Vector3.one * nodeRadius * 2f;

                var renderer = node.GetComponent<Renderer>();
                renderer.material = nodeMaterial != null ? nodeMaterial : renderer.material;
                renderer.material.color = new Color(0.7f, 0.75f, 0.82f);

                var label = new GameObject("Label").transform;
                label.SetParent(node.transform, false);
                label.localPosition = Vector3.up * 0.8f;

                nodeObjects[i] = node;
            }

            if (map.connections == null)
            {
                return;
            }

            connectionLines = new LineRenderer[map.connections.Length];
            for (int i = 0; i < map.connections.Length; i++)
            {
                AirportConnection conn = map.connections[i];
                AirportZone from = AirportMapLoader.Instance.GetZoneById(conn.from);
                AirportZone to = AirportMapLoader.Instance.GetZoneById(conn.to);
                if (from == null || to == null)
                {
                    continue;
                }

                var lineGo = new GameObject($"Conn_{conn.id}");
                lineGo.transform.SetParent(graphRoot, false);
                var lr = lineGo.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.startWidth = 0.05f;
                lr.endWidth = 0.05f;
                lr.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
                lr.startColor = conn.status == "closed" ? Color.red : Color.gray;
                lr.endColor = lr.startColor;
                lr.SetPosition(0, from.position.ToVector3() + Vector3.up * 0.1f);
                lr.SetPosition(1, to.position.ToVector3() + Vector3.up * 0.1f);
                connectionLines[i] = lr;
            }
        }

        void HighlightActivePath()
        {
            PathResult route = AppState.ActiveRoute;
            if (route == null || !route.HasPath || connectionLines == null)
            {
                return;
            }

            foreach (LineRenderer lr in connectionLines)
            {
                if (lr == null)
                {
                    continue;
                }

                lr.startColor = Color.gray;
                lr.endColor = Color.gray;
            }

            AirportGraph graph = PathfindingService.Instance?.GetGraph();
            if (graph == null)
            {
                return;
            }

            for (int i = 0; i < route.ZoneIds.Count - 1; i++)
            {
                string fromId = route.ZoneIds[i];
                string toId = route.ZoneIds[i + 1];
                HighlightConnectionBetween(fromId, toId);
            }
        }

        void HighlightConnectionBetween(string fromId, string toId)
        {
            foreach (AirportConnection conn in PathfindingService.Instance.GetAllConnections())
            {
                bool match = (conn.from == fromId && conn.to == toId) ||
                             (conn.from == toId && conn.to == fromId);
                if (!match)
                {
                    continue;
                }

                foreach (LineRenderer lr in connectionLines)
                {
                    if (lr != null && lr.name == $"Conn_{conn.id}")
                    {
                        Color c = pathMaterial != null ? pathMaterial.color : new Color(0.12f, 0.45f, 0.92f);
                        lr.startColor = c;
                        lr.endColor = c;
                        lr.startWidth = 0.12f;
                        lr.endWidth = 0.12f;
                    }
                }
            }
        }

        public void SimulateMoveToNextWaypoint()
        {
            navigationManager?.AdvanceToNextStep();
        }
    }
}
