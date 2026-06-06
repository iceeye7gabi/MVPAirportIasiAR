using System;
using AirportAR.Navigation;

namespace AirportAR
{
    /// <summary>
    /// Shared application state for navigation flows (chatbot → AR, staff recalc, etc.).
    /// </summary>
    public static class AppState
    {
        public const string DefaultStartZoneId = "entrance";

        public static string CurrentZoneId { get; set; } = DefaultStartZoneId;
        public static string PendingDestinationZoneId { get; set; }
        public static PathResult ActiveRoute { get; set; }

        public static event Action<string> NavigationRequested;

        public static void RequestNavigation(string destinationZoneId)
        {
            PendingDestinationZoneId = destinationZoneId;
            NavigationRequested?.Invoke(destinationZoneId);
        }

        public static void ClearPendingNavigation()
        {
            PendingDestinationZoneId = null;
        }
    }
}
