using NUnit.Framework;
using UnityEngine;
using RadioDispatch.Calls;
using RadioDispatch.Codes;
using RadioDispatch.Units;
using RadioDispatch.Voice;

namespace RadioDispatch.Tests.EditMode
{
    public class CommandInterpreterTests
    {
        private UnitManager unitManager;
        private CallManager callManager;
        private CommandInterpreter interpreter;
        private CodeLibrary codeLibrary;
        private CodeSet codeSet;

        [SetUp]
        public void SetUp()
        {
            var unitObject = new GameObject("UnitManager");
            unitManager = unitObject.AddComponent<UnitManager>();

            var callObject = new GameObject("CallManager");
            callManager = callObject.AddComponent<CallManager>();

            codeLibrary = ScriptableObject.CreateInstance<CodeLibrary>();
            codeSet = ScriptableObject.CreateInstance<CodeSet>();
            codeSet.Phrases.Add(new CodePhrase { Code = "10-33", Meaning = "backup", Variants = { "need backup" } });
            codeLibrary.Register(codeSet);

            interpreter = new CommandInterpreter(unitManager, callManager, codeLibrary);

            var unit = new Unit { Id = "21", DisplayName = "Unit 21" };
            unitManager.SetUnits(new System.Collections.Generic.List<Unit> { unit });

            var call = new CallData { Id = "1023", Title = "Test Call" };
            callManager.SpawnCall(call);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(unitManager.gameObject);
            Object.DestroyImmediate(callManager.gameObject);
            Object.DestroyImmediate(codeLibrary);
            Object.DestroyImmediate(codeSet);
        }

        [Test]
        public void Parse_AssignCommand_FindsUnitAndCall()
        {
            var command = interpreter.Parse("Dispatch send Unit 21 to call 1023");
            Assert.AreEqual(CommandType.AssignUnitsToCall, command.Type);
            Assert.AreEqual(1, command.TargetUnits.Count);
            Assert.IsNotNull(command.TargetCall);
            Assert.AreEqual("21", command.TargetUnits[0].Id);
            Assert.AreEqual("1023", command.TargetCall.Id);
            Assert.That(command.RecognizedKeywords, Does.Contain("Unit 21"));
        }

        [Test]
        public void Parse_RecognizesCodesAndIntent()
        {
            var command = interpreter.Parse("10-33 at call 1023");
            Assert.AreEqual(CommandType.SendBackup, command.Type);
            Assert.IsNotNull(command.TargetCall);
            Assert.That(command.RecognizedCodes, Has.Count.EqualTo(1));
            Assert.That(command.RecognizedKeywords, Does.Contain("10-33"));
        }
    }
}
