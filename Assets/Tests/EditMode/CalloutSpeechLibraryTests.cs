using System.Collections.Generic;
using NUnit.Framework;
using RadioDispatch.Calls;
using UnityEngine;

namespace RadioDispatch.Tests.EditMode
{
    /// <summary>
    /// Verifies the callout speech library pushes voice triggers into the call manager template pool.
    /// </summary>
    public class CalloutSpeechLibraryTests
    {
        [Test]
        public void MergeSpeechLibrary_AddsVoiceTriggersToTemplate()
        {
            var template = ScriptableObject.CreateInstance<CallTemplate>();
            template.id = "2001";
            template.title = "Test";

            var entry = new CalloutSpeechEntry
            {
                Template = template,
                VoiceTriggers = new List<string> { "bank robbery" }
            };

            var speechLibrary = ScriptableObject.CreateInstance<CalloutSpeechLibrary>();
            speechLibrary.Callouts.Add(entry);

            var managerObj = new GameObject("CallManager");
            var manager = managerObj.AddComponent<CallManager>();
            var libraryField = manager.GetType().GetField("calloutSpeechLibrary", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            libraryField.SetValue(manager, speechLibrary);

            manager.SetTemplates(new List<CallTemplate> { template });

            Assert.Contains("bank robbery", template.voiceTriggers);

            Object.DestroyImmediate(managerObj);
        }
    }
}
