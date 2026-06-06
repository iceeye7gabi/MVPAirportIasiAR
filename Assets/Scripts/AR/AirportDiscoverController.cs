using UnityEngine;
using UnityEngine.UI;

namespace AirportAR.AR
{
    /// <summary>
    /// Opens the phone camera so the user can look around.
    /// </summary>
    public class AirportDiscoverController : MonoBehaviour
    {
        [SerializeField] SimpleCameraPreview cameraPreview;
        [SerializeField] Text hintText;
        [SerializeField] GameObject editorPlaceholder;

        public void OnDiscoverShown()
        {
            bool onDevice = Application.isMobilePlatform;

            if (editorPlaceholder != null)
            {
                editorPlaceholder.SetActive(!onDevice);
            }

            if (!onDevice)
            {
                SetHint("Modul cameră funcționează pe iPhone sau Android.", false);
                cameraPreview?.StopPreview();
                return;
            }

            SetHint("Privește împrejurimile prin cameră.", true);
            cameraPreview?.StartPreview();
        }

        public void OnDiscoverHidden()
        {
            cameraPreview?.StopPreview();
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
