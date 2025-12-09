using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using RadioDispatch.Calls;
using RadioDispatch.Radio;
using RadioDispatch.Voice;

namespace RadioDispatch.Tutorial
{
    /// <summary>
    /// Coordinates a lightweight guided flow to teach PTT usage, codes, and dispatch basics.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField]
        private RadioSystem radioSystem;

        [SerializeField]
        private CallManager callManager;

        [SerializeField]
        private VoiceCommandController voiceCommandController;

        [SerializeField]
        private List<TutorialStep> steps = new();

        public UnityEvent<TutorialStep> OnStepStarted = new();
        public UnityEvent<TutorialStep> OnStepCompleted = new();

        private int currentStepIndex;

        private void Awake()
        {
            if (steps.Count == 0)
            {
                SeedDefaultSteps();
            }
        }

        /// <summary>
        /// Allows tests or bootstrap code to wire dependencies before enabling the component.
        /// </summary>
        public void Initialize(RadioSystem radio, CallManager calls, VoiceCommandController controller)
        {
            radioSystem = radio;
            callManager = calls;
            voiceCommandController = controller;

            if (isActiveAndEnabled)
            {
                Unsubscribe();
                Subscribe();
                StartStep();
            }
        }

        private void OnEnable()
        {
            Subscribe();
            StartStep();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        /// <summary>
        /// Replaces tutorial steps at runtime to make the flow fully designer-configurable.
        /// </summary>
        public void SetSteps(IEnumerable<TutorialStep> newSteps)
        {
            steps = newSteps == null ? new List<TutorialStep>() : new List<TutorialStep>(newSteps);
            currentStepIndex = 0;
            StartStep();
        }

        public void ResetTutorial()
        {
            currentStepIndex = 0;
            StartStep();
        }

        private void Subscribe()
        {
            if (voiceCommandController != null)
            {
                voiceCommandController.OnCommandHandled += HandleCommand;
            }
            else
            {
                Debug.LogWarning("TutorialManager missing VoiceCommandController reference.");
            }

            CallManager.OnCallResolved += HandleCallResolved;
        }

        private void Unsubscribe()
        {
            if (voiceCommandController != null)
            {
                voiceCommandController.OnCommandHandled -= HandleCommand;
            }

            CallManager.OnCallResolved -= HandleCallResolved;
        }

        private void StartStep()
        {
            if (steps == null || steps.Count == 0 || currentStepIndex >= steps.Count)
            {
                return;
            }

            var step = steps[currentStepIndex];
            radioSystem?.LogMessage("Tutorial", step.Instruction);
            OnStepStarted?.Invoke(step);
        }

        private void CompleteStep()
        {
            if (steps == null || steps.Count == 0 || currentStepIndex >= steps.Count)
            {
                return;
            }

            var step = steps[currentStepIndex];
            OnStepCompleted?.Invoke(step);
            currentStepIndex++;

            if (currentStepIndex >= steps.Count)
            {
                radioSystem?.LogMessage("Tutorial", "Training complete. Resume regular dispatch duties.");
                return;
            }

            StartStep();
        }

        private void HandleCommand(ParsedCommand command, bool success)
        {
            if (!success || command == null)
            {
                return;
            }

            if (steps == null || currentStepIndex >= steps.Count)
            {
                return;
            }

            var step = steps[currentStepIndex];
            if (step.IsComplete(command))
            {
                CompleteStep();
            }
        }

        private void HandleCallResolved(CallData call)
        {
            if (steps == null || currentStepIndex >= steps.Count)
            {
                return;
            }

            if (call == null)
            {
                return;
            }

            var step = steps[currentStepIndex];
            if (step.RequireCallResolution && !call.IsResolved)
            {
                return;
            }

            if (step.RequireCallResolution)
            {
                CompleteStep();
            }
        }

        private void SeedDefaultSteps()
        {
            steps = new List<TutorialStep>
            {
                new()
                {
                    Id = "assign-first-call",
                    Instruction = "Hold PTT and send a unit to the first call (e.g., 'Send Unit 21 to call 1023').",
                    RequiredKeywords = new List<string> { "unit", "call" }
                },
                new()
                {
                    Id = "use-ten-code",
                    Instruction = "Acknowledge with a 10-code like '10-4' to continue.",
                    RequireCodeUsage = true
                },
                new()
                {
                    Id = "resolve",
                    Instruction = "Let your assigned units resolve a call to finish training.",
                    RequireCallResolution = true
                }
            };
        }
    }
}
