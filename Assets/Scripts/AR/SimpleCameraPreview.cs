using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AirportAR.AR
{
    /// <summary>
    /// Shows the device rear camera using WebCamTexture (no ARKit required for preview).
    /// </summary>
    public class SimpleCameraPreview : MonoBehaviour
    {
        [SerializeField] RawImage previewImage;
        [SerializeField] Text statusText;

        WebCamTexture webcam;
        bool running;

        public void StartPreview()
        {
            if (running)
            {
                return;
            }

            StopAllCoroutines();
            StartCoroutine(StartPreviewRoutine());
        }

        public void StopPreview()
        {
            StopAllCoroutines();
            StopWebCam();
            SetStatus(string.Empty);
        }

        IEnumerator StartPreviewRoutine()
        {
            SetStatus("Se cere acces la cameră...");

            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            }

            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                SetStatus(
                    "Acces cameră refuzat.\n\n" +
                    "Setări → [aplicația ta] → Cameră → Permite");
                yield break;
            }

            SetStatus("Se pornește camera...");

            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices == null || devices.Length == 0)
            {
                SetStatus("Nu s-a găsit nicio cameră pe acest dispozitiv.");
                yield break;
            }

            string deviceName = PickRearCamera(devices);
            webcam = new WebCamTexture(deviceName, 1280, 720, 30);
            previewImage.texture = webcam;
            previewImage.color = Color.white;
            webcam.Play();
            running = true;

            float timeout = 5f;
            while (timeout > 0f && webcam.width <= 16)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            if (webcam.width <= 16)
            {
                SetStatus("Camera nu a pornit. Încearcă din nou sau verifică permisiunile.");
                StopWebCam();
                yield break;
            }

            ApplyRotation();
            SetStatus(string.Empty);
        }

        static string PickRearCamera(WebCamDevice[] devices)
        {
            foreach (WebCamDevice device in devices)
            {
                if (!device.isFrontFacing)
                {
                    return device.name;
                }
            }

            return devices[0].name;
        }

        void ApplyRotation()
        {
            if (previewImage == null || webcam == null)
            {
                return;
            }

            RectTransform rect = previewImage.rectTransform;
            float rotation = -webcam.videoRotationAngle;
            rect.localEulerAngles = new Vector3(0f, 0f, rotation);

            bool vertical = Mathf.Abs(rotation) == 90f || Mathf.Abs(rotation) == 270f;
            float ratio = vertical
                ? (float)webcam.width / webcam.height
                : (float)webcam.height / webcam.width;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            if (ratio > (float)Screen.width / Screen.height)
            {
                float width = ratio / ((float)Screen.width / Screen.height);
                rect.localScale = new Vector3(width, 1f, 1f);
            }
            else
            {
                float height = ((float)Screen.width / Screen.height) / ratio;
                rect.localScale = new Vector3(1f, height, 1f);
            }
        }

        void StopWebCam()
        {
            running = false;
            if (webcam != null)
            {
                if (webcam.isPlaying)
                {
                    webcam.Stop();
                }

                Destroy(webcam);
                webcam = null;
            }

            if (previewImage != null)
            {
                previewImage.texture = null;
            }
        }

        void OnDisable()
        {
            StopPreview();
        }

        void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }
    }
}
