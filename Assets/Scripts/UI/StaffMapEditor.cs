using System.Collections.Generic;
using AirportAR.Map;
using AirportAR.Navigation;
using UnityEngine;
using UnityEngine.UI;

namespace AirportAR.UI
{
    /// <summary>
    /// Staff-facing editor for the simulated demo map only.
    /// </summary>
    public class StaffMapEditor : MonoBehaviour
    {
        [SerializeField] Transform connectionListContainer;
        [SerializeField] GameObject connectionRowPrefab;
        [SerializeField] InputField temporaryMessageField;
        [SerializeField] Text statusText;
        [SerializeField] Text disclaimerText;

        readonly List<GameObject> rows = new List<GameObject>();

        void Start()
        {
            if (disclaimerText != null)
            {
                disclaimerText.text =
                    "This editor modifies the simulated demo layout only. It does not modify any real airport system.";
            }

            if (AirportMapLoader.Instance != null)
            {
                AirportMapLoader.Instance.MapChanged += RefreshConnectionList;
            }

            RefreshConnectionList();
        }

        void OnDestroy()
        {
            if (AirportMapLoader.Instance != null)
            {
                AirportMapLoader.Instance.MapChanged -= RefreshConnectionList;
            }
        }

        public void RefreshConnectionList()
        {
            foreach (GameObject row in rows)
            {
                if (row != null)
                {
                    Destroy(row);
                }
            }

            rows.Clear();

            if (connectionListContainer == null || AirportMapLoader.Instance?.CurrentMap?.connections == null)
            {
                return;
            }

            foreach (AirportConnection connection in AirportMapLoader.Instance.CurrentMap.connections)
            {
                GameObject row = connectionRowPrefab != null
                    ? Instantiate(connectionRowPrefab, connectionListContainer)
                    : CreateConnectionRow(connectionListContainer, connection);

                rows.Add(row);
            }
        }

        GameObject CreateConnectionRow(Transform parent, AirportConnection connection)
        {
            var row = new GameObject($"Row_{connection.id}", typeof(RectTransform), typeof(Image));
            row.transform.SetParent(parent, false);
            row.GetComponent<Image>().color = new Color(0.95f, 0.96f, 0.98f);

            var layout = row.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 8, 8);
            layout.spacing = 6f;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            var rect = row.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 120f);

            AddRowLabel(row.transform,
                $"{connection.id}: {connection.from} → {connection.to} [{connection.status}]");

            var buttonRow = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            buttonRow.transform.SetParent(row.transform, false);
            var hLayout = buttonRow.GetComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 8f;
            hLayout.childControlWidth = true;
            hLayout.childForceExpandWidth = true;

            string connId = connection.id;
            AddRowButton(buttonRow.transform, "Open", () => SetStatus(connId, "open"));
            AddRowButton(buttonRow.transform, "Closed", () => SetStatus(connId, "closed"));
            AddRowButton(buttonRow.transform, "Temporary", () => SetStatus(connId, "temporary"));

            return row;
        }

        void AddRowLabel(Transform parent, string text)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = text;
            label.fontSize = 16;
            label.color = new Color(0.15f, 0.2f, 0.3f);
            label.alignment = TextAnchor.UpperLeft;
        }

        void AddRowButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.12f, 0.45f, 0.92f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = label;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            go.GetComponent<Button>().onClick.AddListener(onClick);
        }

        void SetStatus(string connectionId, string status)
        {
            AirportConnection connection = AirportMapLoader.Instance.GetConnectionById(connectionId);
            if (connection == null)
            {
                return;
            }

            connection.status = status;
            PathfindingService.Instance?.SetConnectionStatus(connectionId, status);

            if (status == "temporary" && temporaryMessageField != null &&
                !string.IsNullOrWhiteSpace(temporaryMessageField.text))
            {
                AddTemporaryMessage(connectionId, temporaryMessageField.text);
            }

            UpdateStatus($"Connection {connectionId} marked as {status}.");
            RefreshConnectionList();
        }

        void AddTemporaryMessage(string connectionId, string message)
        {
            var map = AirportMapLoader.Instance.CurrentMap;
            var list = new List<TemporaryMapMessage>();
            if (map.temporaryMessages != null)
            {
                list.AddRange(map.temporaryMessages);
            }

            list.RemoveAll(m => m.connectionId == connectionId);
            list.Add(new TemporaryMapMessage { connectionId = connectionId, message = message });
            map.temporaryMessages = list.ToArray();
        }

        public void SaveMap()
        {
            AirportMapLoader.Instance?.SaveMap();
            PathfindingService.Instance?.RebuildGraph();
            UpdateStatus("Simulated map saved.");
        }

        public void ResetToDefault()
        {
            AirportMapLoader.Instance?.ResetToDefault();
            PathfindingService.Instance?.RebuildGraph();
            UpdateStatus("Reset to default demo map.");
            RefreshConnectionList();
        }

        void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }
    }
}
