using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RadioDispatch.Records;
using RadioDispatch.Units;

namespace RadioDispatch.Radio
{
    /// <summary>
    /// Generates ambient unit chatter (subject/plate requests, banter) and routes it through the radio and records systems.
    /// </summary>
    public class ChatterManager : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField]
        private ChatterLibrary defaultLibrary;

        [Tooltip("If true, random chatter from the library will fire automatically between the configured delays.")]
        [SerializeField]
        private bool autoPlay = true;

        [SerializeField]
        private Vector2 autoPlayDelaySeconds = new(45f, 90f);

        [Header("Dependencies")]
        [SerializeField]
        private UnitManager unitManager;

        [SerializeField]
        private RecordsManager recordsManager;

        [SerializeField]
        private RadioSystem radioSystem;

        private List<ChatterEntry> runtimeEntries = new();
        private Coroutine autoRoutine;

        private void Awake()
        {
            runtimeEntries = defaultLibrary != null ? defaultLibrary.CloneEntries() : new List<ChatterEntry>();
        }

        /// <summary>
        /// Replaces the chatter library at runtime (used when the GameManager seeds fallback content or tests override the set).
        /// </summary>
        public void SetLibrary(ChatterLibrary library)
        {
            defaultLibrary = library;
            runtimeEntries = defaultLibrary != null ? defaultLibrary.CloneEntries() : new List<ChatterEntry>();
        }

        /// <summary>
        /// Provides dependency injection for UI or tests when not using the Inspector.
        /// </summary>
        public void Initialize(UnitManager units, RecordsManager records, RadioSystem radio)
        {
            unitManager = units;
            recordsManager = records;
            radioSystem = radio;
        }

        private void OnEnable()
        {
            if (autoPlay)
            {
                autoRoutine = StartCoroutine(AutoChatterLoop());
            }
        }

        private void OnDisable()
        {
            if (autoRoutine != null)
            {
                StopCoroutine(autoRoutine);
            }
        }

        /// <summary>
        /// Immediately fires a chatter entry by its ID if present in the loaded library.
        /// </summary>
        public void TriggerChatter(string entryId)
        {
            var entry = runtimeEntries.Find(e => !string.IsNullOrWhiteSpace(e.Id) && e.Id == entryId);
            if (entry != null)
            {
                PlayChatter(entry);
            }
        }

        /// <summary>
        /// Queues a specific chatter entry instance (useful for scripted events or tests).
        /// </summary>
        public void PlayChatter(ChatterEntry entry)
        {
            if (entry == null || radioSystem == null)
            {
                return;
            }

            var unit = !string.IsNullOrWhiteSpace(entry.UnitId) ? unitManager?.GetUnitById(entry.UnitId) : null;
            var speaker = unit != null ? unit.DisplayName ?? unit.Id : string.IsNullOrWhiteSpace(entry.UnitId) ? "Unit" : entry.UnitId;

            radioSystem.BeginTransmission();

            if (entry.UnitPrompt != null)
            {
                radioSystem.PlayCustomClip(entry.UnitPrompt);
            }

            if (!string.IsNullOrWhiteSpace(entry.TransmissionText))
            {
                radioSystem.LogMessage(speaker, entry.TransmissionText);
            }

            // Dispatcher response: either a lookup or a canned line.
            if (!string.IsNullOrWhiteSpace(entry.DispatchResponseOverride))
            {
                radioSystem.LogMessage("Dispatch", entry.DispatchResponseOverride);
                if (entry.DispatchResponseClip != null)
                {
                    radioSystem.PlayCustomClip(entry.DispatchResponseClip);
                }
            }
            else if (!string.IsNullOrWhiteSpace(entry.RecordQuery) && recordsManager != null)
            {
                var filter = entry.UseTypeFilter ? entry.TypeFilter : (RecordType?)null;
                var result = recordsManager.Lookup(entry.RecordQuery, filter);
                radioSystem.LogMessage("Dispatch", result.BuildResponse());
                if (unit != null)
                {
                    radioSystem.PlayUnitVoice(unit.VoiceProfile, VoiceResponseType.Status);
                }
            }

            radioSystem.EndTransmission();
        }

        /// <summary>
        /// Triggers a random chatter line using the current library to keep the radio alive with banter.
        /// </summary>
        public void PlayRandomChatter()
        {
            if (runtimeEntries.Count == 0)
            {
                return;
            }

            var randomIndex = Random.Range(0, runtimeEntries.Count);
            PlayChatter(runtimeEntries[randomIndex]);
        }

        private IEnumerator AutoChatterLoop()
        {
            while (autoPlay && enabled)
            {
                var delay = Random.Range(autoPlayDelaySeconds.x, autoPlayDelaySeconds.y);
                yield return new WaitForSeconds(delay);
                PlayRandomChatter();
            }
        }
    }
}
