using UnityEngine;
using RadioDispatch.Calls;
using RadioDispatch.Radio;
using RadioDispatch.Units;
using RadioDispatch.Voice;
using RadioDispatch.Records;

namespace RadioDispatch.UI
{
    // Connections: subscribes to CallManager/UnitManager/RadioSystem events, forwards PTT to VoiceInputManager,
    // and orchestrates UI panels (radio/terminal/units/records) so visual state follows dispatcher actions.
    /// <summary>
    /// Coordinates simple UI callbacks for the prototype. Real UI will be added later.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        private CallManager callManager;

        [SerializeField]
        private UnitManager unitManager;

        [SerializeField]
        private RadioSystem radioSystem;

        [Header("UI Panels")]
        [SerializeField]
        private UIRadioPanel radioPanel;

        [SerializeField]
        private UITerminalPanel terminalPanel;

        [SerializeField]
        private UIUnitsPanel unitsPanel;

        [SerializeField]
        private UISwipeController swipeController;

        [SerializeField]
        private RecordsTerminalPanel recordsPanel;

        [Header("Input")]
        [SerializeField]
        private VoiceInputManager voiceInputManager;

        [SerializeField]
        private VoiceCommandController voiceCommandController;

        [SerializeField]
        private RecordsManager recordsManager;

        private void Awake()
        {
            if (unitManager != null && radioSystem != null)
            {
                unitManager.OnUnitUpdated += HandleUnitUpdated;
            }

            if (recordsPanel != null)
            {
                recordsPanel.Initialize(recordsManager, radioSystem);
            }
        }

        private void OnEnable()
        {
            SubscribeToFeedback();
        }

        private void OnDisable()
        {
            UnsubscribeFromFeedback();
            if (unitManager != null)
            {
                unitManager.OnUnitUpdated -= HandleUnitUpdated;
            }
        }

        /// <summary>
        /// Example hook for UI button to assign the nearest unit to the latest call.
        /// </summary>
        public void AssignNearestUnitToLatestCall()
        {
            var calls = callManager.GetActiveCalls();
            if (calls.Count == 0)
            {
                radioSystem.LogMessage("UI", "No active calls available.");
                return;
            }

            var latestCall = calls[^1];
            var available = unitManager.GetAvailableUnits();
            if (available.Count == 0)
            {
                radioSystem.LogMessage("UI", "No available units to dispatch.");
                return;
            }

            unitManager.AssignUnitToCall(available[0], latestCall);
            radioSystem.LogMessage("UI", $"{available[0].DisplayName} assigned to call {latestCall.Id}.");
        }

        /// <summary>
        /// Hook for the PTT button press event to begin listening and show UI feedback.
        /// </summary>
        public void OnPttPressed()
        {
            voiceInputManager?.BeginListening();
            radioPanel?.SetListeningState(true);
            radioSystem?.BeginTransmission();
        }

        /// <summary>
        /// Hook for the PTT button release event to stop listening and clear UI feedback.
        /// </summary>
        public void OnPttReleased()
        {
            voiceInputManager?.EndListening();
            radioPanel?.SetListeningState(false);
            radioSystem?.EndTransmission();
            unitsPanel?.RefreshUnits();
        }

        /// <summary>
        /// Toggles the split-screen panel that reveals terminal and unit lists.
        /// </summary>
        public void ToggleSplitPanel()
        {
            swipeController?.ToggleSplit();
        }

        /// <summary>
        /// Forces a refresh of call and unit panels when data changes externally.
        /// </summary>
        public void RefreshPanels()
        {
            terminalPanel?.RefreshCallList();
            unitsPanel?.RefreshUnits();
        }

        private void SubscribeToFeedback()
        {
            if (voiceCommandController == null || radioPanel == null)
            {
                return;
            }

            voiceCommandController.OnFeedbackGenerated.RemoveListener(radioPanel.ShowFeedback);
            voiceCommandController.OnFeedbackGenerated.AddListener(radioPanel.ShowFeedback);
        }

        private void UnsubscribeFromFeedback()
        {
            if (voiceCommandController == null || radioPanel == null)
            {
                return;
            }

            voiceCommandController.OnFeedbackGenerated.RemoveListener(radioPanel.ShowFeedback);
        }

        private void HandleUnitUpdated(Unit unit)
        {
            if (unit == null || radioSystem == null)
            {
                return;
            }

            radioSystem.LogMessage(unit.DisplayName, $"Status updated: {unit.Status} {unit.AcknowledgementLabel ?? string.Empty}".Trim());
        }
    }
}
