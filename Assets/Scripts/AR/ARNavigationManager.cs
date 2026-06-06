using AirportAR.Map;
using AirportAR.Navigation;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace AirportAR.AR
{
    /// <summary>
    /// Manages AR navigation: route calculation, arrow spawning, and step instructions.
    /// On mobile, uses the phone camera via AR Foundation and places arrows on detected planes.
    /// </summary>
    public class ARNavigationManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] ARArrowSpawner arrowSpawner;
        [SerializeField] ARSession arSession;
        [SerializeField] GameObject arOrigin;
        [SerializeField] GameObject editorFallbackRoot;
        [SerializeField] ARCameraFeedController cameraFeedController;

        [Header("Navigation UI (bound at runtime if empty)")]
        public UnityEngine.UI.Text destinationText;
        public UnityEngine.UI.Text routeSummaryText;
        public UnityEngine.UI.Text nextStepText;
        public UnityEngine.UI.Text distanceText;
        public UnityEngine.UI.Text disclaimerText;
        public UnityEngine.UI.Text statusText;

        [Header("AR Settings")]
        [SerializeField] float demoMapScale = 1f;

        string currentDestinationId;
        PathResult currentRoute;
        int currentStepIndex;
        Vector3 routeOrigin = Vector3.zero;
        bool useEditorFallback;
        bool useMobileAR;
        bool waitingForPlacement;

        public bool UseMobileAR => useMobileAR;
        public bool WaitingForPlacement => waitingForPlacement;

        void Start()
        {
            useMobileAR = MobileARSessionBootstrap.Instance != null &&
                          MobileARSessionBootstrap.Instance.IsMobileARSupported;
            useEditorFallback = !useMobileAR || MobileARSessionBootstrap.Instance == null;

            if (editorFallbackRoot != null)
            {
                editorFallbackRoot.SetActive(useEditorFallback);
            }

            if (arOrigin != null)
            {
                arOrigin.SetActive(false);
            }

            if (PathfindingService.Instance != null)
            {
                PathfindingService.Instance.RouteRecalculated += OnRouteRecalculated;
            }

            if (AirportMapLoader.Instance != null)
            {
                AirportMapLoader.Instance.MapChanged += RecalculateActiveRoute;
            }

            AppState.NavigationRequested += StartNavigationToZone;

            if (!string.IsNullOrEmpty(AppState.PendingDestinationZoneId))
            {
                StartNavigationToZone(AppState.PendingDestinationZoneId);
            }

            if (disclaimerText != null)
            {
                disclaimerText.text = "Demo route only. Not a real airport route.";
            }
        }

        void OnDestroy()
        {
            if (PathfindingService.Instance != null)
            {
                PathfindingService.Instance.RouteRecalculated -= OnRouteRecalculated;
            }

            if (AirportMapLoader.Instance != null)
            {
                AirportMapLoader.Instance.MapChanged -= RecalculateActiveRoute;
            }

            AppState.NavigationRequested -= StartNavigationToZone;
        }

        public void OnNavigationPanelShown()
        {
            if (!useMobileAR)
            {
                return;
            }

            MobileARSessionBootstrap bootstrap = MobileARSessionBootstrap.Instance;
            bootstrap?.SetARActive(true);
            cameraFeedController?.SetNavigationARMode(true);
            UpdateARStatus("Point your phone at the floor. Tap 'Place Route Start'.");
        }

        public void OnNavigationPanelHidden()
        {
            if (!useMobileAR)
            {
                return;
            }

            MobileARSessionBootstrap.Instance?.SetARActive(false);
            cameraFeedController?.SetNavigationARMode(false);
        }

        public void PlaceRouteStart()
        {
            if (!useMobileAR)
            {
                return;
            }

            MobileARSessionBootstrap bootstrap = MobileARSessionBootstrap.Instance;
            if (bootstrap == null)
            {
                UpdateARStatus("AR not available on this device.");
                return;
            }

            if (bootstrap.TryAutoPlaceRouteAnchor())
            {
                waitingForPlacement = false;
                routeOrigin = bootstrap.RouteAnchor.position;
                arrowSpawner?.SetWorldAnchor(bootstrap.RouteAnchor);
                UpdateARStatus("Route placed. Follow the blue arrows.");
                RenderRoute();
            }
            else
            {
                waitingForPlacement = true;
                UpdateARStatus("Move the phone slowly to scan the floor, then tap again.");
            }
        }

        public void StartNavigationToZone(string destinationZoneId)
        {
            currentDestinationId = destinationZoneId;
            string startZoneId = string.IsNullOrEmpty(AppState.CurrentZoneId)
                ? AppState.DefaultStartZoneId
                : AppState.CurrentZoneId;

            currentRoute = PathfindingService.Instance.FindPath(startZoneId, destinationZoneId);
            AppState.ActiveRoute = currentRoute;
            currentStepIndex = 0;

            if (!currentRoute.HasPath)
            {
                UpdateStatus(PathfindingService.NoRouteMessage);
                arrowSpawner?.ClearArrows();
                return;
            }

            Debug.Log("[ARNavigationManager] Navigation started.");

            if (useMobileAR)
            {
                waitingForPlacement = true;
                MobileARSessionBootstrap bootstrap = MobileARSessionBootstrap.Instance;
                if (bootstrap != null && bootstrap.IsRoutePlaced)
                {
                    routeOrigin = bootstrap.RouteAnchor.position;
                    arrowSpawner?.SetWorldAnchor(bootstrap.RouteAnchor);
                    waitingForPlacement = false;
                    RenderRoute();
                    UpdateARStatus("Follow the blue arrows on the floor.");
                }
                else
                {
                    UpdateARStatus("Tap 'Place Route Start' to anchor the demo route on the floor.");
                    arrowSpawner?.ClearArrows();
                }
            }
            else
            {
                RenderRoute();
            }

            UpdateNavigationUI();
        }

        void OnRouteRecalculated(PathResult result)
        {
            if (string.IsNullOrEmpty(currentDestinationId))
            {
                return;
            }

            RecalculateActiveRoute();
        }

        public void RecalculateActiveRoute()
        {
            if (string.IsNullOrEmpty(currentDestinationId))
            {
                return;
            }

            StartNavigationToZone(currentDestinationId);
        }

        void RenderRoute()
        {
            if (arrowSpawner == null || currentRoute == null)
            {
                return;
            }

            if (useMobileAR && waitingForPlacement)
            {
                return;
            }

            arrowSpawner.SpawnRoute(currentRoute, routeOrigin, demoMapScale);
        }

        public void AdvanceToNextStep()
        {
            if (currentRoute == null || !currentRoute.HasPath)
            {
                return;
            }

            if (currentStepIndex < currentRoute.ZoneIds.Count - 1)
            {
                currentStepIndex++;
                AppState.CurrentZoneId = currentRoute.ZoneIds[currentStepIndex];
                UpdateNavigationUI();
            }
        }

        void UpdateNavigationUI()
        {
            if (currentRoute == null)
            {
                return;
            }

            AirportGraph graph = PathfindingService.Instance.GetGraph();
            var destination = graph.GetZoneById(currentDestinationId);

            if (destinationText != null)
            {
                destinationText.text = destination != null
                    ? $"Destination: {destination.name}"
                    : "Destination: —";
            }

            if (routeSummaryText != null)
            {
                routeSummaryText.text = currentRoute.HasPath
                    ? $"Route: {currentRoute.GetRouteSummary(graph)}"
                    : PathfindingService.NoRouteMessage;
            }

            if (nextStepText != null)
            {
                nextStepText.text = BuildStepInstruction(graph);
            }

            if (distanceText != null)
            {
                distanceText.text = currentRoute.HasPath
                    ? $"Estimated demo distance: {currentRoute.TotalDistance:F0} m"
                    : string.Empty;
            }
        }

        string BuildStepInstruction(AirportGraph graph)
        {
            if (currentRoute == null || !currentRoute.HasPath)
            {
                return PathfindingService.NoRouteMessage;
            }

            if (currentStepIndex >= currentRoute.ZoneIds.Count - 1)
            {
                AirportZone dest = graph.GetZoneById(currentDestinationId);
                return dest != null
                    ? $"You have arrived at {dest.name}"
                    : "You have arrived at your destination";
            }

            string nextZoneId = currentRoute.ZoneIds[currentStepIndex + 1];
            AirportZone nextZone = graph.GetZoneById(nextZoneId);
            AirportZone currentZone = graph.GetZoneById(currentRoute.ZoneIds[currentStepIndex]);

            if (nextZone == null)
            {
                return "Continue along the demo route";
            }

            if (currentStepIndex == 0)
            {
                return $"Go forward toward {nextZone.name}";
            }

            Vector3 currentPos = currentZone?.position.ToVector3() ?? Vector3.zero;
            Vector3 nextPos = nextZone.position.ToVector3();
            Vector3 delta = nextPos - currentPos;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.z))
            {
                return delta.x > 0
                    ? $"Turn right toward {nextZone.name}"
                    : $"Turn left toward {nextZone.name}";
            }

            return $"Continue toward {nextZone.name}";
        }

        void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }

            if (routeSummaryText != null)
            {
                routeSummaryText.text = message;
            }
        }

        void UpdateARStatus(string message)
        {
            UpdateStatus(message);
            cameraFeedController?.SetStatus(message);
        }

        public void SetRouteOrigin(Vector3 origin)
        {
            routeOrigin = origin;
            RenderRoute();
        }
    }
}
