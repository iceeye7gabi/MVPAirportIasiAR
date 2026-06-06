#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.XR.Management;

namespace AirportAR.Editor
{
    /// <summary>
    /// Configures Android (ARCore) and iOS (ARKit) build settings for mobile AR.
    /// </summary>
    public static class MobileARProjectConfigurator
    {
        const string CameraUsageText =
            "Aplicatia foloseste camera pentru a explora imprejurimile in modul Descopera aeroportul.";

        [MenuItem("Airport AR/Configure Android AR Build")]
        public static void ConfigureAndroidFromMenu()
        {
            ApplyAndroidSettings();
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog(
                "Android AR Configured",
                "Android build settings updated for ARCore:\n\n" +
                "- Min SDK 24\n" +
                "- ARM64 + IL2CPP\n" +
                "- Portrait\n\n" +
                "Also enable ARCore in Project Settings > XR Plug-in Management > Android.",
                "OK");
        }

        [MenuItem("Airport AR/Configure iOS AR Build")]
        public static void ConfigureIosFromMenu()
        {
            ApplyIosSettings();
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog(
                "iOS AR Configured",
                "iOS build settings updated for ARKit:\n\n" +
                "- Min iOS 13\n" +
                "- Camera usage description\n" +
                "- Portrait\n\n" +
                "Also enable ARKit in Project Settings > XR Plug-in Management > iOS.\n\n" +
                "Build to Xcode, sign with your Apple ID, then run on iPhone.",
                "OK");
        }

        [MenuItem("Airport AR/Configure Mobile AR (Android + iOS)")]
        public static void ConfigureBothFromMenu()
        {
            ApplyAndroidSettings();
            ApplyIosSettings();
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog(
                "Mobile AR Configured",
                "Android and iOS AR build settings applied.\n\n" +
                "Enable ARCore (Android) and ARKit (iOS) in XR Plug-in Management.",
                "OK");
        }

        [InitializeOnLoadMethod]
        static void AutoConfigureOnLoad()
        {
            EditorApplication.delayCall += () =>
            {
                ApplyAndroidSettings();
                ApplyIosSettings();
            };
        }

        public static void ApplyAndroidSettings()
        {
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            EnsureLoader(BuildTargetGroup.Android, "arcore");
            Debug.Log("[MobileARProjectConfigurator] Android AR build settings applied.");
        }

        public static void ApplyIosSettings()
        {
            PlayerSettings.iOS.targetOSVersionString = "13.0";
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneOnly;
            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1); // ARM64
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            ApplyIosProjectSettingsFlags();
            EnsureLoader(BuildTargetGroup.iOS, "arkit");
            Debug.Log("[MobileARProjectConfigurator] iOS AR build settings applied.");
        }

        static void ApplyIosProjectSettingsFlags()
        {
            Object settings = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/ProjectSettings.asset");
            if (settings == null)
            {
                return;
            }

            var so = new SerializedObject(settings);

            SerializedProperty requireArkit = so.FindProperty("iOSRequireARKit");
            if (requireArkit != null)
            {
                requireArkit.boolValue = true;
            }

            SerializedProperty cameraUsage = so.FindProperty("cameraUsageDescription");
            if (cameraUsage != null)
            {
                cameraUsage.stringValue = CameraUsageText;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void EnsureLoader(BuildTargetGroup targetGroup, string loaderKeyword)
        {
            var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(targetGroup);
            if (generalSettings == null)
            {
                Debug.LogWarning(
                    $"[MobileARProjectConfigurator] XR General Settings not found for {targetGroup}. " +
                    "Open Project Settings > XR Plug-in Management once.");
                return;
            }

            var managerSettings = generalSettings.AssignedSettings;
            if (managerSettings == null)
            {
                Debug.LogWarning(
                    $"[MobileARProjectConfigurator] XR Manager Settings missing for {targetGroup}.");
                return;
            }

            bool loaderFound = false;
            foreach (XRLoader loader in managerSettings.loaders)
            {
                if (loader != null && loader.name.ToLowerInvariant().Contains(loaderKeyword))
                {
                    loaderFound = true;
                    break;
                }
            }

            if (!loaderFound)
            {
                Debug.LogWarning(
                    $"[MobileARProjectConfigurator] Add {loaderKeyword.ToUpperInvariant()} Loader in " +
                    $"Project Settings > XR Plug-in Management > {targetGroup}.");
            }

            generalSettings.InitManagerOnStart = true;
            EditorUtility.SetDirty(generalSettings);
        }
    }
}
#endif
