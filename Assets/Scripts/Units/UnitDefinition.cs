using System.Collections.Generic;
using UnityEngine;
using RadioDispatch.Radio;

namespace RadioDispatch.Units
{
    /// <summary>
    /// ScriptableObject used to author a single unit with call sign, department, and crew details.
    /// Helps designers drag officers/voices directly into a unit definition for reuse in rosters.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitDefinition", menuName = "RadioDispatch/Unit Definition")]
    public class UnitDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique unit ID (e.g., 3A12 or Unit-21).")]
        public string UnitId = "Unit-01";

        [Tooltip("Call sign as spoken on the radio (e.g., 3-ADAM-12).")]
        public string CallSign = "3-ADAM-12";

        [Tooltip("UI-friendly display name.")]
        public string DisplayName = "Patrol Car";

        [Header("Assignment")]
        public UnitType Type = UnitType.Patrol;
        public UnitDepartment Department = UnitDepartment.Patrol;
        public string DefaultZone = "Central";
        [Tooltip("Optional acknowledgement label shown in UI (e.g., 10-4).")]
        public string DefaultAcknowledgement;

        [Header("Voice & Crew")]
        public UnitVoiceProfile VoiceProfile;
        [Tooltip("Crew assigned to this unit for roster context and voice targeting.")]
        public List<OfficerProfile> Crew = new();

        /// <summary>
        /// Builds a runtime Unit instance from this template.
        /// </summary>
        public Unit CreateInstance()
        {
            return new Unit
            {
                Id = UnitId,
                CallSign = CallSign,
                DisplayName = DisplayName,
                CurrentZone = DefaultZone,
                Status = UnitStatus.Available,
                Type = Type,
                Department = Department,
                AcknowledgementLabel = DefaultAcknowledgement,
                VoiceProfile = VoiceProfile,
                Crew = Crew != null ? new List<OfficerProfile>(Crew) : new List<OfficerProfile>()
            };
        }
    }
}
