using System;
using UnityEngine;

namespace AirportAR.AR
{
    /// <summary>
    /// Direction lines over the camera preview. All three when facing forward;
    /// turning left/right shows only the matching line. Each hint is tappable.
    /// </summary>
    public class DiscoverDirectionOverlay : MonoBehaviour
    {
        const float AllDirectionsCone = 38f;
        const float SideDirectionMin = 38f;
        const float SideDirectionMax = 125f;

        [SerializeField] GameObject overlayRoot;
        [SerializeField] GameObject forwardHint;
        [SerializeField] GameObject forwardLine;
        [SerializeField] GameObject leftHint;
        [SerializeField] GameObject leftLine;
        [SerializeField] GameObject rightHint;
        [SerializeField] GameObject rightLine;

        float referenceYaw;
        bool sensorsEnabled;
        DirectionMode currentMode = DirectionMode.All;

        public bool IsVisible => overlayRoot != null && overlayRoot.activeSelf;
        public DirectionMode CurrentMode => currentMode;

        public event Action OnShown;

        public enum DirectionMode
        {
            All,
            Forward,
            Left,
            Right
        }

        void OnEnable()
        {
            EnableSensors();
        }

        void OnDisable()
        {
            DisableSensors();
        }

        void Update()
        {
            if (!IsVisible)
            {
                return;
            }

            ApplyMode(GetModeFromYaw());
        }

        public void Show()
        {
            if (overlayRoot == null)
            {
                return;
            }

            bool wasHidden = !overlayRoot.activeSelf;
            overlayRoot.SetActive(true);
            EnableSensors();
            CalibrateReference();
            ApplyMode(DirectionMode.All);

            if (wasHidden)
            {
                OnShown?.Invoke();
            }
        }

        public void Hide()
        {
            if (overlayRoot != null)
            {
                overlayRoot.SetActive(false);
            }
        }

        void EnableSensors()
        {
            if (sensorsEnabled)
            {
                return;
            }

            if (SystemInfo.supportsGyroscope)
            {
                Input.gyro.enabled = true;
            }

            Input.compass.enabled = Input.compass.enabled || SystemInfo.supportsGyroscope;
            sensorsEnabled = true;
        }

        void DisableSensors()
        {
            if (!sensorsEnabled)
            {
                return;
            }

            if (SystemInfo.supportsGyroscope)
            {
                Input.gyro.enabled = false;
            }

            sensorsEnabled = false;
        }

        void CalibrateReference()
        {
            referenceYaw = GetDeviceYaw();
        }

        float GetDeviceYaw()
        {
            if (SystemInfo.supportsGyroscope && Input.gyro.enabled)
            {
                Quaternion attitude = Input.gyro.attitude;
                Quaternion portraitFix = Quaternion.Euler(90f, 0f, 0f);
                Quaternion corrected = portraitFix * attitude;
                return corrected.eulerAngles.y;
            }

            if (Input.compass.enabled)
            {
                return Input.compass.trueHeading;
            }

            return 0f;
        }

        float GetYawDelta()
        {
            return Mathf.DeltaAngle(referenceYaw, GetDeviceYaw());
        }

        DirectionMode GetModeFromYaw()
        {
            float delta = GetYawDelta();

            if (Mathf.Abs(delta) <= AllDirectionsCone)
            {
                return DirectionMode.All;
            }

            if (delta < -SideDirectionMin && delta > -SideDirectionMax)
            {
                return DirectionMode.Left;
            }

            if (delta > SideDirectionMin && delta < SideDirectionMax)
            {
                return DirectionMode.Right;
            }

            return DirectionMode.Forward;
        }

        void ApplyMode(DirectionMode mode)
        {
            currentMode = mode;
            bool showAll = mode == DirectionMode.All;
            SetDirectionVisible(forwardHint, forwardLine, showAll || mode == DirectionMode.Forward);
            SetDirectionVisible(leftHint, leftLine, showAll || mode == DirectionMode.Left);
            SetDirectionVisible(rightHint, rightLine, showAll || mode == DirectionMode.Right);
        }

        static void SetDirectionVisible(GameObject hint, GameObject line, bool visible)
        {
            if (hint != null)
            {
                hint.SetActive(visible);
            }

            if (line != null)
            {
                line.SetActive(visible);
            }
        }
    }
}
