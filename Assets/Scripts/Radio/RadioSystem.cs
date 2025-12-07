using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RadioDispatch.Radio
{
    /// <summary>
    /// Simple radio logger. In a full build this manages audio beeps and VO playback.
    /// </summary>
    public class RadioSystem : MonoBehaviour
    {
        public event System.Action<string> OnLogEntry;

        [Header("Audio")]
        [SerializeField]
        private RadioAudioProfile audioProfile;

        [SerializeField]
        private bool playAudioOnLog = true;

        [Header("Timing")]
        [SerializeField, Tooltip("Maximum seconds to wait for other audio (unit replies, chatter) before forcing the roger beep on transmission end.")]
        private float rogerBeepMaxDelaySeconds = 10f;

        private readonly Queue<string> transcript = new();
        [SerializeField]
        private int transcriptLimit = 50;

        private AudioSource oneShotSource;
        private AudioSource staticSource;
        private Coroutine rogerBeepRoutine;
        public IReadOnlyCollection<string> Transcript => transcript;

        /// <summary>
        /// Stores a reference to the last clip played so tests and debugging can confirm which voice fired.
        /// </summary>
        public AudioClip LastPlayedVoiceClip { get; private set; }

        private void Awake()
        {
            CacheAudioSources();
        }

        /// <summary>
        /// Injects a profile at runtime.
        /// </summary>
        public void SetAudioProfile(RadioAudioProfile profile)
        {
            audioProfile = profile;
        }

        /// <summary>
        /// Logs a radio message and optionally plays matching audio cues.
        /// </summary>
        public void LogMessage(string speaker, string message)
        {
            if (string.IsNullOrWhiteSpace(speaker) && string.IsNullOrWhiteSpace(message))
            {
                Debug.LogWarning("RadioSystem.LogMessage received empty input.");
                return;
            }

            var speakerPart = string.IsNullOrWhiteSpace(speaker) ? string.Empty : $"{speaker}: ";
            var formatted = $"{speakerPart}{message}".Trim();
            transcript.Enqueue(formatted);
            TrimTranscript();
            Debug.Log(formatted);
            OnLogEntry?.Invoke(formatted);

            if (playAudioOnLog)
            {
                PlayClickIn();
                ScheduleRogerBeep();
            }
        }

        /// <summary>
        /// Starts a transmission with click-in and optional static loop.
        /// </summary>
        public void BeginTransmission()
        {
            CancelRogerSchedule();
            PlayClickIn();
            StartStatic();
        }

        /// <summary>
        /// Ends a transmission, stopping static and playing click-out.
        /// </summary>
        public void EndTransmission()
        {
            StopStatic();
            ScheduleRogerBeep();
        }

        /// <summary>
        /// Plays a unit reply from the configured pool.
        /// </summary>
        public void PlayUnitResponse()
        {
            if (audioProfile == null || audioProfile.UnitResponses == null || audioProfile.UnitResponses.Count == 0)
            {
                return;
            }

            var clip = audioProfile.UnitResponses[Random.Range(0, audioProfile.UnitResponses.Count)];
            if (clip != null)
            {
                EnsureOneShotSource();
                oneShotSource.PlayOneShot(clip, audioProfile.VoiceVolume);
            }
        }

        /// <summary>
        /// Plays a voice clip tied to a specific unit profile. Falls back to the shared pool when unset.
        /// </summary>
        /// <param name="profile">Voice profile assigned to the responding unit.</param>
        /// <param name="responseType">Type of radio response being played.</param>
        public void PlayUnitVoice(UnitVoiceProfile profile, VoiceResponseType responseType)
        {
            var clip = profile?.GetClip(responseType);
            if (clip != null)
            {
                LastPlayedVoiceClip = clip;
                EnsureOneShotSource();
                oneShotSource.PlayOneShot(clip, profile.VoiceVolume);
                return;
            }

            LastPlayedVoiceClip = null;
            // If the profile has no clip for this response we drop back to the shared bank.
            PlayUnitResponse();
        }

        /// <summary>
        /// Allows callers to play a specific audio clip (e.g., custom chatter responses) without adding a new profile.
        /// </summary>
        public void PlayCustomClip(AudioClip clip, float volume = 1f)
        {
            if (clip == null)
            {
                return;
            }

            LastPlayedVoiceClip = clip;
            EnsureOneShotSource();
            oneShotSource.PlayOneShot(clip, volume);
        }

        private void PlayClickIn()
        {
            if (audioProfile?.ClickIn == null)
            {
                return;
            }

            EnsureOneShotSource();
            oneShotSource.PlayOneShot(audioProfile.ClickIn, audioProfile.ClickVolume);
        }

        private void PlayClickOut()
        {
            if (audioProfile?.ClickOut == null)
            {
                return;
            }

            EnsureOneShotSource();
            oneShotSource.PlayOneShot(audioProfile.ClickOut, audioProfile.ClickVolume);
        }

        /// <summary>
        /// Plays the configured roger/acknowledgement beep when available; falls back to the standard click-out.
        /// </summary>
        private void PlayRogerBeep()
        {
            if (audioProfile?.RogerBeep != null)
            {
                EnsureOneShotSource();
                oneShotSource.PlayOneShot(audioProfile.RogerBeep, audioProfile.ClickVolume);
                return;
            }

            PlayClickOut();
        }

        /// <summary>
        /// Schedules a roger beep to fire after any ongoing one-shot audio completes, or forces it after a timeout.
        /// Ensures PTT release plays only once, even if officers are still replying.
        /// </summary>
        private void ScheduleRogerBeep()
        {
            CancelRogerSchedule();
            rogerBeepRoutine = StartCoroutine(WaitAndPlayRoger());
        }

        /// <summary>
        /// Cancels any pending roger beep so a new transmission can manage its own timing.
        /// </summary>
        private void CancelRogerSchedule()
        {
            if (rogerBeepRoutine != null)
            {
                StopCoroutine(rogerBeepRoutine);
                rogerBeepRoutine = null;
            }
        }

        private IEnumerator WaitAndPlayRoger()
        {
            float waited = 0f;
            // If a unit voice or chatter clip is mid-playback, wait for completion but cap at the configured timeout.
            while (oneShotSource != null && oneShotSource.isPlaying && waited < rogerBeepMaxDelaySeconds)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            PlayRogerBeep();
            rogerBeepRoutine = null;
        }

        private void StartStatic()
        {
            if (audioProfile == null || audioProfile.StaticLoop == null || !audioProfile.LoopStaticDuringTransmission)
            {
                return;
            }

            CacheAudioSources();
            staticSource.clip = audioProfile.StaticLoop;
            staticSource.loop = true;
            staticSource.volume = audioProfile.StaticVolume;
            if (!staticSource.isPlaying)
            {
                staticSource.Play();
            }
        }

        private void StopStatic()
        {
            if (staticSource != null && staticSource.isPlaying)
            {
                staticSource.Stop();
            }
        }

        private void CacheAudioSources()
        {
            EnsureOneShotSource();
            if (staticSource == null)
            {
                staticSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void EnsureOneShotSource()
        {
            if (oneShotSource == null)
            {
                oneShotSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void TrimTranscript()
        {
            while (transcript.Count > transcriptLimit)
            {
                transcript.Dequeue();
            }
        }
    }
}
