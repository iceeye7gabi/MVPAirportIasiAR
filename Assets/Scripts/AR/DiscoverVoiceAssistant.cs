using UnityEngine;
using UnityEngine.UI;

namespace AirportAR.AR
{
    /// <summary>
    /// Tap screen to show directions + welcome info; tap each direction button for its message.
    /// </summary>
    public class DiscoverVoiceAssistant : MonoBehaviour
    {
        const string WelcomeMessage =
            "Bine ai venit la Aeroportul Internațional Iași!\n\n" +
            "În față: check-in · Stânga: Security Control · Dreapta: Ciambella și toalete.\n\n" +
            "Rotește telefonul pentru o direcție, apoi atinge săgeata.";

        const string ForwardMessage =
            "Check-in. Aici găsești ghișeele pentru înregistrarea zborului tău.";

        const string LeftMessage =
            "Security Control. Aici este punctul de control al securității.";

        const string RightMessage =
            "Ciambella și toalete. La dreapta găsești restaurantul și toaletele publice.";

        [SerializeField] Text assistantBubbleText;
        [SerializeField] GameObject assistantBubbleRoot;
        [SerializeField] DiscoverDirectionOverlay directionOverlay;

        MobileSpeechService speech;
        bool active;
        bool welcomed;

        void Awake()
        {
            speech = GetComponent<MobileSpeechService>();
            if (speech == null)
            {
                speech = gameObject.AddComponent<MobileSpeechService>();
            }

            HideBubble();
            directionOverlay?.Hide();
        }

        public void OnDiscoverActivated()
        {
            active = true;
            welcomed = false;
            HideBubble();
            directionOverlay?.Hide();
        }

        public void OnDiscoverDeactivated()
        {
            active = false;
            welcomed = false;
            speech?.StopSpeaking();
            HideBubble();
            directionOverlay?.Hide();
        }

        public void OnScreenTapped()
        {
            if (!active)
            {
                return;
            }

            directionOverlay?.Show();
            ShowWelcomeIfNeeded();
        }

        public void OnForwardTapped()
        {
            OnDirectionTapped(DiscoverDirectionOverlay.DirectionMode.Forward);
        }

        public void OnLeftTapped()
        {
            OnDirectionTapped(DiscoverDirectionOverlay.DirectionMode.Left);
        }

        public void OnRightTapped()
        {
            OnDirectionTapped(DiscoverDirectionOverlay.DirectionMode.Right);
        }

        void OnDirectionTapped(DiscoverDirectionOverlay.DirectionMode mode)
        {
            if (!active)
            {
                return;
            }

            bool wasHidden = directionOverlay != null && !directionOverlay.IsVisible;
            directionOverlay?.Show();

            if (wasHidden)
            {
                ShowWelcomeIfNeeded();
                return;
            }

            switch (mode)
            {
                case DiscoverDirectionOverlay.DirectionMode.Left:
                    PresentGuide(DiscoverSpeechId.Left, LeftMessage);
                    break;
                case DiscoverDirectionOverlay.DirectionMode.Right:
                    PresentGuide(DiscoverSpeechId.Right, RightMessage);
                    break;
                default:
                    PresentGuide(DiscoverSpeechId.Forward, ForwardMessage);
                    break;
            }
        }

        void ShowWelcomeIfNeeded()
        {
            if (welcomed)
            {
                return;
            }

            welcomed = true;
            PresentGuide(DiscoverSpeechId.Welcome, WelcomeMessage);
        }

        void PresentGuide(DiscoverSpeechId speechId, string message)
        {
            ShowBubble(message);
            speech?.Play(speechId);
        }

        void ShowBubble(string message)
        {
            if (assistantBubbleRoot != null)
            {
                assistantBubbleRoot.SetActive(true);
                assistantBubbleRoot.transform.SetAsLastSibling();
            }

            if (assistantBubbleText != null)
            {
                assistantBubbleText.text = message;
            }
        }

        void HideBubble()
        {
            if (assistantBubbleRoot != null)
            {
                assistantBubbleRoot.SetActive(false);
            }
        }
    }
}
