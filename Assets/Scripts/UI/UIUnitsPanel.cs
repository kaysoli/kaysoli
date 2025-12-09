using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RadioDispatch.Units;

namespace RadioDispatch.UI
{
    /// <summary>
    /// Handles population of the units list panel in split-screen mode.
    /// </summary>
    public class UIUnitsPanel : MonoBehaviour
    {
        [SerializeField]
        private UnitManager unitManager;

        [SerializeField]
        private Transform unitListContainer;

        [SerializeField]
        private GameObject unitRowPrefab;

        [Header("Terminal Actions")]
        [SerializeField]
        private UnitTerminalController terminalController;

        private readonly List<GameObject> pooledRows = new();

        private void OnEnable()
        {
            Subscribe();
            RefreshUnits();
        }

        private void OnDisable()
        {
            Unsubscribe();
            ClearRows();
        }

        private void Subscribe()
        {
            if (unitManager == null)
            {
                Debug.LogWarning("UIUnitsPanel is missing a UnitManager reference.");
                return;
            }

            unitManager.OnUnitUpdated += HandleUnitUpdated;
        }

        private void Unsubscribe()
        {
            if (unitManager != null)
            {
                unitManager.OnUnitUpdated -= HandleUnitUpdated;
            }
        }

        private void HandleUnitUpdated(Unit _)
        {
            RefreshUnits();
        }

        /// <summary>
        /// Rebuilds the unit list UI to reflect current statuses.
        /// </summary>
        public void RefreshUnits()
        {
            if (unitManager == null || unitListContainer == null || unitRowPrefab == null)
            {
                return;
            }

            ClearRows();

            foreach (var unit in GetUnitsSnapshot())
            {
                var row = Instantiate(unitRowPrefab, unitListContainer);
                pooledRows.Add(row);

                var presenter = row.GetComponent<UIUnitRow>();
                var textOnly = row.GetComponentInChildren<Text>();
                if (presenter != null)
                {
                    presenter.Bind(unit);
                }
                else if (textOnly != null)
                {
                    textOnly.text = UIDisplayFormatter.BuildUnitSummary(unit);
                }
            }
        }

        /// <summary>
        /// Allows UI buttons to pick a unit by ID so follow-up terminal options (assign, acknowledge) can run.
        /// </summary>
        /// <param name="unitId">Identifier such as "21".</param>
        public void SelectUnitById(string unitId)
        {
            terminalController?.SelectUnit(unitId);
        }

        /// <summary>
        /// Assigns the currently selected unit to a call when triggered by a UI control.
        /// </summary>
        public void AssignSelectedToCall(string callId)
        {
            terminalController?.AssignSelectedToCall(callId);
        }

        /// <summary>
        /// Marks the selected unit acknowledged with a customizable label (defaults to "10-4").
        /// </summary>
        public void MarkSelectedAcknowledged(string label)
        {
            terminalController?.AcknowledgeSelected(label);
        }

        /// <summary>
        /// Updates status for the selected unit from a dropdown (value converted to UnitStatus by inspector binding).
        /// </summary>
        public void UpdateSelectedStatus(UnitStatus status)
        {
            terminalController?.UpdateSelectedUnitStatus(status);
        }

        private IReadOnlyList<Unit> GetUnitsSnapshot()
        {
            return new List<Unit>(unitManager?.GetAllUnits() ?? new List<Unit>());
        }

        private void ClearRows()
        {
            foreach (var row in pooledRows)
            {
                if (row != null)
                {
                    Destroy(row);
                }
            }

            pooledRows.Clear();
        }
    }
}
