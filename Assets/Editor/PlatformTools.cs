#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;

namespace AirportAR.Editor
{
    public static class PlatformTools
    {
        const string MenuSwitchIos = "Airport AR/Switch Platform to iOS";
        const string MenuDiagnose = "Airport AR/Diagnose iOS Build Support";
        const string MenuSetupXr = "Airport AR/Setup Mobile XR (ARKit + ARCore)";

        [MenuItem(MenuSwitchIos)]
        public static void SwitchToIosPlatform()
        {
            if (!TrySwitchToIos(out string error))
            {
                EditorUtility.DisplayDialog("Switch to iOS Failed", error, "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Platform Switched",
                    "Active build target is now iOS.\n\n" +
                    "Next: File → Build Settings → Build (creates Xcode project).",
                    "OK");
            }
        }

        /// <summary>Called from command line: -executeMethod AirportAR.Editor.PlatformTools.BatchSwitchToIos</summary>
        public static void BatchSwitchToIos()
        {
            if (!TrySwitchToIos(out string error))
            {
                Debug.LogError($"[PlatformTools] Batch switch failed: {error}");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[PlatformTools] Batch switch to iOS succeeded.");
            EditorApplication.Exit(0);
        }

        static bool TrySwitchToIos(out string error)
        {
            error = null;

            if (!IsIosBuildSupportInstalled())
            {
                error =
                    "iOS Build Support is missing from this Unity install.\n\n" +
                    "Unity Hub may show \"Installed\" but iOSPlayer is not on disk.\n\n" +
                    "Fix: run ./scripts/install_ios_module.sh in Terminal,\n" +
                    "or use Airport AR → Diagnose iOS Build Support for details.";
                return false;
            }

            SetupMobileXr();
            MobileARProjectConfigurator.ApplyIosSettings();

            bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.iOS,
                BuildTarget.iOS);

            if (!switched)
            {
                error = "EditorUserBuildSettings.SwitchActiveBuildTarget returned false. Check the Console.";
                return false;
            }

            Debug.Log("[PlatformTools] Switched active build target to iOS.");
            return true;
        }

        [MenuItem("Airport AR/Fix XR Plug-in Management (iOS=ARKit, Android=ARCore)")]
        public static void FixXrPluginManagement()
        {
            MobileARProjectConfigurator.ApplyAndroidSettings();
            MobileARProjectConfigurator.ApplyIosSettings();
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog(
                "XR Plug-in Management Fixed",
                "Configured loaders:\n\n" +
                "• iOS tab → Apple ARKit only\n" +
                "• Android tab → Google ARCore only\n\n" +
                "Open Project Settings → XR Plug-in Management and verify the iOS tab (Apple icon), NOT the Android tab.",
                "OK");
        }

        [MenuItem(MenuDiagnose)]
        public static void DiagnoseIosBuildSupport()
        {
            string playbackEngines = GetPlaybackEnginesPath();
            string iosSupport = GetIosSupportPath();
            bool iosInstalled = IsIosBuildSupportInstalled();
            string version = Application.unityVersion;
            DiagnoseXcodeToolchain(out string xcodeSelectPath, out bool actoolAvailable, out string xcodeAppPath);

            string message =
                $"Unity version: {version}\n\n" +
                $"PlaybackEngines:\n{playbackEngines}\n\n" +
                $"iOS support path:\n{iosSupport}\n\n" +
                $"iOS module present: {iosInstalled}\n\n" +
                $"xcode-select path:\n{xcodeSelectPath}\n\n" +
                $"Xcode.app: {(string.IsNullOrEmpty(xcodeAppPath) ? "NOT FOUND" : xcodeAppPath)}\n\n" +
                $"actool available: {actoolAvailable}\n\n" +
                (iosInstalled
                    ? "iOS Build Support is present. Switch Platform should work.\n\n"
                    : "iOS Build Support is MISSING.\n\n" +
                      "Unity Hub often marks it installed without copying files.\n" +
                      "Run: ./scripts/install_ios_module.sh\n\n" +
                      "Note: the official .pkg expects legacy /Applications/Unity/Unity.app " +
                      "and fails silently on Hub installs.\n\n") +
                (!actoolAvailable
                    ? "ARKit iOS builds need full Xcode (not just Command Line Tools).\n\n" +
                      "Fix in Terminal:\n" +
                      "1. Install Xcode from the App Store\n" +
                      "2. Open Xcode once and accept the license\n" +
                      "3. sudo xcode-select -s /Applications/Xcode.app/Contents/Developer\n" +
                      "4. xcrun --find actool\n"
                    : "Xcode toolchain looks OK for ARKit iOS builds.");

            Debug.Log($"[PlatformTools] Diagnosis: iOSPlayer={iosInstalled}, actool={actoolAvailable}, xcode={xcodeSelectPath}");
            EditorUtility.DisplayDialog("iOS Build Support Diagnosis", message, "OK");
        }

        static void DiagnoseXcodeToolchain(out string xcodeSelectPath, out bool actoolAvailable, out string xcodeAppPath)
        {
            xcodeSelectPath = RunShell("xcode-select -p");
            actoolAvailable = !string.IsNullOrEmpty(RunShell("xcrun --find actool 2>/dev/null"));
            xcodeAppPath = Directory.Exists("/Applications/Xcode.app")
                ? "/Applications/Xcode.app"
                : string.Empty;
        }

