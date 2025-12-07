using UnityEngine;
using UnityEngine.UI;
using RadioDispatch.Units;

namespace RadioDispatch.UI
{
    /// <summary>
    /// Presenter for a single unit entry in the units panel.
    /// </summary>
    public class UIUnitRow : MonoBehaviour
    {
        [SerializeField]
        private Text nameLabel;

        [SerializeField]
        private Text statusLabel;

        [SerializeField]
        private Text zoneLabel;

        [SerializeField]
        private Text typeLabel;

        [SerializeField]
        private Text acknowledgementLabel;

        /// <summary>
        /// Populates the UI fields from a unit model.
        /// </summary>
        /// <param name="unit">Unit data to render.</param>
        public void Bind(Unit unit)
        {
            if (unit == null)
            {
                Debug.LogWarning("UIUnitRow received null unit data.");
                return;
            }

            if (nameLabel != null)
            {
                nameLabel.text = string.IsNullOrWhiteSpace(unit.DisplayName) ? unit.Id : unit.DisplayName;
            }

            if (statusLabel != null)
            {
                statusLabel.text = unit.Status.ToString();
            }

            if (zoneLabel != null)
            {
                zoneLabel.text = string.IsNullOrWhiteSpace(unit.CurrentZone) ? "Unknown" : unit.CurrentZone;
            }

            if (typeLabel != null)
            {
                typeLabel.text = unit.Type == UnitType.Unknown ? "General" : unit.Type.ToString();
            }

            if (acknowledgementLabel != null)
            {
                acknowledgementLabel.text = string.IsNullOrWhiteSpace(unit.AcknowledgementLabel)
                    ? string.Empty
                    : $"Ack: {unit.AcknowledgementLabel}";
            }
        }
    }
}
