using System;
using UnityEngine;
using UnityEngine.Events;

namespace RadioDispatch.Voice
{
    // Connections: invoked by UI/VoiceHotkeyRouter for PTT state; publishes events consumed by VoiceCommandController
    // and UI panels to reflect listening and recognized speech.
    /// <summary>
    /// Wraps platform speech recognition. Uses events so platforms can plug in their own providers.
    /// </summary>
    public class VoiceInputManager : MonoBehaviour
    {
        [Tooltip("Invoked when a voice command has been recognized.")]
        public UnityEvent<string> OnVoiceCommandRecognized = new();

        [Tooltip("Invoked when listening starts.")]
        public UnityEvent OnListeningStarted = new();

        [Tooltip("Invoked when listening stops.")]
        public UnityEvent OnListeningStopped = new();

        private bool isListening;

        /// <summary>
        /// Indicates whether the microphone is currently listening for player speech.
        /// </summary>
        public bool IsListening => isListening;

        /// <summary>
        /// Called when the PTT button is pressed to begin capturing microphone input.
        /// </summary>
        public void BeginListening()
        {
            if (isListening)
            {
                Debug.LogWarning("BeginListening called while already listening.");
                return;
            }

            isListening = true;
            OnListeningStarted.Invoke();
            Debug.Log("Voice input: listening started.");
        }

        /// <summary>
        /// Ends listening and optionally submits text immediately if the OS returns it synchronously.
        /// </summary>
        /// <param name="recognizedText">Recognized text if available at stop time (most mobile APIs provide it asynchronously).</param>
        public void EndListening(string recognizedText = null)
        {
            if (!isListening)
            {
                Debug.LogWarning("EndListening called while not listening.");
                return;
            }

            isListening = false;
            OnListeningStopped.Invoke();

            if (!string.IsNullOrWhiteSpace(recognizedText))
            {
                SubmitRecognizedText(recognizedText);
            }
        }

        /// <summary>
        /// Invoked by native speech recognizers once text is available (often after EndListening has been called).
        /// </summary>
        /// <param name="recognizedText">Recognized phrase to forward through the pipeline.</param>
        public void SubmitRecognizedText(string recognizedText)
        {
            if (string.IsNullOrWhiteSpace(recognizedText))
            {
                Debug.LogWarning("Voice input submitted with empty text.");
                return;
            }

            OnVoiceCommandRecognized.Invoke(recognizedText);
            Debug.Log($"Voice input: recognized '{recognizedText}'.");
        }
    }
}
