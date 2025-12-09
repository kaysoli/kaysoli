using NUnit.Framework;
using RadioDispatch.Radio;
using UnityEngine;

namespace RadioDispatch.Tests.EditMode
{
    /// <summary>
    /// Ensures per-unit voice profiles expose clips for command execution.
    /// </summary>
    public class UnitVoiceProfileTests
    {
        [Test]
        public void GetClip_ReturnsAssignmentReply()
        {
            var clip = AudioClip.Create("ack", 1, 1, 44100, false);
            var profile = ScriptableObject.CreateInstance<UnitVoiceProfile>();
            profile.AssignmentReplies.Add(clip);

            var returned = profile.GetClip(VoiceResponseType.Assignment);

            Assert.AreEqual(clip, returned);
        }
    }
}
