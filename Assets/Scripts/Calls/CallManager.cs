using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RadioDispatch.Calls
{
    /// <summary>
    /// Responsible for creating, updating, and resolving active calls in the simulation.
    /// </summary>
    public class CallManager : MonoBehaviour
    {
        public static event Action<CallData> OnNewCall;
        public static event Action<CallData> OnCallUpdated;
        public static event Action<CallData> OnCallResolved;

        [SerializeField]
        private List<CallTemplate> templates = new();

        [SerializeField]
        private float autoSpawnInterval = 30f;

        [SerializeField]
        private bool autoSpawnEnabled = true;

        [SerializeField]
        private CallLibrary callLibrary;

        [SerializeField]
        [Tooltip("Optional speech library that injects custom voice triggers for templated callouts.")]
        private CalloutSpeechLibrary calloutSpeechLibrary;

        private readonly List<CallData> activeCalls = new();
        private float spawnTimer;

        public float AutoSpawnInterval => autoSpawnInterval;

        public int ActiveUnresolvedCount => activeCalls.Count(call => !call.IsResolved);

        public bool HasTemplates => templates.Count > 0;

        private void Awake()
        {
            if (callLibrary != null && callLibrary.GetTemplates().Count > 0)
            {
                templates = callLibrary.GetTemplates();
            }

            MergeSpeechLibrary();
        }

        private void Update()
        {
            UpdateTimers();
            HandleAutoSpawning();
        }

        private void UpdateTimers()
        {
            foreach (var call in activeCalls)
            {
                if (call.IsResolved)
                {
                    continue;
                }

                call.TimeSinceCreated += Time.deltaTime;
                OnCallUpdated?.Invoke(call);
            }
        }

        private void HandleAutoSpawning()
        {
            if (!autoSpawnEnabled || templates.Count == 0)
            {
                return;
            }

            spawnTimer += Time.deltaTime;
            if (spawnTimer < autoSpawnInterval)
            {
                return;
            }

            spawnTimer = 0f;
            var template = templates[UnityEngine.Random.Range(0, templates.Count)];
            SpawnCall(template.CreateInstance());
        }

        /// <summary>
        /// Enables automatic spawning of incidents.
        /// </summary>
        public void StartSpawning()
        {
            autoSpawnEnabled = true;
            spawnTimer = 0f;
        }

        /// <summary>
        /// Stops automatic spawning, leaving current calls untouched.
        /// </summary>
        public void StopSpawning()
        {
            autoSpawnEnabled = false;
        }

        /// <summary>
        /// Adjusts the spawn interval at runtime for difficulty scaling.
        /// </summary>
        public void SetAutoSpawnInterval(float interval)
        {
            autoSpawnInterval = Mathf.Max(5f, interval);
        }

        /// <summary>
        /// Replaces the current template pool, useful for content expansion or tests.
        /// </summary>
        public void SetTemplates(IEnumerable<CallTemplate> newTemplates)
        {
            templates = newTemplates?.ToList() ?? new List<CallTemplate>();
            MergeSpeechLibrary();
        }

        /// <summary>
        /// Adds a new call to the active list and notifies listeners.
        /// </summary>
        public void SpawnCall(CallData call)
        {
            if (call == null)
            {
                Debug.LogWarning("Attempted to spawn a null call.");
                return;
            }

            activeCalls.Add(call);
            OnNewCall?.Invoke(call);
            Debug.Log($"New call spawned: {call.Id} - {call.Title}");
        }

        /// <summary>
        /// Marks a call as resolved and triggers scoring and UI updates.
        /// </summary>
        public void ResolveCall(CallData call, bool failed = false)
        {
            if (call == null)
            {
                Debug.LogWarning("Attempted to resolve a null call.");
                return;
            }

            call.IsResolved = true;
            call.WasFailed = failed || !IsAssignmentSufficient(call);
            OnCallResolved?.Invoke(call);
            Debug.Log($"Call resolved: {call.Id}");
        }

        /// <summary>
        /// Lightweight rubric to determine if a call was adequately staffed.
        /// </summary>
        public bool IsAssignmentSufficient(CallData call)
        {
            if (call == null)
            {
                return false;
            }

            if (call.AssignedUnits == null || call.AssignedUnits.Count < call.MinimumRecommendedUnits)
            {
                return false;
            }

            if (call.RecommendedUnitTypes.Count == 0)
            {
                return true;
            }

            return call.AssignedUnits.Any(unit => call.RecommendedUnitTypes.Contains(unit.Type));
        }

        /// <summary>
        /// Exposes the currently active call collection.
        /// </summary>
        public IReadOnlyList<CallData> GetActiveCalls() => activeCalls.AsReadOnly();

        /// <summary>
        /// Attempts to find a call by its identifier.
        /// </summary>
        public CallData GetCallById(string callId)
        {
            return activeCalls.Find(c => c.Id.Equals(callId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Attempts to find a call template that matches a spoken keyword so new incidents can be created verbally.
        /// </summary>
        /// <param name="speech">Recognized voice text.</param>
        public CallTemplate GetTemplateForSpeech(string speech)
        {
            if (string.IsNullOrWhiteSpace(speech))
            {
                return null;
            }

            var cleaned = speech.ToLowerInvariant();
            return templates.FirstOrDefault(template =>
                template.voiceTriggers != null && template.voiceTriggers.Any(trigger =>
                    !string.IsNullOrWhiteSpace(trigger) && cleaned.Contains(trigger.ToLowerInvariant())));
        }

        /// <summary>
        /// Pulls any designer-authored voice trigger overrides from the speech library into the template list.
        /// </summary>
        private void MergeSpeechLibrary()
        {
            if (calloutSpeechLibrary == null)
            {
                return;
            }

            var entries = calloutSpeechLibrary.GetEntries();
            foreach (var entry in entries)
            {
                if (entry.Template == null)
                {
                    continue;
                }

                if (!templates.Contains(entry.Template))
                {
                    templates.Add(entry.Template);
                }

                if (entry.VoiceTriggers == null || entry.VoiceTriggers.Count == 0)
                {
                    continue;
                }

                entry.Template.voiceTriggers ??= new List<string>();

                foreach (var trigger in entry.VoiceTriggers)
                {
                    if (string.IsNullOrWhiteSpace(trigger) || entry.Template.voiceTriggers.Contains(trigger))
                    {
                        continue;
                    }

                    entry.Template.voiceTriggers.Add(trigger.ToLowerInvariant());
                }
            }
        }

        /// <summary>
        /// Spawns an incident from a template and returns the new call instance for immediate assignment.
        /// </summary>
        public CallData SpawnCallFromTemplate(CallTemplate template)
        {
            if (template == null)
            {
                Debug.LogWarning("SpawnCallFromTemplate received a null template.");
                return null;
            }

            var call = template.CreateInstance();
            SpawnCall(call);
            return call;
        }
    }
}
