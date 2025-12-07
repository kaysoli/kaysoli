using System.Collections.Generic;
using UnityEngine;

namespace RadioDispatch.Calls
{
    /// <summary>
    /// ScriptableObject template for defining incident presets that can be spawned at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "CallTemplate", menuName = "RadioDispatch/Call Template")]
    public class CallTemplate : ScriptableObject
    {
        public string id = "0000";
        public string title = "Incident";
        [TextArea]
        public string description = "Caller reported an incident.";
        public string location = "Unknown";
        public CallPriority priority = CallPriority.Medium;
        public float allowedResponseTime = 120f;
        public CallCategory category = CallCategory.General;
        [Min(1)]
        public int minimumRecommendedUnits = 1;
        public List<Units.UnitType> recommendedUnitTypes = new();
        [Tooltip("Optional voice tokens that allow the player to spawn this callout verbally.")]
        public List<string> voiceTriggers = new();

        public CallData CreateInstance()
        {
            return new CallData
            {
                Id = id,
                Title = title,
                Description = description,
                Location = location,
                Priority = priority,
                Category = category,
                AllowedResponseTime = allowedResponseTime,
                MinimumRecommendedUnits = minimumRecommendedUnits,
                RecommendedUnitTypes = new List<Units.UnitType>(recommendedUnitTypes),
                VoiceTriggerTokens = new List<string>(voiceTriggers),
                TimeSinceCreated = 0f,
                IsResolved = false
            };
        }
    }
}
