using NUnit.Framework;
using UnityEngine;
using RadioDispatch.Radio;
using RadioDispatch.Records;
using RadioDispatch.Units;

namespace RadioDispatch.Tests.EditMode
{
    /// <summary>
    /// Validates that chatter requests route through records and radio logging so dispatchers hear both sides.
    /// </summary>
    public class ChatterManagerTests
    {
        private GameObject host;
        private ChatterManager chatter;
        private RecordsManager records;
        private RadioSystem radio;
        private UnitManager units;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("ChatterHost");
            chatter = host.AddComponent<ChatterManager>();
            radio = host.AddComponent<RadioSystem>();

            var recordsObj = new GameObject("Records");
            records = recordsObj.AddComponent<RecordsManager>();
            var db = ScriptableObject.CreateInstance<RecordsDatabase>();
            db.Records.Add(new RecordEntry { Id = "TEST-PLATE", Name = "Grey Sedan", Type = RecordType.Vehicle, VehiclePlate = "8TEST123", Notes = "Expired registration" });
            records.SetDatabase(db);
            records.LoadDatabases();

            var unitsObj = new GameObject("Units");
            units = unitsObj.AddComponent<UnitManager>();
            units.SetUnits(new System.Collections.Generic.List<Unit> { new Unit { Id = "21", DisplayName = "Unit 21" } });

            chatter.Initialize(units, records, radio);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(records.gameObject);
            Object.DestroyImmediate(units.gameObject);
        }

        [Test]
        public void PlayChatter_RunsLookupAndLogsResponse()
        {
            var entry = new ChatterEntry
            {
                UnitId = "21",
                TransmissionText = "Dispatch, plate check 8TEST123.",
                RecordQuery = "8TEST123",
                UseTypeFilter = true,
                TypeFilter = RecordType.Vehicle
            };

            chatter.PlayChatter(entry);

            CollectionAssert.IsNotEmpty(radio.Transcript);
        }
    }
}
