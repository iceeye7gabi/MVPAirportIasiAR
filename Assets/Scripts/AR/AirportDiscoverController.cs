using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AirportAR.AR
{
    /// <summary>
    /// Opens the device camera for discover mode (static preview, no AR planes).
    /// </summary>
    public class AirportDiscoverController : MonoBehaviour
    {
        [SerializeField] SimpleCameraPreview cameraPreview;
        [SerializeField] DiscoverVoiceAssistant voiceAssistant;
        [SerializeField] Text hintText;
        [SerializeField] GameObject editorPlaceholder;

        Coroutine discoverRoutine;

        public void OnDiscoverShown()
        {
            bool onDevice = Application.isMobilePlatform;

            if (editorPlaceholder != null)
            {
                editorPlaceholder.SetActive(!onDevice);
            }

            if (!onDevice)
            {
                SetHint("Atinge ecranul pentru informații vocale despre aeroport (simulare Editor).", false);
                cameraPreview?.SetPreviewVisible(true);
                cameraPreview?.StopPreview();
                voiceAssistant?.OnDiscoverActivated();
                return;
            }

            if (discoverRoutine != null)
            {
                StopCoroutine(discoverRoutine);
            }

            discoverRoutine = StartCoroutine(StartDiscoverRoutine());
        }

        public void OnDiscoverHidden()
        {
            if (discoverRoutine != null)
            {
                StopCoroutine(discoverRoutine);
                discoverRoutine = null;
            }

            cameraPreview?.StopPreview();
            cameraPreview?.SetPreviewVisible(true);
            voiceAssistant?.OnDiscoverDeactivated();
        }

        IEnumerator StartDiscoverRoutine()
        {
            cameraPreview?.SetPreviewVisible(true);
            cameraPreview?.StartPreview();
            SetHint("Pornesc camera...", true);

            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            }

            SetHint(
                "Atinge ecranul → bun venit și săgeți.\n" +
                "Rotește telefonul stânga/dreapta pentru o direcție.\n" +
                "Atinge săgeata pentru informații vocale.",
                true);

            voiceAssistant?.OnDiscoverActivated();
            discoverRoutine = null;
        }

        void SetHint(string message, bool onCamera)
        {
            if (hintText == null)
            {
                return;
            }

            hintText.text = message;
            hintText.color = onCamera ? Color.white : new Color(0.15f, 0.2f, 0.3f);
        }
    }
}
