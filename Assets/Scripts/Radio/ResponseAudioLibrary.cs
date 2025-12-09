using System;
using System.Collections.Generic;
using UnityEngine;

namespace RadioDispatch.Radio
{
    /// <summary>
    /// ScriptableObject that maps response keywords ("Acknowledgement", "Status", etc.) to multiple audio clip variations.
    /// /// This lets designers drag WAV files for common replies without touching code and keeps officer replies varied.
    /// </summary>
    [CreateAssetMenu(fileName = "ResponseAudioLibrary", menuName = "RadioDispatch/Response Audio Library")]
    public class ResponseAudioLibrary : ScriptableObject
    {
        [Tooltip("Audio buckets keyed by response keyword (e.g., Acknowledgement, Status, Backup, Panic).")]
        public List<ResponseAudioEntry> Responses = new();

        /// <summary>
        /// Retrieves a random clip for the given key (case-insensitive). Returns null when missing so callers can fallback.
        /// </summary>
        public AudioClip GetClip(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            var match = Responses.Find(r => r.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (match == null || match.Clips.Count == 0)
            {
                return null;
            }

            var index = UnityEngine.Random.Range(0, match.Clips.Count);
            return match.Clips[index];
        }
    }

    /// <summary>
    /// Maps a response keyword to one or more audio clips so radio replies can be varied per keyword.
    /// </summary>
    [Serializable]
    public class ResponseAudioEntry
    {
        [Tooltip("Lookup key used by the radio/voice systems (e.g., Acknowledgement, Status, Backup, Panic).")]
        public string Key;

        [Tooltip("All clip variations that can play for this key.")]
        public List<AudioClip> Clips = new();
    }
}
