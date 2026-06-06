using System;
using UnityEngine;

namespace AirportAR.Map
{
    [Serializable]
    public class Vector3Data
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3() => new Vector3(x, y, z);
    }

    [Serializable]
    public class AirportZone
    {
        public string id;
        public string name;
        public string description;
        public string type;
        public Vector3Data position;
    }

    [Serializable]
    public class AirportConnection
    {
        public string id;
        public string from;
        public string to;
        public float distance;
        public string status;
    }

    [Serializable]
    public class TemporaryMapMessage
    {
        public string connectionId;
        public string message;
    }

    [Serializable]
    public class AirportMapData
    {
        public string mapName;
        public string version;
        public string lastUpdated;
        public string disclaimer;
        public AirportZone[] zones;
        public AirportConnection[] connections;
        public TemporaryMapMessage[] temporaryMessages;
    }
}
