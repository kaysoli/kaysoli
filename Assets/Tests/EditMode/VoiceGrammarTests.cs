using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using RadioDispatch.Calls;
using RadioDispatch.Radio;
using RadioDispatch.Units;
using RadioDispatch.Voice;

namespace RadioDispatch.Tests
{
    /// <summary>
    /// Covers voice grammar additions such as callout creation, backup modes, and status updates.
    /// </summary>
    public class VoiceGrammarTests
    {
        private UnitManager unitManager;
        private CallManager callManager;
        private RadioSystem radioSystem;

        [SetUp]
        public void SetUp()
        {
            unitManager = new GameObject("UnitManager").AddComponent<UnitManager>();
            callManager = new GameObject("CallManager").AddComponent<CallManager>();
            radioSystem = new GameObject("RadioSystem").AddComponent<RadioSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(unitManager.gameObject);
            Object.DestroyImmediate(callManager.gameObject);
            Object.DestroyImmediate(radioSystem.gameObject);
        }

        [Test]
        public void Interpreter_MapsCalloutTriggers()
        {
            var template = ScriptableObject.CreateInstance<CallTemplate>();
            template.title = "Bank Robbery";
            template.location = "Main Street";
            template.voiceTriggers = new List<string> { "bank robbery" };
            callManager.SetTemplates(new List<CallTemplate> { template });

            var interpreter = new CommandInterpreter(unitManager, callManager);
            var command = interpreter.Parse("start call bank robbery at main street");

            Assert.AreEqual(CommandType.CreateCallout, command.Type);
            Assert.AreEqual(template, command.TargetTemplate);
        }

        [Test]
        public void Executor_SpawnsCalloutAndAssignsUnit()
        {
            var template = ScriptableObject.CreateInstance<CallTemplate>();
            template.id = "500";
            template.title = "Shots Fired";
            template.location = "5th and Main";
            template.voiceTriggers = new List<string> { "shots fired" };
            callManager.SetTemplates(new List<CallTemplate> { template });

            var unit = new Unit { Id = "12", DisplayName = "Unit 12" };
            unitManager.SetUnits(new List<Unit> { unit });

            var executor = new CommandExecutor(unitManager, callManager, radioSystem);
            var command = new ParsedCommand
            {
                Type = CommandType.CreateCallout,
                TargetTemplate = template,
                TargetUnits = new List<Unit> { unit }
            };

            executor.Execute(command);

            Assert.AreEqual(1, callManager.GetActiveCalls().Count);
            Assert.IsNotNull(unit.CurrentCall, "Unit should be attached to the new call.");
            Assert.AreEqual(UnitStatus.EnRoute, unit.Status);
        }

        [Test]
        public void Interpreter_DetectsBackupCode3()
        {
            var interpreter = new CommandInterpreter(unitManager, callManager);
            var command = interpreter.Parse("request backup code 3 to call 1");

            Assert.AreEqual(CommandType.SendBackup, command.Type);
            Assert.AreEqual(BackupRequestType.Code3, command.BackupType);
        }
    }
}
