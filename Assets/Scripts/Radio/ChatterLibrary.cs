using System;
using System.Collections.Generic;
using UnityEngine;
using RadioDispatch.Records;

namespace RadioDispatch.Radio
{
    /// <summary>
    /// ScriptableObject that holds authored radio chatter so designers can queue spontaneous unit requests or banter.
    /// </summary>
    [CreateAssetMenu(fileName = "ChatterLibrary", menuName = "RadioDispatch/Chatter Library")]
    public class ChatterLibrary : ScriptableObject
    {
        [Tooltip("All chatter entries the dispatcher can hear during a shift.")]
        public List<ChatterEntry> Entries = new();

        /// <summary>
        /// Returns a shallow copy so runtime mutations never alter the asset.
        /// </summary>
        public List<ChatterEntry> CloneEntries()
        {
            var clones = new List<ChatterEntry>();
            foreach (var entry in Entries)
            {
                clones.Add(entry.Clone());
            }

            return clones;
        }
    }

    /// <summary>
    /// Defines a single chatter transmission including the unit that speaks, what they ask for, and how dispatch should reply.
    /// </summary>
    [Serializable]
    public class ChatterEntry
    {
        [Tooltip("Optional unique ID so other systems can trigger a specific chatter line.")]
        public string Id;

        [Tooltip("Readable label describing when or why this chatter fires.")]
        public string Title;

        [Tooltip("Unit identifier (call sign or ID) to match against the UnitManager roster.")]
        public string UnitId;

        [Tooltip("What the unit says over the air when this chatter begins.")]
        [TextArea]
        public string TransmissionText;

        [Tooltip("Optional record query (ID, plate, name) this chatter should trigger.")]
        public string RecordQuery;

        [Tooltip("Optional filter to limit results to a specific record type. Leave unchecked to search any type.")]
        public bool UseTypeFilter;

        [Tooltip("Record type filter applied when UseTypeFilter is true.")]
        public RecordType TypeFilter = RecordType.Civilian;

        [Tooltip("If set, dispatch will read this response instead of the lookup result (useful for pure banter).")]
        [TextArea]
        public string DispatchResponseOverride;

        [Tooltip("Optional prompt clip to play for the requesting unit before the dispatcher answers.")]
        public AudioClip UnitPrompt;

        [Tooltip("Optional clip played after dispatch responds (lets you drag custom voice replies without code changes).")]
        public AudioClip DispatchResponseClip;

        /// <summary>
        /// Creates a detached copy so runtime queues cannot mutate the asset instance.
        /// </summary>
        public ChatterEntry Clone()
        {
            return new ChatterEntry
            {
                Id = Id,
                Title = Title,
                UnitId = UnitId,
                TransmissionText = TransmissionText,
                RecordQuery = RecordQuery,
                UseTypeFilter = UseTypeFilter,
                TypeFilter = TypeFilter,
                DispatchResponseOverride = DispatchResponseOverride,
                UnitPrompt = UnitPrompt,
                DispatchResponseClip = DispatchResponseClip
            };
        }
    }
}
