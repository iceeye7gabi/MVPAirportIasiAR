#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

namespace AirportAR.Editor
{
    public static class IosSpeechPlistPostProcessor
    {
        const string SpeechUsageText =
            "Aplicatia foloseste recunoasterea vocala pentru asistentul din modul Descopera aeroportul.";

        const string MicrophoneUsageText =
            "Microfonul este necesar pentru comenzi vocale in modul Descopera aeroportul.";

        [PostProcessBuild(999)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS)
            {
                return;
            }

            string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            plist.root.SetString("NSSpeechRecognitionUsageDescription", SpeechUsageText);
            plist.root.SetString("NSMicrophoneUsageDescription", MicrophoneUsageText);

            plist.WriteToFile(plistPath);

            string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);

            AddSpeechFrameworks(project, project.GetUnityMainTargetGuid());

#if UNITY_2019_3_OR_NEWER
            AddSpeechFrameworks(project, project.GetUnityFrameworkTargetGuid());
#endif

            project.WriteToFile(projectPath);
        }

        static void AddSpeechFrameworks(PBXProject project, string targetGuid)
        {
            if (string.IsNullOrEmpty(targetGuid))
            {
                return;
            }

            // Required for AVSpeechSynthesizer, AVAudioEngine, AVAudioSession.
            project.AddFrameworkToProject(targetGuid, "AVFoundation.framework", false);
            // Required for SFSpeechRecognizer / SFSpeechAudioBufferRecognitionRequest.
            project.AddFrameworkToProject(targetGuid, "Speech.framework", false);
        }
    }
}
#endif
