using UnityEngine;
using UnityEngine.UI;

namespace AirportAR.UI
{
    /// <summary>
    /// Displays information for a single tour zone.
    /// </summary>
    public class ZoneInfoPanel : MonoBehaviour
    {
        [SerializeField] Text zoneNameText;
        [SerializeField] Text zoneDescriptionText;
        [SerializeField] Text zoneIndexText;
        [SerializeField] Button navigateButton;
        [SerializeField] Button nextButton;

        string currentZoneId;
        System.Action onNext;

        void Awake()
        {
            if (navigateButton != null)
            {
                navigateButton.onClick.AddListener(OnNavigate);
            }

            if (nextButton != null)
            {
                nextButton.onClick.AddListener(() => onNext?.Invoke());
            }
        }

        public void ShowZone(string zoneId, string name, string description, int index, int total,
            System.Action nextCallback)
        {
            currentZoneId = zoneId;
            onNext = nextCallback;

            if (zoneNameText != null)
            {
                zoneNameText.text = name;
            }

            if (zoneDescriptionText != null)
            {
                zoneDescriptionText.text = description;
            }

            if (zoneIndexText != null)
            {
                zoneIndexText.text = $"Zone {index + 1} of {total}";
            }
        }

        void OnNavigate()
        {
            if (string.IsNullOrEmpty(currentZoneId))
            {
                return;
            }

            AppState.RequestNavigation(currentZoneId);
            FindObjectOfType<AppController>()?.ShowMainMenu();
        }
    }
}
