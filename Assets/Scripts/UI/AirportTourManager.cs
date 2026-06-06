using UnityEngine;

namespace AirportAR.UI
{
    /// <summary>
    /// Guided tour through the simulated demo airport zones.
    /// </summary>
    public class AirportTourManager : MonoBehaviour
    {
        [SerializeField] ZoneInfoPanel zoneInfoPanel;

        struct TourStop
        {
            public string ZoneId;
            public string Name;
            public string Description;
        }

        readonly TourStop[] stops =
        {
            new TourStop
            {
                ZoneId = "entrance",
                Name = "Airport Entrance",
                Description =
                    "Airport Entrance: In a real airport, this is where passengers enter the terminal. " +
                    "In this MVP, the entrance is the starting point of the simulated demo layout."
            },
            new TourStop
            {
                ZoneId = "info",
                Name = "Information Desk",
                Description =
                    "Information Desk: A public assistance point in the demo layout where passengers could ask for help. " +
                    "This zone is fictional and used only for demonstration."
            },
            new TourStop
            {
                ZoneId = "checkin",
                Name = "Check-in Area",
                Description =
                    "Check-in Area: In a real airport, passengers usually drop baggage and receive boarding documents here. " +
                    "In this MVP, the check-in zone is simulated and used only to demonstrate AR navigation."
            },
            new TourStop
            {
                ZoneId = "security",
                Name = "Security Control",
                Description =
                    "Security Control: A generic simulated checkpoint area in the demo graph. " +
                    "It does not represent real security infrastructure."
            },
            new TourStop
            {
                ZoneId = "gate_a1",
                Name = "Gates",
                Description =
                    "Gates: Gate A1 and Gate A2 are fictional boarding areas in the demo layout. " +
                    "They illustrate how AR guidance could lead passengers toward departure zones."
            },
            new TourStop
            {
                ZoneId = "baggage",
                Name = "Baggage Claim",
                Description =
                    "Baggage Claim: A simulated area where arriving passengers would collect luggage. " +
                    "All coordinates and routes here are fictional."
            },
            new TourStop
            {
                ZoneId = "exit_taxi",
                Name = "Taxi Exit",
                Description =
                    "Taxi Exit: A demo exit zone representing ground transportation pickup. " +
                    "This completes the simulated passenger journey in the MVP."
            }
        };

        int currentIndex;

        void OnEnable()
        {
            currentIndex = 0;
            ShowCurrentStop();
        }

        public void ShowNextStop()
        {
            if (currentIndex < stops.Length - 1)
            {
                currentIndex++;
                ShowCurrentStop();
            }
        }

        public void ShowPreviousStop()
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                ShowCurrentStop();
            }
        }

        void ShowCurrentStop()
        {
            if (zoneInfoPanel == null)
            {
                return;
            }

            TourStop stop = stops[currentIndex];
            zoneInfoPanel.ShowZone(stop.ZoneId, stop.Name, stop.Description, currentIndex, stops.Length,
                ShowNextStop);
        }
    }
}
