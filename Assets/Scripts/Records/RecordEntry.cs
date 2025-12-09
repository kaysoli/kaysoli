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

        [Tooltip("Last name portion of the record if provided by a CSV import.")]
        public string LastName;

        [Tooltip("First name portion of the record if provided by a CSV import.")]
        public string FirstName;

        [Tooltip("Middle initial from the CSV import.")]
        public string MiddleInitial;

        [Tooltip("Name suffix (Jr, Sr, III, etc.).")]
        public string NameSuffix;

        [Tooltip("Category so UI and voice responses can distinguish the record type.")]
        public RecordType Type = RecordType.Civilian;

        [Tooltip("Optional custom category label (e.g., 'FBI Agent', 'Secret Service') when a built-in type is insufficient.")]
        public string CustomCategoryLabel;

        [Tooltip("Occupation or assignment to differentiate workers and undercover roles.")]
        public string Occupation;

        [Tooltip("Employer if provided by the CSV import.")]
        public string Employer;

        [Tooltip("Optional vehicle plate or asset tag.")]
        public string VehiclePlate;

        [Tooltip("Status set by the data source (active, closed, sealed, etc.).")]
        public string RecordStatus;

        [Tooltip("Caution flags that should be read out before officer contact.")]
        public string PoliceCautions;

        [Tooltip("Active warrants associated with this record.")]
        public string ActiveWarrants;

        [Tooltip("Known BOLO status if the subject or vehicle is being sought.")]
        public string BoloStatus;

        [Tooltip("Whether the subject is considered armed as reported in the record.")]
        public string IsArmed;

        [Tooltip("Whether the subject has any security clearance.")]
        public string HasSecurityClearance;

        [Tooltip("Whether the subject is enrolled in witness protection.")]
        public string IsWitnessProtection;

        [Tooltip("Aliases captured in the import.")]
        public string Aliases;

        [Tooltip("Known associates captured in the import.")]
        public string KnownAssociates;

        [Tooltip("Addresses and phone contacts for local agencies.")]
        public string LocalPoliceContacts;

        [Tooltip("Concise criminal history text.")]
        public string CriminalHistory;

        [Tooltip("Protective orders associated with this record.")]
        public string ProtectiveOrders;

        [Tooltip("Free-form notes like warrants, caution flags, or descriptors.")]
        [TextArea]
        public string Notes;

        [Tooltip("Optional JSON or key:value metadata blob for custom mod data.")]
        [TextArea]
        public string Metadata;

        [Tooltip("Timestamp of record creation if provided in the CSV source.")]
        public string Timestamp;

        [Tooltip("Last updated timestamp if provided in the CSV source.")]
        public string LastUpdated;

        [Tooltip("Version marker from the upstream data system.")]
        public string DataVersion;

        [Tooltip("Demographics: sex, race, eye color, hair color, and skin tone.")]
        public string Sex;

        public string Race;

        public string EyeColor;

        public string HairColor;

        public string SkinTone;

        [Tooltip("Physical descriptors such as tattoos or scars.")]
        public string DistinguishingMarks;

        [Tooltip("Height description (stored as text to avoid unit mismatches).")]
        public string Height;

        [Tooltip("Weight description (stored as text to avoid unit mismatches).")]
        public string Weight;

        [Tooltip("Date of birth as provided by the CSV source.")]
        public string DateOfBirth;

        [Tooltip("Place of birth from the CSV source.")]
        public string PlaceOfBirth;

        [Tooltip("Driver license number and state of issue.")]
        public string DlNumber;

        public string DlState;

        public string DlClass;

        public string DlExpiration;

        public string DlStatus;

        [Tooltip("State ID values if present.")]
        public string StateIdNumber;

        public string StateIdState;

        public string StateIdExpiration;

        [Tooltip("Passport details if present.")]
        public string PassportNumber;

        public string PassportCountry;

        public string PassportExpiration;

        [Tooltip("National identifiers that should remain text to avoid locale issues.")]
        public string Ssn;

        public string MilitaryId;

        public string GovernmentEmployeeId;

        [Tooltip("Residential address supplied by the CSV source.")]
        public string Address;

        public string AddressVerifiedDate;

        [Tooltip("Education level supplied by the CSV source.")]
        public string EducationLevel;

        [Tooltip("Federal/State/Local identifiers used when cross-referencing other systems.")]
        public string FbiNumber;

        public string StateIdSid;

        public string LocalCid;

        [Tooltip("Registered vehicles associated with the record.")]
        public string RegisteredVehicles;

        [Tooltip("Registered firearms associated with the record.")]
        public string RegisteredFirearms;

        [Tooltip("Optional protective custody or detention status.")]
        public string RegisteredCustody;

        [Tooltip("Optional field for any additional BOLO/flag text not captured elsewhere.")]
        public string AdditionalFlags;

        /// <summary>
        /// Creates a readable one-line summary for radio playback or UI display.
        /// </summary>
        public string BuildSummary()
        {
            var plate = string.IsNullOrWhiteSpace(VehiclePlate) ? string.Empty : $" Plate: {VehiclePlate}.";
            var category = string.IsNullOrWhiteSpace(CustomCategoryLabel) ? Type.ToString() : CustomCategoryLabel;
            var job = string.IsNullOrWhiteSpace(Occupation) ? string.Empty : $" Role: {Occupation}.";
            var status = string.IsNullOrWhiteSpace(RecordStatus) ? string.Empty : $" Status: {RecordStatus}.";
            var bolo = string.IsNullOrWhiteSpace(BoloStatus) ? string.Empty : $" BOLO: {BoloStatus}.";
            var caution = string.IsNullOrWhiteSpace(PoliceCautions) ? string.Empty : $" Caution: {PoliceCautions}.";
            var warrants = string.IsNullOrWhiteSpace(ActiveWarrants) ? string.Empty : $" Warrants: {ActiveWarrants}.";
            var armed = string.IsNullOrWhiteSpace(IsArmed) ? string.Empty : $" Armed: {IsArmed}.";
            var notes = string.IsNullOrWhiteSpace(Notes) ? string.Empty : $" Notes: {Notes}";
            var summaryName = string.IsNullOrWhiteSpace(Name) ? "Unknown" : Name;
            return $"{category} {summaryName} (ID: {Id}).{plate}{job}{status}{bolo}{caution}{warrants}{armed}{notes}".Trim();
        }
    }
}
