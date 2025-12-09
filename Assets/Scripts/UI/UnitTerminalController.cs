using UnityEngine;
using RadioDispatch.Calls;
using RadioDispatch.Radio;
using RadioDispatch.Units;

namespace RadioDispatch.UI
{
    /// <summary>
    /// Provides terminal-style interactions for unit rows so the dispatcher can select a unit,
    /// update its status, assign it to a call, or mark a non-destructive acknowledgement label ("10-4").
    /// </summary>
    public class UnitTerminalController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private UnitManager unitManager;

        [SerializeField]
        private CallManager callManager;

        [SerializeField]
        private RadioSystem radioSystem;

        private Unit selectedUnit;

        /// <summary>
        /// Initializes dependencies for tests or runtime injection.
        /// </summary>
        public void Initialize(UnitManager units, CallManager calls, RadioSystem radio)
        {
            unitManager = units;
            callManager = calls;
            radioSystem = radio;
        }

        /// <summary>
        /// Selects a unit by ID so UI buttons/dropdowns can act on it.
        /// </summary>
        /// <param name="unitId">Identifier shown in the UI (e.g., "21").</param>
        public void SelectUnit(string unitId)
        {
            if (unitManager == null)
            {
                Debug.LogWarning("UnitTerminalController missing UnitManager; cannot select unit.");
                return;
            }

            selectedUnit = unitManager.GetUnitById(unitId);
            if (selectedUnit == null)
            {
                radioSystem?.LogMessage("Terminal", $"Unit {unitId} not found.");
            }
            else
            {
                radioSystem?.LogMessage("Terminal", $"Selected {selectedUnit.DisplayName}.");
            }
        }

        /// <summary>
        /// Updates the selected unit's status, mirroring a manual status change from the terminal.
        /// </summary>
        /// <param name="status">New status to apply.</param>
        public void UpdateSelectedUnitStatus(UnitStatus status)
        {
            if (!EnsureSelection())
            {
                return;
            }

            unitManager.ChangeUnitStatus(selectedUnit, status);
            radioSystem?.LogMessage(selectedUnit.DisplayName, $"Status set via terminal: {status}.");
        }

        /// <summary>
        /// Assigns the selected unit to the specified call ID when the dispatcher taps the UI option.
        /// </summary>
        /// <param name="callId">Call identifier to attach the unit to.</param>
        public void AssignSelectedToCall(string callId)
        {
            if (!EnsureSelection())
            {
                return;
            }

            if (callManager == null)
            {
                Debug.LogWarning("UnitTerminalController missing CallManager; cannot assign.");
                return;
            }

            var call = callManager.GetCallById(callId);
            if (call == null)
            {
                radioSystem?.LogMessage("Terminal", $"Call {callId} not found.");
                return;
            }

            unitManager.AssignUnitToCall(selectedUnit, call);
            radioSystem?.LogMessage("Terminal", $"{selectedUnit.DisplayName} assigned to call {call.Id} from terminal.");
        }

        /// <summary>
        /// Applies a user-facing acknowledgement label (defaults to "10-4") without affecting status.
        /// </summary>
        /// <param name="label">Freeform acknowledgement text.</param>
        public void AcknowledgeSelected(string label = "10-4")
        {
            if (!EnsureSelection())
            {
                return;
            }

            unitManager.SetAcknowledgement(selectedUnit, label);
            radioSystem?.LogMessage(selectedUnit.DisplayName, $"Acknowledgement noted: {label}.");
        }

        /// <summary>
        /// Clears the current selection when the user deselects a row.
        /// </summary>
        public void ClearSelection()
        {
            selectedUnit = null;
        }

        private bool EnsureSelection()
        {
            if (selectedUnit != null)
            {
                return true;
            }

            radioSystem?.LogMessage("Terminal", "No unit selected.");
            return false;
        }
    }
}
