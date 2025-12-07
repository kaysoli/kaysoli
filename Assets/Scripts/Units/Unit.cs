using UnityEngine;
using RadioDispatch.Calls;
using RadioDispatch.Radio;

namespace RadioDispatch.Units
{
    public enum UnitStatus
    {
        Available,
        EnRoute,
        OnScene,
        Busy,
        Pursuit,
        Transporting,
        Detaining,
        InCustody,
        Returning,
        Panic,
        OutOfService
    }

    public enum UnitType
    {
        Unknown,
        Patrol,
        K9,
        SWAT,
        EMS,
        Traffic,
        AirSupport
    }

    /// <summary>
    /// Represents a dispatchable unit within the simulation.
    /// </summary>
    [System.Serializable]
    public class Unit
    {
        public string Id;
        /// <summary>
        /// Optional human-readable call sign (e.g., "2-LINCOLN-20") used in transcripts.
        /// </summary>
        public string CallSign;
        public string DisplayName;
        public UnitStatus Status = UnitStatus.Available;
        public string CurrentZone;
        public CallData CurrentCall;
        public UnitType Type = UnitType.Unknown;
        /// <summary>
        /// Optional acknowledgement marker (e.g., "10-4") applied from terminal selection for operator reference only.
        /// </summary>
        public string AcknowledgementLabel;
        /// <summary>
        /// Voice profile that determines which set of officer audio clips should play when this unit responds.
        /// </summary>
        public UnitVoiceProfile VoiceProfile;
    }
}
