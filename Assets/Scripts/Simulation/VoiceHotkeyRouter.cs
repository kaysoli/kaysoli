using System.Collections.Generic;
using UnityEngine;
using RadioDispatch.Voice;
using RadioDispatch.Radio;

namespace RadioDispatch.Simulation
{
    /// <summary>
    /// Provides a reference implementation for hotkey-based push-to-talk and routes recognized text from a provider into the
    /// existing voice pipeline (VoiceInputManager → CommandInterpreter → CommandExecutor).
    /// </summary>
    [DisallowMultipleComponent]
    public class VoiceHotkeyRouter : MonoBehaviour
    {
        [Header("Components")]
        [Tooltip("The input manager already used by UI PTT buttons. Hotkey presses will call Begin/EndListening on it.")]
        [SerializeField]
        private VoiceInputManager voiceInputManager;

        [Tooltip("Optional radio system used to play click/roger beeps when the hotkey is pressed without UI involvement.")]
        [SerializeField]
        private RadioSystem radioSystem;

        [Header("Hotkey")]
        [Tooltip("Keyboard key to hold for push-to-talk in editor/desktop playtests.")]
        [SerializeField]
        private KeyCode pttKey = KeyCode.RightAlt;

        [Tooltip("If true, the router will emit a Begin/End transmission when the key is pressed/released.")]
        [SerializeField]
        private bool playClicksForHotkey = true;

        [Header("Providers")]
        [Tooltip("Optional provider that will supply recognized text. If not set, the router will rely on the default queue provider.")]
        [SerializeField]
        private ScriptableSpeechProvider speechProvider;

        private ISpeechToTextProvider runtimeProvider;
        private bool isPressed;

        private void Awake()
        {
            // Initialize with the configured provider; tests or bootstrap code can override via SetProvider.
            SetProvider(speechProvider != null ? speechProvider.CreateRuntimeProvider() : new QueuedSpeechProvider());
        }

        private void Update()
        {
            if (voiceInputManager == null)
            {
                return;
            }

            if (Input.GetKeyDown(pttKey))
            {
                isPressed = true;
                voiceInputManager.BeginListening();
                runtimeProvider.BeginCapture();
                if (playClicksForHotkey && radioSystem != null)
                {
                    radioSystem.BeginTransmission();
                }
            }

            if (Input.GetKeyUp(pttKey) && isPressed)
            {
                isPressed = false;
                voiceInputManager.EndListening();
                runtimeProvider.EndCapture();
                if (playClicksForHotkey && radioSystem != null)
                {
                    radioSystem.EndTransmission();
                }
            }
        }

        /// <summary>
        /// Allows callers (including tests) to inject a custom provider such as a plugin-backed recognizer or a stub.
        /// </summary>
        public void SetProvider(ISpeechToTextProvider provider)
        {
            if (runtimeProvider != null)
            {
                runtimeProvider.OnRecognized -= HandleRecognized;
            }

            runtimeProvider = provider;

            if (runtimeProvider != null)
            {
                runtimeProvider.OnRecognized += HandleRecognized;
            }
        }

        private void HandleRecognized(string text)
        {
            if (voiceInputManager == null)
            {
                Debug.LogWarning("VoiceHotkeyRouter has no VoiceInputManager to forward recognized text to.");
                return;
            }

            voiceInputManager.SubmitRecognizedText(text);
        }
    }

    /// <summary>
    /// Abstraction for speech providers so you can plug in Whisper, Vosk, or any other offline STT system.
    /// </summary>
    public interface ISpeechToTextProvider
    {
        /// <summary>
        /// Raised when the provider has recognized a phrase. The router forwards this directly to VoiceInputManager.
        /// </summary>
        event System.Action<string> OnRecognized;

        /// <summary>
        /// Begin microphone capture.
        /// </summary>
        void BeginCapture();

        /// <summary>
        /// End capture and emit recognition if available.
        /// </summary>
        void EndCapture();
    }

    /// <summary>
    /// Scriptable factory so different providers can be swapped per scene/profile.
    /// </summary>
    public abstract class ScriptableSpeechProvider : ScriptableObject
    {
        public abstract ISpeechToTextProvider CreateRuntimeProvider();
    }

    /// <summary>
    /// Simple queue-driven provider used for editor tests; call EnqueuePhrase from UI buttons to simulate recognition.
    /// </summary>
    public class QueuedSpeechProvider : ISpeechToTextProvider
    {
        public event System.Action<string> OnRecognized;

        private readonly Queue<string> queue = new();

        public void BeginCapture()
        {
            // Nothing to do for the queue provider; it simply waits for queued text.
        }

        public void EndCapture()
        {
            if (queue.Count > 0)
            {
                OnRecognized?.Invoke(queue.Dequeue());
            }
        }

        /// <summary>
        /// Adds a phrase to the queue so the next EndCapture will emit it.
        /// </summary>
        public void EnqueuePhrase(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                Debug.LogWarning("QueuedSpeechProvider received an empty phrase.");
                return;
            }

            queue.Enqueue(text);
        }
    }
}
