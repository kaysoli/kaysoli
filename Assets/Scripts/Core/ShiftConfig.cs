using UnityEngine;

namespace RadioDispatch.Core
{
    /// <summary>
    /// Configures shift length and difficulty scaling for Phase 5 progression.
    /// </summary>
    [CreateAssetMenu(fileName = "ShiftConfig", menuName = "RadioDispatch/Shift Config")]
    public class ShiftConfig : ScriptableObject
    {
        [Header("Timing")]
        public float ShiftLengthSeconds = 600f;
        public float SpawnRampSeconds = 120f;

        [Header("Spawning")]
        public float SpawnIntervalStart = 45f;
        public float SpawnIntervalMin = 15f;
        public float SpawnIntervalStep = 5f;
        public int MaxUnresolvedCalls = 5;
    }
}
