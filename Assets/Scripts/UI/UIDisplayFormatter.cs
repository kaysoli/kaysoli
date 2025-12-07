using System;
using System.Globalization;
using RadioDispatch.Calls;
using RadioDispatch.Units;

namespace RadioDispatch.UI
{
    /// <summary>
    /// Helper methods to format dispatcher data for on-screen display. Keeps UI code thin and testable.
    /// </summary>
    public static class UIDisplayFormatter
    {
        private const string TimeFormat = "mm\:ss";

        /// <summary>
        /// Builds a compact summary line for a call row, including priority and elapsed time.
        /// </summary>
        /// <param name="call">Call information to format.</param>
        /// <returns>User-friendly summary string.</returns>
        public static string BuildCallSummary(CallData call)
        {
            if (call == null)
            {
                return "No call data";
            }

            var priority = call.Priority.ToString();
            var timer = TimeSpan.FromSeconds(Math.Max(call.TimeSinceCreated, 0f));
            var timerText = timer.ToString(TimeFormat, CultureInfo.InvariantCulture);
            var location = string.IsNullOrWhiteSpace(call.Location) ? "Unknown" : call.Location;
            var title = string.IsNullOrWhiteSpace(call.Title) ? "Untitled" : call.Title;

            return $"#{call.Id} | {title} @ {location} | Priority: {priority} | {timerText}";
        }

        /// <summary>
        /// Builds a short descriptor for a dispatch unit showing type, status, and zone.
        /// </summary>
        /// <param name="unit">Unit data to format.</param>
        /// <returns>Readable unit description.</returns>
        public static string BuildUnitSummary(Unit unit)
        {
            if (unit == null)
            {
                return "No unit data";
            }

            var zone = string.IsNullOrWhiteSpace(unit.CurrentZone) ? "Unknown" : unit.CurrentZone;
            var type = string.IsNullOrWhiteSpace(unit.Type) ? "Unit" : unit.Type;
            var display = string.IsNullOrWhiteSpace(unit.DisplayName) ? unit.Id : unit.DisplayName;
            var acknowledgement = string.IsNullOrWhiteSpace(unit.AcknowledgementLabel)
                ? string.Empty
                : $" | Ack: {unit.AcknowledgementLabel}";

            return $"{display} ({type}) | {unit.Status} | Zone: {zone}{acknowledgement}";
        }
    }
}
