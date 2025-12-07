using System;
using UnityEngine;

namespace RadioDispatch.Records
{
    /// <summary>
    /// Represents a single searchable record for civilians, officers, prisoners, or vehicles.
    /// </summary>
    [Serializable]
    public class RecordEntry
    {
        [Tooltip("Unique identifier such as license plate, subject ID, or badge number.")]
        public string Id;

        [Tooltip("Display name of the person or vehicle owner.")]
        public string Name;

        [Tooltip("Category so UI and voice responses can distinguish the record type.")]
        public RecordType Type = RecordType.Civilian;

        [Tooltip("Optional vehicle plate or asset tag.")]
        public string VehiclePlate;

        [Tooltip("Free-form notes like warrants, caution flags, or descriptors.")]
        [TextArea]
        public string Notes;

        [Tooltip("Optional JSON or key:value metadata blob for custom mod data.")]
        [TextArea]
        public string Metadata;

        /// <summary>
        /// Creates a readable one-line summary for radio playback or UI display.
        /// </summary>
        public string BuildSummary()
        {
            var plate = string.IsNullOrWhiteSpace(VehiclePlate) ? string.Empty : $" Plate: {VehiclePlate}.";
            var category = Type.ToString();
            var cleanNotes = string.IsNullOrWhiteSpace(Notes) ? string.Empty : $" Notes: {Notes}";
            return $"{category} {Name} (ID: {Id}).{plate}{cleanNotes}".Trim();
        }
    }
}
