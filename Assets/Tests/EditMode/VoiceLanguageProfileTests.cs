using NUnit.Framework;
using UnityEngine;
using RadioDispatch.Calls;
using RadioDispatch.Units;
using RadioDispatch.Voice;

namespace RadioDispatch.Tests
{
    public class VoiceLanguageProfileTests
    {
        private UnitManager unitManager;
        private CallManager callManager;
        private VoiceLanguageProfile spanishProfile;
        private CommandInterpreter interpreter;

        [SetUp]
        public void SetUp()
        {
            unitManager = new GameObject("Units").AddComponent<UnitManager>();
            callManager = new GameObject("Calls").AddComponent<CallManager>();

            spanishProfile = ScriptableObject.CreateInstance<VoiceLanguageProfile>();
            spanishProfile.SetUnitTokens(new[] { "unidad", "unit" });
            spanishProfile.SetIntentKeywords(new[] { "enviar", "asignar" }, new[] { "refuerzo" }, new[] { "estado" }, new[] { "cancelar" });

            var unit = new Unit { Id = "21", DisplayName = "Unidad 21" };
            unitManager.SetUnits(new System.Collections.Generic.List<Unit> { unit });

            var call = new CallData { Id = "500", Title = "Prueba" };
            callManager.SpawnCall(call);

            interpreter = new CommandInterpreter(unitManager, callManager, null, spanishProfile);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(unitManager.gameObject);
            Object.DestroyImmediate(callManager.gameObject);
            Object.DestroyImmediate(spanishProfile);
        }

        [Test]
        public void Interpreter_UsesLocalizedUnitToken()
        {
            // Spanish-style phrase uses "unidad" instead of "unit".
            var command = interpreter.Parse("enviar unidad 21 a la llamada 500");
            Assert.AreEqual(CommandType.AssignUnitsToCall, command.Type);
            Assert.AreEqual(1, command.TargetUnits.Count);
            Assert.IsNotNull(command.TargetCall);
        }
    }
}
