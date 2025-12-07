using System;
using UnityEngine;
using UnityEngine.UI;
using RadioDispatch.Calls;
using RadioDispatch.Radio;

namespace RadioDispatch.UI
{
    /// <summary>
    /// Controls the always-visible radio portion of the HUD (top bar, transcript, PTT feedback).
    /// </summary>
    public class UIRadioPanel : MonoBehaviour
    {
        [Header("Data Sources")]
        [SerializeField]
        private CallManager callManager;

        [SerializeField]
        private RadioSystem radioSystem;

        [Header("UI References")]
        [SerializeField]
        private Text activeCallsLabel;

        [SerializeField]
        private Text timeLabel;

        [SerializeField]
        private Text listeningIndicator;

        [SerializeField]
        private Transform transcriptContainer;

        [SerializeField]
        private GameObject transcriptEntryPrefab;

        [SerializeField, Tooltip("Optional limit to avoid overflowing the transcript container.")]
        private int visualTranscriptLimit = 30;

        [SerializeField, Tooltip("Shows color-coded feedback for parsed voice commands.")]
        private Text feedbackLabel;

        private Action<CallData> callCountHandler;

        private void Awake()
        {
            RefreshActiveCalls();
            UpdateClockLabel();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            UpdateClockLabel();
        }

        /// <summary>
        /// Shows or hides the listening indicator when PTT is held.
        /// </summary>
        /// <param name="listening">Whether the system is currently listening to player speech.</param>
        public void SetListeningState(bool listening)
        {
            if (listeningIndicator != null)
            {
                listeningIndicator.gameObject.SetActive(listening);
                listeningIndicator.text = listening ? "Listening…" : string.Empty;
            }
        }

        /// <summary>
        /// Writes feedback text (with rich text coloring) to the HUD.
        /// </summary>
        public void ShowFeedback(string message)
        {
            if (feedbackLabel == null)
            {
                return;
            }

            feedbackLabel.supportRichText = true;
            feedbackLabel.text = message;
        }

        private void SubscribeToEvents()
        {
            if (callManager != null)
            {
                callCountHandler = _ => RefreshActiveCalls();
                CallManager.OnNewCall += callCountHandler;
                CallManager.OnCallResolved += callCountHandler;
                CallManager.OnCallUpdated += callCountHandler;
            }
            else
            {
                Debug.LogWarning("UIRadioPanel has no CallManager reference.");
            }

            if (radioSystem != null)
            {
                radioSystem.OnLogEntry += HandleTranscriptEntry;
            }
            else
            {
                Debug.LogWarning("UIRadioPanel has no RadioSystem reference.");
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (callCountHandler != null)
            {
                CallManager.OnNewCall -= callCountHandler;
                CallManager.OnCallResolved -= callCountHandler;
                CallManager.OnCallUpdated -= callCountHandler;
                callCountHandler = null;
            }

            if (radioSystem != null)
            {
                radioSystem.OnLogEntry -= HandleTranscriptEntry;
            }
        }

        private void RefreshActiveCalls()
        {
            if (activeCallsLabel == null || callManager == null)
            {
                return;
            }

            var count = callManager.GetActiveCalls().Count;
            activeCallsLabel.text = $"Active Calls: {count}";
        }

        private void UpdateClockLabel()
        {
            if (timeLabel == null)
            {
                return;
            }

            timeLabel.text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void HandleTranscriptEntry(string entry)
        {
            if (transcriptContainer == null || transcriptEntryPrefab == null)
            {
                return;
            }

            var instance = Instantiate(transcriptEntryPrefab, transcriptContainer);
            var textComponent = instance.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.text = entry;
            }

            TrimTranscript();
        }

        private void TrimTranscript()
        {
            if (visualTranscriptLimit <= 0 || transcriptContainer == null)
            {
                return;
            }

            while (transcriptContainer.childCount > visualTranscriptLimit)
            {
                Destroy(transcriptContainer.GetChild(0).gameObject);
            }
        }
    }
}
