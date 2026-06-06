using System;
using System.Collections.Generic;
using AirportAR.Map;
using UnityEngine;

namespace AirportAR.Navigation
{
    /// <summary>
    /// Graph representation of the simulated airport layout.
    /// Zones are nodes; connections are bidirectional edges.
    /// </summary>
    public class AirportGraph
    {
        readonly Dictionary<string, AirportZone> zonesById = new Dictionary<string, AirportZone>();
        readonly Dictionary<string, AirportConnection> connectionsById = new Dictionary<string, AirportConnection>();
        readonly Dictionary<string, List<Edge>> adjacency = new Dictionary<string, List<Edge>>();

        public struct Edge
        {
            public string ConnectionId;
            public string TargetZoneId;
            public float Cost;
            public string Status;
        }

        public AirportGraph(AirportMapData mapData)
        {
            Rebuild(mapData);
        }

        public void Rebuild(AirportMapData mapData)
        {
            zonesById.Clear();
            connectionsById.Clear();
            adjacency.Clear();

            if (mapData == null)
            {
                return;
            }

            if (mapData.zones != null)
            {
                foreach (AirportZone zone in mapData.zones)
                {
                    zonesById[zone.id] = zone;
                    adjacency[zone.id] = new List<Edge>();
                }
            }

            if (mapData.connections != null)
            {
                foreach (AirportConnection connection in mapData.connections)
                {
                    connectionsById[connection.id] = connection;
                    AddEdge(connection.from, connection.to, connection);
                    AddEdge(connection.to, connection.from, connection);
                }
            }
        }

        void AddEdge(string from, string to, AirportConnection connection)
        {
            if (!adjacency.ContainsKey(from))
            {
                adjacency[from] = new List<Edge>();
            }

            adjacency[from].Add(new Edge
            {
                ConnectionId = connection.id,
                TargetZoneId = to,
                Cost = connection.distance,
                Status = connection.status
            });
        }

        public AirportZone GetZoneById(string id)
        {
            zonesById.TryGetValue(id, out AirportZone zone);
            return zone;
        }

        public IReadOnlyCollection<AirportZone> GetAllZones() => zonesById.Values;

        public IReadOnlyCollection<AirportConnection> GetAllConnections() => connectionsById.Values;

        public IEnumerable<Edge> GetEdgesFrom(string zoneId)
        {
            if (adjacency.TryGetValue(zoneId, out List<Edge> edges))
            {
                return edges;
            }

            return Array.Empty<Edge>();
        }

        public bool IsTraversable(string status)
        {
            return status == "open" || status == "temporary";
        }

        public void SetConnectionStatus(string connectionId, string status)
        {
            if (!connectionsById.TryGetValue(connectionId, out AirportConnection connection))
            {
                Debug.LogWarning($"[AirportGraph] Connection not found: {connectionId}");
                return;
            }

            connection.status = status;
            RebuildFromConnections();
            Debug.Log($"[AirportGraph] Connection status changed: {connectionId} -> {status}");
        }

        public string GetConnectionStatus(string connectionId)
        {
            return connectionsById.TryGetValue(connectionId, out AirportConnection connection)
                ? connection.status
                : null;
        }

        void RebuildFromConnections()
        {
            adjacency.Clear();
            foreach (AirportZone zone in zonesById.Values)
            {
                adjacency[zone.id] = new List<Edge>();
            }

            foreach (AirportConnection connection in connectionsById.Values)
            {
                AddEdge(connection.from, connection.to, connection);
                AddEdge(connection.to, connection.from, connection);
            }
        }
    }
}
