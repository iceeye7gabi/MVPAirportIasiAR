using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using Unity.XR.CoreUtils;

namespace AirportAR.AR
{
    /// <summary>
    /// Creates AR Session + XR Origin at runtime on mobile and exposes the camera feed.
    /// </summary>
    public class MobileARSessionBootstrap : MonoBehaviour
    {
        public static MobileARSessionBootstrap Instance { get; private set; }

        public ARSession Session { get; private set; }
        public XROrigin Origin { get; private set; }
        public ARRaycastManager RaycastManager { get; private set; }
        public ARPlaneManager PlaneManager { get; private set; }
        public ARCameraManager CameraManager { get; private set; }
        public ARCameraBackground CameraBackground { get; private set; }
        public Camera ARCamera { get; private set; }
        public Transform RouteAnchor { get; private set; }

        public bool IsMobileARSupported =>
            Application.isMobilePlatform && SystemInfo.supportsGyroscope;

        public bool IsSessionCreated { get; private set; }
        public bool IsARActive { get; private set; }
        public bool IsRoutePlaced { get; private set; }

        static readonly List<ARRaycastHit> RaycastHits = new List<ARRaycastHit>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (IsMobileARSupported)
            {
                CreateARHierarchy();
            }
        }

        void CreateARHierarchy()
        {
            if (IsSessionCreated)
            {
                return;
            }

            var sessionGo = new GameObject("AR Session");
            sessionGo.transform.SetParent(transform, false);
            Session = sessionGo.AddComponent<ARSession>();
            sessionGo.AddComponent<ARInputManager>();

            var originGo = new GameObject("XR Origin");
            originGo.transform.SetParent(transform, false);
            Origin = originGo.AddComponent<XROrigin>();
            RaycastManager = originGo.AddComponent<ARRaycastManager>();
            PlaneManager = originGo.AddComponent<ARPlaneManager>();
            if (PlaneManager != null)
            {
                PlaneManager.enabled = false;
            }

            var cameraOffset = new GameObject("Camera Offset");
            cameraOffset.transform.SetParent(originGo.transform, false);

            var cameraGo = new GameObject("AR Camera");
            cameraGo.transform.SetParent(cameraOffset.transform, false);
            cameraGo.tag = "MainCamera";

            ARCamera = cameraGo.AddComponent<Camera>();
            ARCamera.clearFlags = CameraClearFlags.SolidColor;
            ARCamera.backgroundColor = Color.black;
            ARCamera.nearClipPlane = 0.1f;
            ARCamera.farClipPlane = 50f;
            ARCamera.depth = 0;

            cameraGo.AddComponent<AudioListener>();
            CameraManager = cameraGo.AddComponent<ARCameraManager>();
            CameraBackground = cameraGo.AddComponent<ARCameraBackground>();

            Origin.CameraFloorOffsetObject = cameraOffset;
            Origin.Camera = ARCamera;

            var anchorGo = new GameObject("RouteAnchor");
            anchorGo.transform.SetParent(originGo.transform, false);
            RouteAnchor = anchorGo.transform;

            originGo.SetActive(false);
            sessionGo.SetActive(false);

            IsSessionCreated = true;
            Debug.Log("[MobileARSessionBootstrap] AR hierarchy created.");
        }

        public void SetARActive(bool active)
        {
            if (!IsMobileARSupported || !IsSessionCreated)
            {
                Debug.LogWarning("[MobileARSessionBootstrap] AR not supported or hierarchy missing.");
                return;
            }

            if (active)
            {
                EnsureXrStarted();
            }

            IsARActive = active;
            Origin.gameObject.SetActive(active);
            Session.gameObject.SetActive(active);

            if (active)
            {
                DisableFallbackCameras();
                if (CameraBackground != null)
                {
                    CameraBackground.enabled = true;
                }

                if (CameraManager != null)
                {
                    CameraManager.enabled = true;
                }
            }

            Debug.Log($"[MobileARSessionBootstrap] AR active: {active}, session state: {ARSession.state}");
        }

        static void EnsureXrStarted()
        {
            XRGeneralSettings generalSettings = XRGeneralSettings.Instance;
            if (generalSettings == null || generalSettings.Manager == null)
            {
                Debug.LogError("[MobileARSessionBootstrap] XRGeneralSettings missing. Enable ARKit in XR Plug-in Management.");
                return;
            }

            XRManagerSettings manager = generalSettings.Manager;
            if (!manager.isInitializationComplete)
            {
                manager.InitializeLoaderSync();
            }

            if (manager.activeLoader == null)
            {
                Debug.LogError("[MobileARSessionBootstrap] XR loader failed to start. Check ARKit is enabled for iOS.");
                return;
            }

            if (!manager.activeLoader.name.ToLowerInvariant().Contains("arkit") &&
                !manager.activeLoader.name.ToLowerInvariant().Contains("arcore"))
            {
                Debug.LogWarning($"[MobileARSessionBootstrap] Active loader: {manager.activeLoader.name}");
            }

            manager.StartSubsystems();
        }

        void DisableFallbackCameras()
        {
            foreach (Camera cam in Camera.allCameras)
            {
                if (cam != null && cam != ARCamera && cam.CompareTag("MainCamera"))
                {
                    cam.enabled = false;
                }
            }

            if (ARCamera != null)
            {
                ARCamera.enabled = true;
            }
        }

        public bool TryPlaceRouteAnchor(Vector2 screenPosition)
        {
            if (!IsARActive || RaycastManager == null || RouteAnchor == null)
            {
                return false;
            }

            if (!RaycastManager.Raycast(screenPosition, RaycastHits, TrackableType.PlaneWithinPolygon))
            {
                return false;
            }

            Pose hitPose = RaycastHits[0].pose;
            RouteAnchor.SetPositionAndRotation(hitPose.position, hitPose.rotation);

            if (ARCamera != null)
            {
                Vector3 forward = ARCamera.transform.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude > 0.001f)
                {
                    RouteAnchor.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
                }
            }

            IsRoutePlaced = true;
            Debug.Log("[MobileARSessionBootstrap] Route anchor placed on AR plane.");
            return true;
        }

        public bool TryAutoPlaceRouteAnchor()
        {
            return TryPlaceRouteAnchor(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        }

        public void ResetRoutePlacement()
        {
            IsRoutePlaced = false;
            if (RouteAnchor != null)
            {
                RouteAnchor.localPosition = Vector3.zero;
                RouteAnchor.localRotation = Quaternion.identity;
            }
        }
    }
}
