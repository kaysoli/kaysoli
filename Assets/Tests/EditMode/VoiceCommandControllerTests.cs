using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using RadioDispatch.Calls;
using RadioDispatch.Radio;
using RadioDispatch.Units;
using RadioDispatch.Voice;

namespace RadioDispatch.Tests
{
    public class VoiceCommandControllerTests
    {
        private UnitManager unitManager;
        private CallManager callManager;
        private RadioSystem radioSystem;
        private VoiceInputManager voiceInput;
        private VoiceCommandController controller;
        private string lastFeedback;

        [SetUp]
        public void SetUp()
        {
            unitManager = new GameObject("UnitManager").AddComponent<UnitManager>();
            callManager = new GameObject("CallManager").AddComponent<CallManager>();
            radioSystem = new GameObject("RadioSystem").AddComponent<RadioSystem>();
            voiceInput = new GameObject("VoiceInput").AddComponent<VoiceInputManager>();
            controller = new GameObject("VoiceController").AddComponent<VoiceCommandController>();
            controller.Initialize(unitManager, callManager, radioSystem, voiceInput);
            controller.OnFeedbackGenerated.AddListener(feedback => lastFeedback = feedback);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(unitManager.gameObject);
            Object.DestroyImmediate(callManager.gameObject);
            Object.DestroyImmediate(radioSystem.gameObject);
            Object.DestroyImmediate(voiceInput.gameObject);
            Object.DestroyImmediate(controller.gameObject);
        }

        [Test]
        public void VoiceCommand_AssignsUnitToCall()
        {
            var unit = new Unit { Id = "21", DisplayName = "Unit 21" };
            unitManager.SetUnits(new List<Unit> { unit });

            var call = new CallData { Id = "1023", Title = "Test Incident" };
            callManager.SpawnCall(call);

            voiceInput.EndListening("send unit 21 to call 1023");

            Assert.AreEqual(UnitStatus.EnRoute, unit.Status);
            Assert.AreSame(call, unit.CurrentCall);
            CollectionAssert.Contains(call.AssignedUnits, unit);
            Assert.IsTrue(radioSystem.Transcript.Any(entry => entry.Contains("Unit 21")), "Radio transcript should include the dispatched unit.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(lastFeedback), "Feedback should be emitted after a successful command.");
        }
    }
}
