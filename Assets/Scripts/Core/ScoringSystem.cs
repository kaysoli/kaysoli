using System;
using System.Linq;
using UnityEngine;
using RadioDispatch.Calls;

namespace RadioDispatch.Core
{
    /// <summary>
    /// Listens to call lifecycle events and computes points based on timeliness and correctness of decisions.
    /// </summary>
    public class ScoringSystem : MonoBehaviour
    {
        [SerializeField]
        private int totalScore;

        [SerializeField]
        private int baseResolvePoints = 100;

        [SerializeField]
        private int latePenalty = 25;

        [SerializeField]
        private int failurePenalty = 75;

        [SerializeField]
        private int correctUnitBonus = 25;

        private int failedCalls;

        private void OnEnable()
        {
            CallManager.OnCallResolved += HandleCallResolved;
        }

        private void OnDisable()
        {
            CallManager.OnCallResolved -= HandleCallResolved;
        }

        private void HandleCallResolved(CallData call)
        {
            if (call == null)
            {
                Debug.LogWarning("ScoringSystem received null call reference.");
                return;
            }

            var points = baseResolvePoints;

            if (call.TimeSinceCreated > call.AllowedResponseTime)
            {
                points = Math.Max(0, points - latePenalty);
            }

            if (!call.WasFailed)
            {
                if (call.RecommendedUnitTypes.Count > 0 && call.AssignedUnits.Any(unit => call.RecommendedUnitTypes.Contains(unit.Type)))
                {
                    points += correctUnitBonus;
                }

                totalScore += Math.Max(0, points);
            }
            else
            {
                failedCalls++;
                totalScore = Math.Max(0, totalScore - failurePenalty);
            }

            Debug.Log($"Scoring updated for call {call.Id}. Change: {points}, Failures: {failedCalls}, Total: {totalScore}.");
        }

        /// <summary>
        /// Returns the current accumulated score.
        /// </summary>
        public int GetScore() => totalScore;

        /// <summary>
        /// Resets the score, useful for starting a new session.
        /// </summary>
        public void ResetScore()
        {
            totalScore = 0;
            failedCalls = 0;
        }

        /// <summary>
        /// Returns the number of failed/penalized calls.
        /// </summary>
        public int GetFailedCalls() => failedCalls;

        /// <summary>
        /// Provides a simple rank letter based on current score.
        /// </summary>
        public string GetRank()
        {
            if (totalScore >= 2000)
            {
                return "S";
            }

            if (totalScore >= 1500)
            {
                return "A";
            }

            if (totalScore >= 1000)
            {
                return "B";
            }

            if (totalScore >= 500)
            {
                return "C";
            }

            return "D";
        }
    }
}
