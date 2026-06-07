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
        bool pausedForSpeech;
        bool isFrontFacing;

        public bool IsRunning => running;

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

        public void SetPreviewVisible(bool visible)
        {
            if (previewImage != null)
            {
                previewImage.gameObject.SetActive(visible);
            }
        }

        public void PauseForSpeech()
        {
            if (webcam == null || !webcam.isPlaying || pausedForSpeech)
            {
                return;
            }

            webcam.Pause();
            pausedForSpeech = true;
        }

        public void ResumeAfterSpeech()
        {
            if (webcam == null || !pausedForSpeech)
            {
                return;
            }

            webcam.Play();
            pausedForSpeech = false;
            ApplyRotation();
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

            WebCamDevice device = PickRearCamera(devices);
            isFrontFacing = device.isFrontFacing;
            webcam = new WebCamTexture(device.name, 1280, 720, 30);
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

        static WebCamDevice PickRearCamera(WebCamDevice[] devices)
        {
            foreach (WebCamDevice device in devices)
            {
                if (!device.isFrontFacing)
                {
                    return device;
                }
            }

            return devices[0];
        }

        void ApplyRotation()
        {
            if (previewImage == null || webcam == null)
            {
                return;
            }

            RectTransform rect = previewImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localEulerAngles = new Vector3(0f, 0f, -webcam.videoRotationAngle);

            // Fix left-right mirror on rear camera using UV flip (keeps correct orientation).
            if (webcam.videoVerticallyMirrored)
            {
                previewImage.uvRect = new Rect(0f, 1f, 1f, -1f);
            }
            else if (!isFrontFacing)
            {
                previewImage.uvRect = new Rect(1f, 0f, -1f, 1f);
            }
            else
            {
                previewImage.uvRect = new Rect(0f, 0f, 1f, 1f);
            }

            int angle = webcam.videoRotationAngle;
            bool vertical = angle == 90 || angle == 270;
            float ratio = vertical
                ? (float)webcam.width / webcam.height
                : (float)webcam.height / webcam.width;

            float screenRatio = (float)Screen.width / Screen.height;
            float fitX = 1f;
            float fitY = 1f;

            if (ratio > screenRatio)
            {
                fitX = ratio / screenRatio;
            }
            else
            {
                fitY = screenRatio / ratio;
            }

            rect.localScale = new Vector3(fitX, fitY, 1f);
        }

        void StopWebCam()
        {
            running = false;
            pausedForSpeech = false;
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
                previewImage.uvRect = new Rect(0f, 0f, 1f, 1f);
                previewImage.rectTransform.localEulerAngles = Vector3.zero;
                previewImage.rectTransform.localScale = Vector3.one;
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
