using AirportAR.AR;
using UnityEngine;

namespace AirportAR.UI
{
    public enum AppPanel
    {
        MainMenu,
        Discover,
        Faq,
        About
    }

    /// <summary>
    /// Switches between the three main sections of the demo app.
    /// </summary>
    public class AppController : MonoBehaviour
    {
        [SerializeField] GameObject mainMenuPanel;
        [SerializeField] GameObject discoverPanel;
        [SerializeField] GameObject faqPanel;
        [SerializeField] GameObject aboutPanel;

        AppPanel currentPanel = AppPanel.MainMenu;
        AirportDiscoverController discoverController;

        void Start()
        {
            discoverController = discoverPanel != null
                ? discoverPanel.GetComponent<AirportDiscoverController>()
                : null;
            ShowPanel(AppPanel.MainMenu);
        }

        public void ShowPanel(AppPanel panel)
        {
            if (currentPanel == AppPanel.Discover && panel != AppPanel.Discover)
            {
                discoverController?.OnDiscoverHidden();
            }

            currentPanel = panel;
            SetActive(mainMenuPanel, panel == AppPanel.MainMenu);
            SetActive(discoverPanel, panel == AppPanel.Discover);
            SetActive(faqPanel, panel == AppPanel.Faq);
            SetActive(aboutPanel, panel == AppPanel.About);

            if (panel == AppPanel.Discover)
            {
                discoverController?.OnDiscoverShown();
            }
        }

        public void ShowMainMenu() => ShowPanel(AppPanel.MainMenu);
        public void ShowDiscover() => ShowPanel(AppPanel.Discover);
        public void ShowFaq() => ShowPanel(AppPanel.Faq);
        public void ShowAbout() => ShowPanel(AppPanel.About);

        static void SetActive(GameObject obj, bool active)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
    }
}
