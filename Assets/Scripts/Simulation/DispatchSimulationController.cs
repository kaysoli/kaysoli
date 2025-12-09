using UnityEngine;
using System.Collections.Generic;
using RadioDispatch.Core;
using RadioDispatch.Calls;
using RadioDispatch.Codes;
using RadioDispatch.Units;
using RadioDispatch.Voice;
using RadioDispatch.Radio;
using RadioDispatch.Records;

namespace RadioDispatch.Simulation
{
    /// <summary>
    /// High-level bootstrap that wires together generation, voice routing, and dispatcher telemetry so designers can simulate
    /// busy radio traffic with minimal scene setup.
    /// </summary>
    [DisallowMultipleComponent]
    public class DispatchSimulationController : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField]
        [Tooltip("Core game manager controlling shift timers and scoring.")]
        private GameManager gameManager;

        [SerializeField]
        [Tooltip("Unit manager to receive generated rosters and apply status updates.")]
        private UnitManager unitManager;

        [SerializeField]
        [Tooltip("Call manager for spawning incidents from templates or voice triggers.")]
        private CallManager callManager;

        [SerializeField]
        [Tooltip("Radio system used to play roger beeps and officer voice replies.")]
        private RadioSystem radioSystem;

        [SerializeField]
        [Tooltip("Voice command controller that sits between the interpreter and executor; wired for automatic initialization.")]
        private VoiceCommandController voiceCommandController;

        [Header("Simulation Hooks")]
        [SerializeField]
        [Tooltip("Optional generator that will seed hundreds of units from rosters/CSV/procedural sources.")]
        private UnitGenesisPool unitGenesisPool;

        [SerializeField]
        [Tooltip("Optional hotkey router to allow desktop PTT and queue-driven STT when mobile platforms are unavailable.")]
        private VoiceHotkeyRouter voiceHotkeyRouter;

        [SerializeField]
        [Tooltip("Voice input component used for push-to-talk collection from mobile or editor hotkeys.")]
        private VoiceInputManager voiceInputManager;

        [SerializeField]
        [Tooltip("Optional set of code sets to seed into the interpreter for localized grammar.")]
        private List<CodeSet> codeSets = new();

        [SerializeField]
        [Tooltip("Feedback profile controlling success/failure strings for voice parsing.")]
        private RadioFeedbackProfile feedbackProfile;

        [SerializeField]
        [Tooltip("Records manager used for subject/vehicle lookups when officers request database checks.")]
        private RecordsManager recordsManager;

        [SerializeField]
        [Tooltip("When enabled, the controller will call BeginShift on start to spin up timers and call spawning.")]
        private bool autoBeginShift = true;

        private void Awake()
        {
            if (unitGenesisPool != null)
            {
                unitGenesisPool.GenerateAndApply();
            }

            if (voiceCommandController != null)
            {
                voiceCommandController.Initialize(
                    unitManager,
                    callManager,
                    radioSystem,
                    voiceInputManager,
                    codeSets,
                    feedbackProfile,
                    recordsManager);
            }

            if (autoBeginShift && gameManager != null)
            {
                gameManager.BeginShift();
            }
        }

        /// <summary>
        /// Allows external UI or debug consoles to spawn a test call and immediately assign the nearest available unit.
        /// </summary>
        public void SpawnTestCallAndDispatchNearest()
        {
            if (callManager == null || unitManager == null || radioSystem == null)
            {
                Debug.LogWarning("SpawnTestCallAndDispatchNearest requires CallManager, UnitManager, and RadioSystem.");
                return;
            }

            var call = callManager.SpawnCallFromTemplate(callManager.GetTemplateForSpeech("stabbing"));
            var unit = unitManager.GetAvailableUnits().Count > 0 ? unitManager.GetAvailableUnits()[0] : null;
            if (call != null && unit != null)
            {
                unitManager.AssignUnitToCall(unit, call);
                radioSystem.LogMessage("Dispatch", $"{unit.DisplayName} responding to {call.Id}");
            }
        }
    }
}
