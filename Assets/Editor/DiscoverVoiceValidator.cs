#if UNITY_EDITOR
using AirportAR.AR;
using UnityEditor;
using UnityEngine;

namespace AirportAR.Editor
{
    public static class DiscoverVoiceValidator
    {
        [MenuItem("Airport AR/Validate Discover Voice (Play Mode)")]
        static void ValidateInPlayMode()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Discover Voice Validator",
                    "Apasă Play în Unity, apoi rulează din nou acest meniu.",
                    "OK");
                return;
            }

            var assistant = Object.FindObjectOfType<DiscoverVoiceAssistant>();
            var overlay = Object.FindObjectOfType<DiscoverDirectionOverlay>();
            if (assistant == null)
            {
                EditorUtility.DisplayDialog("Discover Voice Validator", "DiscoverVoiceAssistant nu a fost găsit.", "OK");
                return;
            }

            var speech = Object.FindObjectOfType<MobileSpeechService>();
            bool speechOk = speech != null;
            bool overlayOk = overlay != null;

            string message =
                (speechOk ? "✓ MobileSpeechService activ\n" : "✗ MobileSpeechService lipsă\n") +
                (overlayOk ? "✓ DiscoverDirectionOverlay activ\n" : "✗ DiscoverDirectionOverlay lipsă\n") +
                "\nTest pe iPhone:\n" +
                "1. Descoperă aeroportul\n" +
                "2. Tap pe ecran → bun venit + 3 săgeți (clip audio)\n" +
                "3. Tap Check-in / Security / Ciambella → clip pentru fiecare\n" +
                "4. Volum sus, silent OFF";

            Debug.Log("[DiscoverVoiceValidator]\n" + message);
            EditorUtility.DisplayDialog("Discover Voice Validator", message, "OK");
        }
    }
}
#endif
