using System.Collections.Generic;
using UnityEngine;

namespace RadioDispatch.Radio
{
    /// <summary>
    /// Categorizes officer response types so a single unit can play consistent audio variants for different events.
    /// </summary>
    public enum VoiceResponseType
    {
        Acknowledgement,
        Assignment,
        Status,
        Backup,
        Panic,
        Custody,
        Termination
    }

    /// <summary>
    /// Designer-authored audio container that lets you drag officer voice clips into buckets and bind them to a specific unit.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitVoiceProfile", menuName = "RadioDispatch/Unit Voice Profile")]
    public class UnitVoiceProfile : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Optional label so you can track which actor/voice set is used for a unit.")]
        public string VoiceName = "Officer";

        [Header("Responses")]
        [Tooltip("Used for simple 10-4 acknowledgements or short confirmations.")]
        public List<AudioClip> Acknowledgements = new();

        [Tooltip("Played when the dispatcher assigns the unit to a call.")]
        public List<AudioClip> AssignmentReplies = new();

        [Tooltip("Used when a unit reports or is asked for status.")]
        public List<AudioClip> StatusReplies = new();

        [Tooltip("Used when dispatch sends backup or the unit confirms backup requests.")]
        public List<AudioClip> BackupReplies = new();

        [Tooltip("Played when a panic check occurs.")]
        public List<AudioClip> PanicReplies = new();

        [Tooltip("Used for detained/in custody confirmations.")]
        public List<AudioClip> CustodyReplies = new();

        [Tooltip("Used when a callout is ended or cleared.")]
        public List<AudioClip> TerminationReplies = new();

        [Header("Mix")]
        [Range(0f, 1f)] public float VoiceVolume = 0.9f;

        /// <summary>
        /// Returns a random clip for the requested response type, or null when nothing is configured.
        /// </summary>
        public AudioClip GetClip(VoiceResponseType responseType)
        {
            var pool = responseType switch
            {
                VoiceResponseType.Assignment => AssignmentReplies,
                VoiceResponseType.Status => StatusReplies,
                VoiceResponseType.Backup => BackupReplies,
                VoiceResponseType.Panic => PanicReplies,
                VoiceResponseType.Custody => CustodyReplies,
                VoiceResponseType.Termination => TerminationReplies,
                _ => Acknowledgements
            };

            if (pool == null || pool.Count == 0)
            {
                return null;
            }

            return pool[Random.Range(0, pool.Count)];
        }
    }
}
