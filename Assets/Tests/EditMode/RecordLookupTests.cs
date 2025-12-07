using NUnit.Framework;
using UnityEngine;
using RadioDispatch.Records;
using RadioDispatch.Voice;
using RadioDispatch.Units;
using RadioDispatch.Calls;

namespace RadioDispatch.Tests.EditMode
{
    public class RecordLookupTests
    {
        private RecordsManager recordsManager;
        private RecordsDatabase db;
        private UnitManager unitManager;
        private CallManager callManager;

        [SetUp]
        public void SetUp()
        {
            var recordObject = new GameObject("RecordsManager");
            recordsManager = recordObject.AddComponent<RecordsManager>();
            db = ScriptableObject.CreateInstance<RecordsDatabase>();
            db.Records.Add(new RecordEntry { Id = "A123", Name = "Chris Red", Type = RecordType.Civilian, VehiclePlate = "8ABC123", Notes = "No wants" });
            db.Records.Add(new RecordEntry { Id = "B777", Name = "Officer Quinn", Type = RecordType.Officer, VehiclePlate = "Q21", Notes = "Badge 21" });
            recordsManager.SetDatabase(db);
            recordsManager.LoadDatabases();

            var unitObject = new GameObject("UnitManager");
            unitManager = unitObject.AddComponent<UnitManager>();
            unitManager.SetUnits(new System.Collections.Generic.List<Unit> { new Unit { Id = "21", DisplayName = "Unit 21" } });

            var callObject = new GameObject("CallManager");
            callManager = callObject.AddComponent<CallManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(recordsManager.gameObject);
            Object.DestroyImmediate(db);
            Object.DestroyImmediate(unitManager.gameObject);
            Object.DestroyImmediate(callManager.gameObject);
        }

        [Test]
        public void Lookup_ById_ReturnsMatch()
        {
            var result = recordsManager.Lookup("A123");
            Assert.IsTrue(result.Found);
            Assert.IsNotNull(result.Entry);
            Assert.AreEqual("Chris Red", result.Entry.Name);
            StringAssert.Contains("Plate", result.BuildResponse());
        }

        [Test]
        public void Interpreter_DetectsLookupIntent()
        {
            var interpreter = new CommandInterpreter(unitManager, callManager, null, ScriptableObject.CreateInstance<VoiceLanguageProfile>());
            var command = interpreter.Parse("run plate 8ABC123 for Unit 21");
            Assert.AreEqual(CommandType.LookupRecord, command.Type);
            Assert.AreEqual("8abc123", command.LookupQuery.ToLowerInvariant());
        }
    }
}
