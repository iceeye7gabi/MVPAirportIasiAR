using UnityEngine;
using UnityEngine.UI;

namespace AirportAR.AR
{
    /// <summary>
    /// Switches navigation UI to a transparent HUD so the phone camera feed is visible.
    /// </summary>
    public class ARCameraFeedController : MonoBehaviour
    {
        [SerializeField] Image panelBackground;
        [SerializeField] Text statusText;

        Color opaqueBackground;
        bool configured;

        void Awake()
        {
            if (panelBackground != null)
            {
                opaqueBackground = panelBackground.color;
            }
        }

        public void SetNavigationARMode(bool arMode)
        {
            if (!configured && panelBackground != null)
            {
                opaqueBackground = panelBackground.color;
                configured = true;
            }

            if (panelBackground != null)
            {
                if (arMode)
                {
                    panelBackground.color = new Color(
                        opaqueBackground.r,
                        opaqueBackground.g,
                        opaqueBackground.b,
                        0.15f);
                }
                else
                {
                    panelBackground.color = opaqueBackground;
                }
            }

            ApplyReadableTextColors(arMode);
        }

        void ApplyReadableTextColors(bool arMode)
        {
            if (!arMode)
            {
                return;
            }

            foreach (Text text in GetComponentsInChildren<Text>(true))
            {
                if (text.color.a < 0.5f)
                {
                    continue;
                }

                text.color = Color.white;
            }
        }

        public void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }
    }
}
