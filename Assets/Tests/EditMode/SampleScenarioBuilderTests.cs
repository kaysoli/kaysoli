using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using RadioDispatch.Calls;
using RadioDispatch.Radio;
using RadioDispatch.Records;
using RadioDispatch.Simulation;
using RadioDispatch.Units;

namespace RadioDispatch.Tests.EditMode
{
    /// <summary>
    /// Covers the sample scenario builder to ensure it loads content into managers and runs the mini demo loop.
    /// </summary>
    public class SampleScenarioBuilderTests
    {
        [Test]
        public void BuildScenario_LoadsRosterTemplatesAndRecords()
        {
            var unitGo = new GameObject("Units");
            var unitManager = unitGo.AddComponent<UnitManager>();

            var callGo = new GameObject("Calls");
            var callManager = callGo.AddComponent<CallManager>();

            var radioGo = new GameObject("Radio");
            var radio = radioGo.AddComponent<RadioSystem>();

            var recordsGo = new GameObject("Records");
            var recordsManager = recordsGo.AddComponent<RecordsManager>();
            recordsManager.SetDatabase(ScriptableObject.CreateInstance<RecordsDatabase>());

            var roster = ScriptableObject.CreateInstance<UnitRoster>();
            roster.Units.Add(new Unit { Id = "U1", DisplayName = "Unit One" });

            var template = ScriptableObject.CreateInstance<CallTemplate>();
            template.id = "C1";
            template.title = "Test";
            template.voiceTriggers.Add("test");
            var callLibrary = ScriptableObject.CreateInstance<CallLibrary>();
            callLibrary.Templates.Add(template);

            var csv = new TextAsset("ID1,Test Civilian,Civilian,,Notes,,Engineer");

            var builderGo = new GameObject("Builder");
            var builder = builderGo.AddComponent<SampleScenarioBuilder>();
            SetField(builder, "unitManager", unitManager);
            SetField(builder, "callManager", callManager);
            SetField(builder, "radioSystem", radio);
            SetField(builder, "recordsManager", recordsManager);
            SetField(builder, "roster", roster);
            SetField(builder, "callLibrary", callLibrary);
            SetField(builder, "csvSheets", new List<TextAsset> { csv });
            SetField(builder, "autoRun", false);

            builder.BuildScenario();

            Assert.That(unitManager.GetAllUnits().Count, Is.EqualTo(1));
            Assert.That(callManager.GetTemplateForSpeech("test"), Is.Not.Null);
            Assert.That(recordsManager.Lookup("ID1").Found, Is.True);
        }

        [UnityTest]
        public IEnumerator RunSampleSequence_DispatchesUnitAndPlaysChatter()
        {
            var unitGo = new GameObject("Units");
            var unitManager = unitGo.AddComponent<UnitManager>();
            unitManager.SetUnits(new List<Unit> { new Unit { Id = "U1", DisplayName = "Unit One" } });

            var callGo = new GameObject("Calls");
            var callManager = callGo.AddComponent<CallManager>();
            var template = ScriptableObject.CreateInstance<CallTemplate>();
            template.id = "C1";
            template.title = "Test";
            var callLibrary = ScriptableObject.CreateInstance<CallLibrary>();
            callLibrary.Templates.Add(template);
            callManager.SetTemplates(callLibrary.GetTemplates());

            var radioGo = new GameObject("Radio");
            var radio = radioGo.AddComponent<RadioSystem>();

            var chatterGo = new GameObject("Chatter");
            var chatter = chatterGo.AddComponent<ChatterManager>();
            SetField(chatter, "unitManager", unitManager);
            SetField(chatter, "radioSystem", radio);
            SetField(chatter, "recordsManager", null);
            SetField(chatter, "autoPlay", false);
            SetField(chatter, "runtimeEntries", new List<ChatterEntry> { new ChatterEntry { TransmissionText = "Test chatter" } });

            var builderGo = new GameObject("Builder");
            var builder = builderGo.AddComponent<SampleScenarioBuilder>();
            SetField(builder, "unitManager", unitManager);
            SetField(builder, "callManager", callManager);
            SetField(builder, "radioSystem", radio);
            SetField(builder, "chatterManager", chatter);
            SetField(builder, "callLibrary", callLibrary);
            SetField(builder, "playSampleChatter", true);
            SetField(builder, "autoRun", false);
            SetField(builder, "chatterDelaySeconds", 0.01f);

            yield return builder.RunSampleSequence();

            Assert.That(unitManager.GetAllUnits()[0].CurrentCall, Is.Not.Null);
        }

        private static void SetField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(target, value);
        }
    }
}
