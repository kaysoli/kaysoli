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
        public void Lookup_TypeFilter_PrefersMatchingCategory()
        {
            var result = recordsManager.Lookup("Officer", RecordType.Officer);
            Assert.IsTrue(result.Found);
            Assert.AreEqual(RecordType.Officer, result.Entry.Type);
        }

        [Test]
        public void Interpreter_DetectsLookupIntent()
        {
            var interpreter = new CommandInterpreter(unitManager, callManager, null, ScriptableObject.CreateInstance<VoiceLanguageProfile>());
            var command = interpreter.Parse("run plate 8ABC123 for Unit 21");
            Assert.AreEqual(CommandType.LookupRecord, command.Type);
            Assert.AreEqual("8abc123", command.LookupQuery.ToLowerInvariant());
        }

        [Test]
        public void ImportCsv_ParsesVerboseHeader()
        {
            var header = "RECORD_ID\tTIMESTAMP\tDATA_VERSION\tRECORD_STATUS\tLAST_UPDATED\tLAST_NAME\tFIRST_NAME\tMIDDLE_INITIAL\tNAME_SUFFIX\tSEX\tRACE\tHEIGHT\tWEIGHT\tDATE_OF_BIRTH\tPLACE_OF_BIRTH\tEYE_COLOR\tHAIR_COLOR\tSKIN_TONE\tDISTINGUISHING_MARKS\tDL_NUMBER\tDL_STATE\tDL_CLASS\tDL_EXPIRATION\tDL_STATUS\tSTATE_ID_NUMBER\tSTATE_ID_STATE\tSTATE_ID_EXPIRATION\tPASSPORT_NUMBER\tPASSPORT_COUNTRY\tPASSPORT_EXPIRATION\tSSN\tMILITARY_ID\tGOVERNMENT_EMP_ID\tADDRESS\tADDRESS_VERIFIED_DATE\tOCCUPATION\tEMPLOYER\tEDUCATION_LEVEL\tFBI_NUMBER\tSTATE_ID_SID\tLOCAL_CID\tALIASES\tPOLICE_CAUTIONS\tACTIVE_WARRANTS\tCRIMINAL_HISTORY\tPROTECTIVE_ORDERS\tREGISTERED_VEHICLES\tREGISTERED_FIREARMS\tKNOWN_ASSOCIATES\tLOCAL_POLICE_CONTACTS\tBOLO_STATUS\tIS_ARMED\tHAS_SECURITY_CLEARANCE\tIS_WITNESS_PROTECTION\tTYPE";
            var row = "REC-1\t2025-12-07T19:12:55Z\t1\tActive\t2025-12-08\tNguyen\tSarah\tA\tJr\tF\tAsian\t5'6\"\t130\t1990-01-01\tSan Francisco\tBrown\tBlack\tTan\tScar on left hand\tD1234567\tCA\tC\t2028-01-01\tValid\tCA-12345\tCA\t2026-01-01\tP1234567\tUS\t2030-01-01\t123-45-6789\tMIL-55\tGOV-77\t123 Main St SF\t2025-01-01\tVictim\tTarget\tCollege\tFBI-999\tSID-5\tCID-9\t\"S.Nguyen\"\tNone\tNone\tClean\tNone\t1ABC234;2BCD345\tRegistered Glock\tAssoc1|Assoc2\tSFPD Sgt Doe\tBOLO cleared\tNo\tNo\tNo\tCivilian Worker";
            var csv = $"{header}\n{row}";

            var verboseDb = ScriptableObject.CreateInstance<RecordsDatabase>();
            verboseDb.ImportCsv(new TextAsset(csv));

            recordsManager.SetDatabase(verboseDb);
            recordsManager.LoadDatabases();

            var result = recordsManager.Lookup("REC-1");
            Assert.IsTrue(result.Found);
            Assert.AreEqual("Sarah A Nguyen Jr", result.Entry.Name);
            Assert.AreEqual(RecordType.CivilianWorker, result.Entry.Type);
            StringAssert.Contains("BOLO", result.Entry.BuildSummary());
        }
    }
}