        static string RunShell(string command)
        {
            try
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "/bin/zsh";
                process.StartInfo.Arguments = $"-lc \"{command}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(5000);
                return output;
            }
            catch (System.Exception ex)
            {
                return $"error: {ex.Message}";
            }
        }

        [MenuItem(MenuSetupXr)]
        public static void SetupMobileXr()
        {
            EnsureXrForBuildTarget(
                BuildTargetGroup.iOS,
                "UnityEngine.XR.ARKit.ARKitLoader",
                "Assets/XR/Loaders/ARKitLoader.asset");
            EnsureXrForBuildTarget(
                BuildTargetGroup.Android,
                "UnityEngine.XR.ARCore.ARCoreLoader",
                "Assets/XR/Loaders/ARCoreLoader.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("[PlatformTools] Mobile XR loaders configured.");
        }

        [InitializeOnLoadMethod]
        static void AutoSetupXrOnLoad()
        {
            EditorApplication.delayCall += SetupMobileXr;
        }

        static string GetPlaybackEnginesPath()
        {
            string appPath = EditorApplication.applicationPath;
            if (appPath.EndsWith(".app", System.StringComparison.OrdinalIgnoreCase))
            {
                // Hub layout: .../2022.3.xx/Unity.app
                string hubPlayback = Path.GetFullPath(
                    Path.Combine(appPath, "..", "..", "PlaybackEngines"));
                if (Directory.Exists(hubPlayback))
                {
                    return hubPlayback;
                }

                return Path.Combine(appPath, "Contents", "PlaybackEngines");
            }

            return Path.GetFullPath(Path.Combine(appPath, "..", "PlaybackEngines"));
        }

        static string GetIosSupportPath()
        {
            string hubIos = Path.Combine(GetPlaybackEnginesPath(), "iOSSupport");
            if (Directory.Exists(hubIos))
            {
                return hubIos;
            }

            string appPath = EditorApplication.applicationPath;
            if (appPath.EndsWith(".app", System.StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(appPath, "Contents", "PlaybackEngines", "iOSPlayer");
            }

            return Path.Combine(
                Path.GetFullPath(Path.Combine(appPath, "..", "PlaybackEngines")),
                "iOSPlayer");
        }

        public static bool IsIosBuildSupportInstalled()
        {
            string hubSupport = Path.Combine(GetPlaybackEnginesPath(), "iOSSupport");
            if (Directory.Exists(hubSupport))
            {
                return true;
            }

            string appPlayer = Path.Combine(
                EditorApplication.applicationPath.EndsWith(".app", System.StringComparison.OrdinalIgnoreCase)
                    ? Path.Combine(EditorApplication.applicationPath, "Contents", "PlaybackEngines")
                    : Path.GetFullPath(Path.Combine(EditorApplication.applicationPath, "..", "PlaybackEngines")),
                "iOSPlayer");
            return Directory.Exists(appPlayer);
        }

        static XRGeneralSettingsPerBuildTarget GetOrCreateXrSettingsAsset()
        {
            if (EditorBuildSettings.TryGetConfigObject(
                    XRGeneralSettings.k_SettingsKey,
                    out XRGeneralSettingsPerBuildTarget existing))
            {
                return existing;
            }

            string[] guids = AssetDatabase.FindAssets("t:XRGeneralSettingsPerBuildTarget");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(path);
            }

            return null;
        }

        static void EnsureXrForBuildTarget(
            BuildTargetGroup group,
            string loaderTypeName,
            string loaderAssetPath)
        {
            var perBuildTarget = GetOrCreateXrSettingsAsset();
            if (perBuildTarget == null)
            {
                Debug.LogWarning("[PlatformTools] XRGeneralSettingsPerBuildTarget asset not found.");
                return;
            }
            if (!perBuildTarget.HasSettingsForBuildTarget(group))
            {
                perBuildTarget.CreateDefaultSettingsForBuildTarget(group);
            }

            if (!perBuildTarget.HasManagerSettingsForBuildTarget(group))
            {
                perBuildTarget.CreateDefaultManagerSettingsForBuildTarget(group);
            }

            var generalSettings = perBuildTarget.SettingsForBuildTarget(group);
            var managerSettings = generalSettings?.AssignedSettings;
            if (managerSettings == null)
            {
                Debug.LogWarning($"[PlatformTools] Could not create XR manager settings for {group}.");
                return;
            }

            if (!XRPackageMetadataStore.AssignLoader(managerSettings, loaderTypeName, group))
            {
                var loader = AssetDatabase.LoadAssetAtPath<XRLoader>(loaderAssetPath);
                if (loader != null)
                {
                    managerSettings.TryAddLoader(loader);
                }
                else
                {
                    Debug.LogWarning($"[PlatformTools] Could not assign loader for {group}: {loaderTypeName}");
                }
            }

            generalSettings.InitManagerOnStart = true;
            EditorUtility.SetDirty(generalSettings);
            EditorUtility.SetDirty(managerSettings);
            EditorUtility.SetDirty(perBuildTarget);
        }
    }
}
#endif
