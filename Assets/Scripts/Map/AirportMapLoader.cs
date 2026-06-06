using System;
using System.IO;
using UnityEngine;

namespace AirportAR.Map
{
    /// <summary>
    /// Loads the demo airport map from Resources and persists staff edits locally.
    /// </summary>
    public class AirportMapLoader : MonoBehaviour
    {
        public const string DefaultResourcePath = "Data/demo_airport_map";
        public const string SavedFileName = "demo_airport_map_saved.json";

        public static AirportMapLoader Instance { get; private set; }

        public AirportMapData CurrentMap { get; private set; }

        public event Action MapChanged;

        string SavedFilePath => Path.Combine(Application.persistentDataPath, SavedFileName);

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadMap();
        }

        public void LoadMap()
        {
            if (File.Exists(SavedFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SavedFilePath);
                    CurrentMap = JsonUtility.FromJson<AirportMapData>(json);
                    Debug.Log("[AirportMapLoader] Map loaded from saved file.");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[AirportMapLoader] Failed to load saved map: {ex.Message}. Falling back to default.");
                    LoadDefaultMap();
                }
            }
            else
            {
                LoadDefaultMap();
            }

            MapChanged?.Invoke();
        }

        void LoadDefaultMap()
        {
            TextAsset asset = Resources.Load<TextAsset>(DefaultResourcePath);
            if (asset == null)
            {
                Debug.LogError("[AirportMapLoader] Default map not found in Resources/Data/demo_airport_map.json");
                CurrentMap = new AirportMapData
                {
                    mapName = "Demo Airport Layout",
                    zones = Array.Empty<AirportZone>(),
                    connections = Array.Empty<AirportConnection>(),
                    temporaryMessages = Array.Empty<TemporaryMapMessage>()
                };
                return;
            }

            CurrentMap = JsonUtility.FromJson<AirportMapData>(asset.text);
            Debug.Log("[AirportMapLoader] Map loaded from default Resources JSON.");
        }

        public void SaveMap()
        {
            if (CurrentMap == null)
            {
                Debug.LogWarning("[AirportMapLoader] Cannot save null map.");
                return;
            }

            CurrentMap.lastUpdated = DateTime.Now.ToString("yyyy-MM-dd");
            string json = JsonUtility.ToJson(CurrentMap, true);
            File.WriteAllText(SavedFilePath, json);
            Debug.Log("[AirportMapLoader] Staff changes saved.");
            MapChanged?.Invoke();
        }

        public void ResetToDefault()
        {
            if (File.Exists(SavedFilePath))
            {
                File.Delete(SavedFilePath);
            }

            LoadDefaultMap();
            Debug.Log("[AirportMapLoader] Map reset to default demo layout.");
        }

        public AirportZone GetZoneById(string zoneId)
        {
            if (CurrentMap?.zones == null || string.IsNullOrEmpty(zoneId))
            {
                return null;
            }

            foreach (AirportZone zone in CurrentMap.zones)
            {
                if (zone.id == zoneId)
                {
                    return zone;
                }
            }

            return null;
        }

        public AirportConnection GetConnectionById(string connectionId)
        {
            if (CurrentMap?.connections == null || string.IsNullOrEmpty(connectionId))
            {
                return null;
            }

            foreach (AirportConnection connection in CurrentMap.connections)
            {
                if (connection.id == connectionId)
                {
                    return connection;
                }
            }

            return null;
        }
    }
}
