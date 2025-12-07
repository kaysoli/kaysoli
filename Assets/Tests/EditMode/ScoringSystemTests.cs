using NUnit.Framework;
using RadioDispatch.Calls;
using RadioDispatch.Core;
using RadioDispatch.Units;
using System.Reflection;
using UnityEngine;

namespace RadioDispatch.Tests.EditMode
{
    public class ScoringSystemTests
    {
        private ScoringSystem scoring;

        [TearDown]
        public void TearDown()
        {
            if (scoring != null)
            {
                Object.DestroyImmediate(scoring.gameObject);
            }
        }

        [Test]
        public void Handles_Failed_Call_Penalty()
        {
            scoring = new GameObject("Scoring").AddComponent<ScoringSystem>();
            var call = new CallData
            {
                Id = "200",
                AssignedUnits = new System.Collections.Generic.List<Unit>(),
                WasFailed = true
            };

            var handler = typeof(ScoringSystem).GetMethod("HandleCallResolved", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            handler.Invoke(scoring, new object[] { call });

            Assert.AreEqual(0, scoring.GetScore());
            Assert.AreEqual(1, scoring.GetFailedCalls());
        }
    }
}
