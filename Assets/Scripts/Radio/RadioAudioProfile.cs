using System.Collections.Generic;
using UnityEngine;

namespace RadioDispatch.Radio
{
    /// <summary>
    /// Designer-friendly configuration for radio beeps, static, and unit responses.
    /// </summary>
    [CreateAssetMenu(fileName = "RadioAudioProfile", menuName = "RadioDispatch/Radio Audio Profile")]
    public class RadioAudioProfile : ScriptableObject
    {
        [Header("Clicks & Static")]
        public AudioClip ClickIn;
        public AudioClip ClickOut;
        public AudioClip StaticLoop;
        public bool LoopStaticDuringTransmission = true;

        [Header("Voices & Replies")]
        public List<AudioClip> UnitResponses = new();

        [Header("Mix")] 
        [Range(0f, 1f)] public float ClickVolume = 0.9f;
        [Range(0f, 1f)] public float StaticVolume = 0.2f;
        [Range(0f, 1f)] public float VoiceVolume = 0.9f;
    }
}
