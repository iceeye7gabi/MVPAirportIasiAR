using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace AirportAR.AR
{
    public enum DiscoverSpeechId
    {
        Welcome,
        Forward,
        Left,
        Right
    }

    /// <summary>
    /// Plays pre-recorded Romanian voice clips. iOS uses native AVAudioPlayer (bypasses Unity audio).
    /// </summary>
    public class MobileSpeechService : MonoBehaviour
    {
        public static MobileSpeechService Instance { get; private set; }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void _DiscoverPlayAudioFile([MarshalAs(UnmanagedType.LPUTF8Str)] string path);

        [DllImport("__Internal")]
        static extern void _DiscoverStopAudio();
#endif

        AudioSource audioSource;
        Coroutine playRoutine;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

#if !UNITY_IOS || UNITY_EDITOR
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = 1f;
            audioSource.spatialBlend = 0f;
#endif
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Play(DiscoverSpeechId speechId)
        {
            string fileName = GetFileName(speechId);
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogWarning($"[MobileSpeechService] Unknown speech id: {speechId}");
                return;
            }

            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }

#if UNITY_IOS && !UNITY_EDITOR
            string path = Path.Combine(Application.streamingAssetsPath, "Audio/Discover", fileName);
            Debug.Log($"[MobileSpeechService] Play native: {path} (exists={File.Exists(path)})");
            _DiscoverPlayAudioFile(path);
#else
            playRoutine = StartCoroutine(PlayClipRoutine(speechId, fileName));
#endif
        }

        public void StopSpeaking()
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }

#if UNITY_IOS && !UNITY_EDITOR
            _DiscoverStopAudio();
#else
            audioSource?.Stop();
#endif
        }

        IEnumerator PlayClipRoutine(DiscoverSpeechId speechId, string fileName)
        {
            AudioListener.pause = false;
            AudioListener.volume = 1f;

            AudioClip clip = Resources.Load<AudioClip>($"Audio/Discover/{Path.GetFileNameWithoutExtension(fileName)}");
            if (clip == null)
            {
                string streamingPath = Path.Combine(Application.streamingAssetsPath, "Audio/Discover", fileName);
                if (File.Exists(streamingPath))
                {
                    using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + streamingPath, AudioType.WAV))
                    {
                        yield return request.SendWebRequest();
                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            clip = DownloadHandlerAudioClip.GetContent(request);
                        }
                    }
                }
            }

            if (clip == null)
            {
                Debug.LogWarning($"[MobileSpeechService] Missing clip for {speechId}");
                playRoutine = null;
                yield break;
            }

            Debug.Log($"[MobileSpeechService] Play clip: {speechId}");
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();

            while (audioSource.isPlaying)
            {
                yield return null;
            }

            playRoutine = null;
        }

        static string GetFileName(DiscoverSpeechId speechId)
        {
            return speechId switch
            {
                DiscoverSpeechId.Welcome => "welcome.wav",
                DiscoverSpeechId.Forward => "forward.wav",
                DiscoverSpeechId.Left => "left.wav",
                DiscoverSpeechId.Right => "right.wav",
                _ => null
            };
        }
    }
}
