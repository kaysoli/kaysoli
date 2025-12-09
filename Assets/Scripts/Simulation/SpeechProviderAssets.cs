using UnityEngine;

namespace RadioDispatch.Simulation
{
    /// <summary>
    /// Example provider asset that could be wired to Whisper/Vosk bridging code. Currently returns a queued provider but exposes
    /// the hook for future native implementations.
    /// </summary>
    [CreateAssetMenu(menuName = "RadioDispatch/Voice/Queued Speech Provider", fileName = "QueuedSpeechProvider.asset")]
    public class QueuedSpeechProviderAsset : ScriptableSpeechProvider
    {
        public override ISpeechToTextProvider CreateRuntimeProvider()
        {
            return new QueuedSpeechProvider();
        }
    }
}
