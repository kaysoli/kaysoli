using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using RadioDispatch.Calls;
using RadioDispatch.Radio;
using RadioDispatch.Tutorial;
using RadioDispatch.Units;
using RadioDispatch.Voice;

namespace RadioDispatch.Tests.EditMode
{
    public class TutorialManagerTests
    {
        private UnitManager unitManager;
        private CallManager callManager;
        private RadioSystem radioSystem;
        private VoiceInputManager voiceInput;
        private VoiceCommandController controller;
        private TutorialManager tutorialManager;
        private CallData call;

        [SetUp]
        public void SetUp()
        {
            unitManager = new GameObject("UnitManager").AddComponent<UnitManager>();
            callManager = new GameObject("CallManager").AddComponent<CallManager>();
            radioSystem = new GameObject("RadioSystem").AddComponent<RadioSystem>();
            voiceInput = new GameObject("VoiceInput").AddComponent<VoiceInputManager>();
            controller = new GameObject("VoiceController").AddComponent<VoiceCommandController>();
            controller.Initialize(unitManager, callManager, radioSystem, voiceInput);

            tutorialManager = new GameObject("TutorialManager").AddComponent<TutorialManager>();
            tutorialManager.Initialize(radioSystem, callManager, controller);

            var unit = new Unit { Id = "12", DisplayName = "Unit 12" };
            unitManager.SetUnits(new List<Unit> { unit });

            call = new CallData { Id = "1", Title = "Tutorial Call" };
            callManager.SpawnCall(call);

            var steps = new List<TutorialStep>
            {
                new TutorialStep { Id = "assign", Instruction = "Assign", RequiredKeywords = new List<string> { "unit", "call" } },
                new TutorialStep { Id = "resolve", Instruction = "Resolve", RequireCallResolution = true }
            };
            tutorialManager.SetSteps(steps);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(unitManager.gameObject);
            Object.DestroyImmediate(callManager.gameObject);
            Object.DestroyImmediate(radioSystem.gameObject);
            Object.DestroyImmediate(voiceInput.gameObject);
            Object.DestroyImmediate(controller.gameObject);
            Object.DestroyImmediate(tutorialManager.gameObject);
        }

        [Test]
        public void Tutorial_AdvancesOnCommandThenResolution()
        {
            var startedSteps = new List<string>();
            tutorialManager.OnStepStarted.AddListener(step => startedSteps.Add(step.Id));
            tutorialManager.ResetTutorial();

            voiceInput.EndListening("send unit 12 to call 1");
            Assert.That(startedSteps, Does.Contain("assign"));

            callManager.ResolveCall(call);
            Assert.That(startedSteps, Does.Contain("resolve"));
        }
    }
}
