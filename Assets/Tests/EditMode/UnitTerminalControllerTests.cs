using NUnit.Framework;
using UnityEngine;
using RadioDispatch.Calls;
using RadioDispatch.Radio;
using RadioDispatch.UI;
using RadioDispatch.Units;

namespace RadioDispatch.Tests.EditMode
{
    public class UnitTerminalControllerTests
    {
        private UnitManager unitManager;
        private CallManager callManager;
        private RadioSystem radioSystem;
        private UnitTerminalController controller;

        [SetUp]
        public void SetUp()
        {
            unitManager = new GameObject("Units").AddComponent<UnitManager>();
            callManager = new GameObject("Calls").AddComponent<CallManager>();
            radioSystem = new GameObject("Radio").AddComponent<RadioSystem>();

            var unit = new Unit { Id = "15", DisplayName = "Unit 15" };
            unitManager.SetUnits(new System.Collections.Generic.List<Unit> { unit });

            var call = new CallData { Id = "77", Title = "Test" };
            callManager.SpawnCall(call);

            controller = new GameObject("Controller").AddComponent<UnitTerminalController>();
            controller.Initialize(unitManager, callManager, radioSystem);
            controller.SelectUnit("15");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(unitManager.gameObject);
            Object.DestroyImmediate(callManager.gameObject);
            Object.DestroyImmediate(radioSystem.gameObject);
            Object.DestroyImmediate(controller.gameObject);
        }

        [Test]
        public void AcknowledgeSelected_SetsLabel()
        {
            controller.AcknowledgeSelected();
            var unit = unitManager.GetUnitById("15");
            Assert.AreEqual("10-4", unit.AcknowledgementLabel);
        }

        [Test]
        public void AssignSelectedToCall_UpdatesCall()
        {
            controller.AssignSelectedToCall("77");
            var unit = unitManager.GetUnitById("15");
            Assert.AreEqual("77", unit.CurrentCall.Id);
            Assert.AreEqual(UnitStatus.EnRoute, unit.Status);
        }
    }
}
