using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using RadioDispatch.Simulation;
using RadioDispatch.Units;
using RadioDispatch.Voice;

namespace RadioDispatch.Tests.EditMode
{
    /// <summary>
    /// Validates the new simulation helpers so they stay wired to the dispatcher stack.
    /// </summary>
    public class SimulationMechanismsTests
    {
        [Test]
        public void UnitGenesisPool_UsesCsvAndProceduralToHitMinimum()
        {
            var unitManagerGo = new GameObject("UnitManager");
            var unitManager = unitManagerGo.AddComponent<UnitManager>();

            var genesisGo = new GameObject("Genesis");
            var genesis = genesisGo.AddComponent<UnitGenesisPool>();

            // Inject dependencies via reflection because the serialized fields are private.
            SetPrivateField(genesis, "unitManager", unitManager);
            SetPrivateField(genesis, "minimumUnitCount", 5);

            var csv = new TextAsset("U1,Unit One,Patrol,Available,Zone X\nU2,Unit Two,EMS,Available,Zone Y");
            SetPrivateField(genesis, "csvSheets", new List<TextAsset> { csv });

            var roster = genesis.GenerateAndApply();

            Assert.That(roster.Count, Is.GreaterThanOrEqualTo(5));
            Assert.That(unitManager.GetAllUnits().Count, Is.EqualTo(roster.Count));
        }

        [Test]
        public void VoiceHotkeyRouter_ForwardsRecognizedText()
        {
            var voiceGo = new GameObject("VoiceInput");
            var input = voiceGo.AddComponent<VoiceInputManager>();

            var routerGo = new GameObject("Router");
            var router = routerGo.AddComponent<VoiceHotkeyRouter>();
            SetPrivateField(router, "voiceInputManager", input);

            string received = null;
            input.OnVoiceCommandRecognized.AddListener(text => received = text);

            // Use a stub provider to emit text without relying on keyboard input.
            var stubProvider = new StubSpeechProvider();
            router.SetProvider(stubProvider);

            stubProvider.Emit("dispatch test");

            Assert.That(received, Is.EqualTo("dispatch test"));
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }

        private class StubSpeechProvider : ISpeechToTextProvider
        {
            public event System.Action<string> OnRecognized;

            public void BeginCapture()
            {
            }

            public void EndCapture()
            {
            }

            public void Emit(string phrase)
            {
                OnRecognized?.Invoke(phrase);
            }
        }
    }
}
