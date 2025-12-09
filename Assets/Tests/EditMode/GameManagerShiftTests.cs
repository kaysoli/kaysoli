using NUnit.Framework;
using RadioDispatch.Calls;
using RadioDispatch.Core;
using RadioDispatch.Radio;
using UnityEngine;

namespace RadioDispatch.Tests.EditMode
{
    public class GameManagerShiftTests
    {
        private GameManager gameManager;
        private CallManager callManager;
        private ScoringSystem scoring;
        private RadioSystem radio;

        [SetUp]
        public void SetUp()
        {
            var gmObject = new GameObject("GameManager");
            gameManager = gmObject.AddComponent<GameManager>();

            callManager = new GameObject("CallManager").AddComponent<CallManager>();
            scoring = new GameObject("ScoringSystem").AddComponent<ScoringSystem>();
            radio = new GameObject("RadioSystem").AddComponent<RadioSystem>();

            var shiftConfig = ScriptableObject.CreateInstance<ShiftConfig>();
            shiftConfig.ShiftLengthSeconds = 10f;
            shiftConfig.SpawnIntervalStart = 30f;
            shiftConfig.SpawnIntervalMin = 5f;
            shiftConfig.SpawnIntervalStep = 5f;
            shiftConfig.SpawnRampSeconds = 3f;
            shiftConfig.MaxUnresolvedCalls = 2;

            SetPrivateField(gameManager, "callManager", callManager);
            SetPrivateField(gameManager, "scoringSystem", scoring);
            SetPrivateField(gameManager, "radioSystem", radio);
            SetPrivateField(gameManager, "shiftConfig", shiftConfig);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameManager.gameObject);
            Object.DestroyImmediate(callManager.gameObject);
            Object.DestroyImmediate(scoring.gameObject);
            Object.DestroyImmediate(radio.gameObject);
        }

        [Test]
        public void Tick_CompletesShift_WhenTimerExpires()
        {
            gameManager.BeginShift();
            gameManager.Tick(11f);

            Assert.AreEqual(GameManager.GameState.Debrief, gameManager.GetState());
        }

        [Test]
        public void Tick_FailsShift_WhenUnresolvedLimitExceeded()
        {
            var call = new CallData { Id = "100", Title = "Test" };
            callManager.SpawnCall(call);

            var shiftConfig = (ShiftConfig)GetPrivateField(gameManager, "shiftConfig");
            shiftConfig.MaxUnresolvedCalls = 0;

            gameManager.BeginShift();
            gameManager.Tick(0.1f);

            Assert.AreEqual(GameManager.GameState.Pause, gameManager.GetState());
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(target, value);
        }

        private static object GetPrivateField(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field.GetValue(target);
        }
    }
}
