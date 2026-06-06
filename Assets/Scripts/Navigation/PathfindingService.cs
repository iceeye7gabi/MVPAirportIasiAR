using System;
using System.Collections.Generic;
using AirportAR.Map;
using UnityEngine;

namespace AirportAR.Navigation
{
    public class PathResult
    {
        public List<string> ZoneIds = new List<string>();
        public float TotalDistance;
        public bool HasPath => ZoneIds != null && ZoneIds.Count > 0;

        public string GetRouteSummary(AirportGraph graph)
        {
            if (!HasPath)
            {
                return string.Empty;
            }

            var names = new List<string>();
            foreach (string zoneId in ZoneIds)
            {
                AirportZone zone = graph.GetZoneById(zoneId);
                names.Add(zone != null ? zone.name : zoneId);
            }

            return string.Join(" → ", names);
        }
    }

    /// <summary>
    /// Dijkstra pathfinding over the simulated airport graph.
    /// </summary>
    public class PathfindingService : MonoBehaviour
    {
        public static PathfindingService Instance { get; private set; }

        public const string NoRouteMessage =
            "No available route in the simulated layout. Please ask airport staff for assistance.";

        AirportGraph graph;

        public event Action<PathResult> RouteRecalculated;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            if (AirportMapLoader.Instance != null)
            {
                AirportMapLoader.Instance.MapChanged += RebuildGraph;
                RebuildGraph();
            }
        }

        void OnDestroy()
        {
            if (AirportMapLoader.Instance != null)
            {
                AirportMapLoader.Instance.MapChanged -= RebuildGraph;
            }
        }

        public void RebuildGraph()
        {
            if (AirportMapLoader.Instance?.CurrentMap == null)
            {
                return;
            }

            graph = new AirportGraph(AirportMapLoader.Instance.CurrentMap);
        }

        public AirportGraph GetGraph() => graph;

        public AirportZone GetZoneById(string id) => graph?.GetZoneById(id);

        public IEnumerable<AirportZone> GetAllZones()
        {
            return graph != null ? graph.GetAllZones() : Array.Empty<AirportZone>();
        }

        public IEnumerable<AirportConnection> GetAllConnections()
        {
            return graph != null ? graph.GetAllConnections() : Array.Empty<AirportConnection>();
        }

        public void SetConnectionStatus(string connectionId, string status)
        {
            graph?.SetConnectionStatus(connectionId, status);
        }

        public string GetConnectionStatus(string connectionId)
        {
            return graph?.GetConnectionStatus(connectionId);
        }

        public PathResult FindPath(string startZoneId, string destinationZoneId)
        {
            var result = new PathResult();

            if (graph == null)
            {
                Debug.LogWarning("[PathfindingService] Graph not initialized.");
                return result;
            }

            if (string.IsNullOrEmpty(startZoneId) || string.IsNullOrEmpty(destinationZoneId))
            {
                return result;
            }

            if (startZoneId == destinationZoneId)
            {
                result.ZoneIds.Add(startZoneId);
                return result;
            }

            var distances = new Dictionary<string, float>();
            var previous = new Dictionary<string, string>();
            var visited = new HashSet<string>();
            var queue = new List<string>();

            foreach (AirportZone zone in graph.GetAllZones())
            {
                distances[zone.id] = float.MaxValue;
            }

            if (!distances.ContainsKey(startZoneId) || !distances.ContainsKey(destinationZoneId))
            {
                Debug.Log("[PathfindingService] No route found - invalid zone ids.");
                return result;
            }

            distances[startZoneId] = 0f;
            queue.Add(startZoneId);

            while (queue.Count > 0)
            {
                queue.Sort((a, b) => distances[a].CompareTo(distances[b]));
                string current = queue[0];
                queue.RemoveAt(0);

                if (visited.Contains(current))
                {
                    continue;
                }

                visited.Add(current);

                if (current == destinationZoneId)
                {
                    break;
                }

                foreach (AirportGraph.Edge edge in graph.GetEdgesFrom(current))
                {
                    if (!graph.IsTraversable(edge.Status))
                    {
                        continue;
                    }

                    float alt = distances[current] + edge.Cost;
                    if (alt < distances[edge.TargetZoneId])
                    {
                        distances[edge.TargetZoneId] = alt;
                        previous[edge.TargetZoneId] = current;
                        if (!queue.Contains(edge.TargetZoneId))
                        {
                            queue.Add(edge.TargetZoneId);
                        }
                    }
                }
            }

            if (!previous.ContainsKey(destinationZoneId) && startZoneId != destinationZoneId)
            {
                Debug.Log("[PathfindingService] No route found.");
                return result;
            }

            string step = destinationZoneId;
            var path = new List<string>();
            while (!string.IsNullOrEmpty(step))
            {
                path.Add(step);
                if (step == startZoneId)
                {
                    break;
                }

                previous.TryGetValue(step, out step);
            }

            path.Reverse();
            result.ZoneIds = path;
            result.TotalDistance = distances[destinationZoneId];

            Debug.Log($"[PathfindingService] Route calculated: {result.GetRouteSummary(graph)} ({result.TotalDistance:F0}m demo distance)");
            RouteRecalculated?.Invoke(result);
            return result;
        }
    }
}
