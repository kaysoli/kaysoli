using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RadioDispatch.Calls;

namespace RadioDispatch.Units
{
    /// <summary>
    /// Maintains the pool of available units and handles assignment logic.
    /// </summary>
    public class UnitManager : MonoBehaviour
    {
        public event Action<Unit> OnUnitUpdated;

        [SerializeField]
        private List<Unit> units = new();

        [SerializeField]
        private UnitRoster defaultRoster;

        /// <summary>
        /// Provides read-only access to the full roster.
        /// </summary>
        public IReadOnlyList<Unit> GetAllUnits()
        {
            return units.AsReadOnly();
        }

        private void Awake()
        {
            if (defaultRoster != null && defaultRoster.Units != null && defaultRoster.Units.Count > 0)
            {
                SetUnits(defaultRoster.GetClonedUnits());
            }
        }

        /// <summary>
        /// Replaces the current roster with a provided list (useful for seeding content or tests).
        /// </summary>
        /// <param name="seedUnits">Units to load into the manager.</param>
        public void SetUnits(IEnumerable<Unit> seedUnits)
        {
            units = seedUnits?.Select(CloneUnit).ToList() ?? new List<Unit>();
            foreach (var unit in units)
            {
                NotifyUnitChanged(unit);
            }
        }

        /// <summary>
        /// Adds a single unit to the active roster at runtime (useful for testing or CSV-driven generation).
        /// The unit is cloned so the original template is not mutated.
        /// </summary>
        /// <param name="unitTemplate">Template containing ID, call sign, voice profile, etc.</param>
        public Unit AddUnit(Unit unitTemplate)
        {
            if (unitTemplate == null)
            {
                Debug.LogWarning("AddUnit received a null template.");
                return null;
            }

            var clone = CloneUnit(unitTemplate);
            units.Add(clone);
            NotifyUnitChanged(clone);
            return clone;
        }

        /// <summary>
        /// Returns units that are currently marked as available.
        /// </summary>
        public List<Unit> GetAvailableUnits()
        {
            return units.Where(u => u.Status == UnitStatus.Available).ToList();
        }

        /// <summary>
        /// Returns units in a specific status bucket (e.g., Pursuit or Panic) for targeted handling.
        /// </summary>
        public List<Unit> GetUnitsByStatus(UnitStatus status)
        {
            return units.Where(u => u.Status == status).ToList();
        }

        /// <summary>
        /// Retrieves all units matching a specific type (e.g., SWAT/K9/EMS).
        /// </summary>
        public List<Unit> GetUnitsByType(UnitType type)
        {
            return units.Where(u => u.Type == type).ToList();
        }

        /// <summary>
        /// Assigns a specific unit to a call and updates its status.
        /// </summary>
        public void AssignUnitToCall(Unit unit, CallData call)
        {
            if (unit == null || call == null)
            {
                Debug.LogWarning("AssignUnitToCall received null arguments.");
                return;
            }

            unit.Status = UnitStatus.EnRoute;
            unit.CurrentCall = call;
            call.AssignedUnits.Add(unit);
            NotifyUnitChanged(unit);
        }

        /// <summary>
        /// Updates the status for a unit, ensuring listeners are notified.
        /// </summary>
        public void ChangeUnitStatus(Unit unit, UnitStatus status)
        {
            if (unit == null)
            {
                Debug.LogWarning("ChangeUnitStatus received a null unit.");
                return;
            }

            unit.Status = status;
            NotifyUnitChanged(unit);
        }

        /// <summary>
        /// Marks a unit as in a panic state so downstream systems can respond appropriately.
        /// </summary>
        /// <param name="unit">Unit that triggered panic.</param>
        public void FlagPanic(Unit unit)
        {
            ChangeUnitStatus(unit, UnitStatus.Panic);
        }

        /// <summary>
        /// Applies a non-destructive acknowledgement label (e.g., "10-4") so the dispatcher can track confirmations.
        /// </summary>
        /// <param name="unit">Unit to flag.</param>
        /// <param name="label">Label to store. Null/empty clears the acknowledgement.</param>
        public void SetAcknowledgement(Unit unit, string label)
        {
            if (unit == null)
            {
                Debug.LogWarning("SetAcknowledgement received a null unit.");
                return;
            }

            unit.AcknowledgementLabel = label;
            NotifyUnitChanged(unit);
        }

        public Unit GetUnitById(string unitId)
        {
            return units.FirstOrDefault(u => u.Id.Equals(unitId, StringComparison.OrdinalIgnoreCase));
        }

        private static Unit CloneUnit(Unit source)
        {
            if (source == null)
            {
                return null;
            }

            return new Unit
            {
                Id = source.Id,
                CallSign = source.CallSign,
                DisplayName = source.DisplayName,
                Status = source.Status,
                CurrentZone = source.CurrentZone,
                CurrentCall = source.CurrentCall,
                Type = source.Type,
                AcknowledgementLabel = source.AcknowledgementLabel,
                VoiceProfile = source.VoiceProfile
            };
        }

        private void NotifyUnitChanged(Unit unit)
        {
            OnUnitUpdated?.Invoke(unit);
            Debug.Log($"Unit updated: {unit.DisplayName} -> {unit.Status}");
        }
    }
}
