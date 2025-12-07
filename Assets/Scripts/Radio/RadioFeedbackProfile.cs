using System.Collections.Generic;
using UnityEngine;

namespace RadioDispatch.Radio
{
    /// <summary>
    /// Configurable feedback strings and highlight colors for successful and failed commands.
    /// </summary>
    [CreateAssetMenu(fileName = "RadioFeedbackProfile", menuName = "RadioDispatch/Radio Feedback Profile")]
    public class RadioFeedbackProfile : ScriptableObject
    {
        [Header("Colors")]
        public Color SuccessColor = Color.green;
        public Color FailureColor = Color.red;

        [Header("Success Templates")]
        [Tooltip("Optional text that will be prefixed when a command is understood.")]
        public string SuccessPrefix = "Understood";
        public List<string> SuccessSuffixes = new() { "units dispatched", "copies" };

        [Header("Failure Suggestions")]
        [Tooltip("Randomized suggestions that help the player rephrase commands.")]
        public List<string> Suggestions = new() { "Send Unit 21 to call 1023.", "Request status of Unit 12." };

        /// <summary>
        /// Builds an HTML-ready color tag for success using the configured color and suffix suggestions.
        /// </summary>
        public string BuildSuccessFeedback(IEnumerable<string> keywords)
        {
            var color = ColorUtility.ToHtmlStringRGB(SuccessColor);
            var suffix = SuccessSuffixes.Count > 0 ? SuccessSuffixes[Random.Range(0, SuccessSuffixes.Count)] : "acknowledged";
            var keywordText = keywords == null ? string.Empty : string.Join(", ", keywords);
            return $"<color=#{color}>{SuccessPrefix}: {keywordText} {suffix}</color>".Trim();
        }

        /// <summary>
        /// Provides a random or fallback suggestion for misunderstood commands.
        /// </summary>
        public string BuildFailureFeedback()
        {
            var color = ColorUtility.ToHtmlStringRGB(FailureColor);
            var suggestion = Suggestions.Count > 0 ? Suggestions[Random.Range(0, Suggestions.Count)] : "Send Unit 21 to call 1023.";
            return $"<color=#{color}>Didn't understand, try: '{suggestion}'</color>";
        }
    }
}
