using System.Collections.Generic;
using AirportAR.Map;
using AirportAR.Navigation;
using UnityEngine;
using UnityEngine.UI;

namespace AirportAR.UI
{
    /// <summary>
    /// UI list for selecting a navigation destination in the simulated layout.
    /// </summary>
    public class DestinationSelector : MonoBehaviour
    {
        [SerializeField] Transform buttonContainer;
        [SerializeField] Button destinationButtonPrefab;
        [SerializeField] Text routePreviewText;
        [SerializeField] AR.ARNavigationManager navigationManager;

        readonly List<Button> spawnedButtons = new List<Button>();

        void Start()
        {
            BuildDestinationButtons();
        }

        void BuildDestinationButtons()
        {
            if (buttonContainer == null || PathfindingService.Instance == null)
            {
                return;
            }

            foreach (Transform child in buttonContainer)
            {
                Destroy(child.gameObject);
            }

            spawnedButtons.Clear();

            foreach (AirportZone zone in PathfindingService.Instance.GetAllZones())
            {
                Button button = destinationButtonPrefab != null
                    ? Instantiate(destinationButtonPrefab, buttonContainer)
                    : CreateDefaultButton(buttonContainer);

                var label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = zone.name;
                }

                string zoneId = zone.id;
                button.onClick.AddListener(() => OnDestinationSelected(zoneId));
                spawnedButtons.Add(button);
            }
        }

        Button CreateDefaultButton(Transform parent)
        {
            var go = new GameObject("DestinationButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 64f);

            var image = go.GetComponent<Image>();
            image.color = new Color(0.12f, 0.45f, 0.92f);

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;

            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return go.GetComponent<Button>();
        }

        void OnDestinationSelected(string destinationZoneId)
        {
            string startZoneId = AppState.CurrentZoneId;
            PathResult route = PathfindingService.Instance.FindPath(startZoneId, destinationZoneId);

            if (routePreviewText != null)
            {
                AirportGraph graph = PathfindingService.Instance.GetGraph();
                routePreviewText.text = route.HasPath
                    ? $"Route selected: {route.GetRouteSummary(graph)}"
                    : PathfindingService.NoRouteMessage;
            }

            AppState.RequestNavigation(destinationZoneId);
            navigationManager?.StartNavigationToZone(destinationZoneId);
            FindObjectOfType<AppController>()?.ShowMainMenu();
        }
    }
}
