using System.Collections.Generic;
using UnityEngine;

namespace RadioDispatch.Calls
{
    public enum CallPriority { Low, Medium, High, Critical }
    public enum CallCategory { General, Robbery, Traffic, Disturbance, Medical, Pursuit }

    /// <summary>
    /// Runtime representation of a call/incident currently active in the city.
    /// </summary>
    [System.Serializable]
    public class CallData
    {
        public string Id;
        public string Title;
        public string Description;
        public string Location;
        public CallPriority Priority;
        public List<Units.Unit> AssignedUnits = new();
        public float TimeSinceCreated;
        public float AllowedResponseTime = 120f;
        public bool IsResolved;
        public bool WasFailed;
        public CallCategory Category = CallCategory.General;
        public int MinimumRecommendedUnits = 1;
        public List<Units.UnitType> RecommendedUnitTypes = new();
        /// <summary>
        /// Optional list of voice tokens that were used to spawn this call (for context or repeat commands).
        /// </summary>
        public List<string> VoiceTriggerTokens = new();

        public CallData Clone()
        {
            return new CallData
            {
                Id = Id,
                Title = Title,
                Description = Description,
                Location = Location,
                Priority = Priority,
                AssignedUnits = new List<Units.Unit>(),
                TimeSinceCreated = 0f,
                AllowedResponseTime = AllowedResponseTime,
                IsResolved = false,
                WasFailed = false,
                Category = Category,
                MinimumRecommendedUnits = MinimumRecommendedUnits,
                RecommendedUnitTypes = new List<Units.UnitType>(RecommendedUnitTypes),
                VoiceTriggerTokens = new List<string>(VoiceTriggerTokens)
            };
        }
    }
}
