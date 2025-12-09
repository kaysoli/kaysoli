using System.Collections.Generic;
using UnityEngine;
using RadioDispatch.Radio;

namespace RadioDispatch.Units
{
    /// <summary>
    /// ScriptableObject describing an individual officer so units can be populated with rich rosters and tied to specific voices.
    /// </summary>
    [CreateAssetMenu(fileName = "OfficerProfile", menuName = "RadioDispatch/Officer Profile")]
    public class OfficerProfile : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this officer (badge, personnel ID, etc.).")]
        public string OfficerId = "OFF-0001";

        [Tooltip("Full name used in records or transcripts.")]
        public string FullName = "Officer";

        [Tooltip("Rank or role (e.g., Officer, Sergeant, Detective).")] 
        public string Rank = "Officer";

        [Tooltip("Optional specialization for lookup or filtering (K9, SWAT, Detective, etc.).")]
        public string Specialty = "Patrol";

        [Header("Assignment")]
        [Tooltip("Optional default call sign to associate with this officer when attached to a unit.")]
        public string PreferredCallSign;

        [Tooltip("Free-form notes such as shift, language, or experience.")]
        public string Notes;

        [Header("Voice")] 
        [Tooltip("Officer-specific voice profile; overrides the unit voice profile when set.")]
        public UnitVoiceProfile VoiceProfile;

        [Tooltip("Optional fallback response clips if the unit profile is missing.")]
        public List<AudioClip> PersonalResponses = new();
    }
}
