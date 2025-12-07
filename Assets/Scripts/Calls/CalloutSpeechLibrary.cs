using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RadioDispatch.Calls
{
    /// <summary>
    /// ScriptableObject that lets designers author voice triggers for callouts without modifying code.
    /// Each entry links a call template with a set of spoken phrases.
    /// </summary>
    [CreateAssetMenu(fileName = "CalloutSpeechLibrary", menuName = "RadioDispatch/Callout Speech Library")]
    public class CalloutSpeechLibrary : ScriptableObject
    {
        [Tooltip("List of callouts with custom voice triggers that can be swapped at runtime.")]
        public List<CalloutSpeechEntry> Callouts = new();

        /// <summary>
        /// Returns a defensive copy so runtime changes never modify the asset.
        /// </summary>
        public List<CalloutSpeechEntry> GetEntries()
        {
            return Callouts?.Select(entry => entry.Clone()).ToList() ?? new List<CalloutSpeechEntry>();
        }
    }

    /// <summary>
    /// Defines the spoken tokens for a single call template, enabling fully customizable callout grammar.
    /// </summary>
    [System.Serializable]
    public class CalloutSpeechEntry
    {
        public CallTemplate Template;
        [Tooltip("Any spoken keywords that should trigger this callout when heard by the interpreter.")]
        public List<string> VoiceTriggers = new();

        public CalloutSpeechEntry Clone()
        {
            return new CalloutSpeechEntry
            {
                Template = Template,
                VoiceTriggers = new List<string>(VoiceTriggers)
            };
        }
    }
}
