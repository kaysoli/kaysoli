using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using RadioDispatch.Calls;
using RadioDispatch.Codes;
using RadioDispatch.Radio;
using RadioDispatch.Units;

namespace RadioDispatch.Voice
{
    /// <summary>
    /// Bridges voice recognition results to intent parsing and command execution.
    /// Attach alongside a VoiceInputManager and reference core managers.
    /// </summary>
    public class VoiceCommandController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private VoiceInputManager voiceInputManager;

        [SerializeField]
        private UnitManager unitManager;

        [SerializeField]
        private CallManager callManager;

        [SerializeField]
        private RadioSystem radioSystem;

        [SerializeField]
        private Records.RecordsManager recordsManager;

        [Header("Codes & Feedback")]
        [SerializeField]
        private List<CodeSet> codeSets = new();

        [SerializeField]
        private RadioFeedbackProfile feedbackProfile;

        [Header("Localization")]
        [SerializeField]
        private VoiceLanguageProfile languageProfile;

        public UnityEvent<string> OnFeedbackGenerated = new();
        public event System.Action<ParsedCommand, bool> OnCommandHandled;

        private CommandInterpreter interpreter;
        private CommandExecutor executor;
        private CodeLibrary codeLibrary;

        private UnityAction<string> voiceHandler;

        private void Awake()
        {
            voiceHandler = HandleVoiceCommand;
            EnsureInitialized();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        /// <summary>
        /// Allows runtime or test injection of dependencies before enabling.
        /// </summary>
        public void Initialize(UnitManager units, CallManager calls, RadioSystem radio, VoiceInputManager input, IEnumerable<CodeSet> sets = null, RadioFeedbackProfile feedback = null, Records.RecordsManager records = null)
        {
            unitManager = units;
            callManager = calls;
            radioSystem = radio;
            voiceInputManager = input;
            codeSets = sets == null ? new List<CodeSet>() : new List<CodeSet>(sets);
            feedbackProfile = feedback;
            recordsManager = records;
            EnsureInitialized();
            Subscribe();
        }

        /// <summary>
        /// Switches the active language profile at runtime (useful for multi-lingual packs).
        /// </summary>
        /// <param name="profile">Language profile containing localized keywords and replies.</param>
        public void SetLanguageProfile(VoiceLanguageProfile profile)
        {
            languageProfile = profile;
            EnsureInitialized();
        }

        private void Subscribe()
        {
            if (voiceInputManager == null)
            {
                Debug.LogWarning("VoiceCommandController has no VoiceInputManager set.");
                return;
            }

            voiceInputManager.OnVoiceCommandRecognized.RemoveListener(voiceHandler);
            voiceInputManager.OnVoiceCommandRecognized.AddListener(voiceHandler);
        }

        private void Unsubscribe()
        {
            if (voiceInputManager == null)
            {
                return;
            }

            voiceInputManager.OnVoiceCommandRecognized.RemoveListener(voiceHandler);
        }

        private void EnsureInitialized()
        {
            if (unitManager == null || callManager == null || radioSystem == null)
            {
                Debug.LogWarning("VoiceCommandController missing dependencies; cannot initialize parser.");
                return;
            }

            codeLibrary ??= ScriptableObject.CreateInstance<CodeLibrary>();
            codeLibrary.SetCodeSets(codeSets);
            interpreter = new CommandInterpreter(unitManager, callManager, codeLibrary, languageProfile);
            executor = new CommandExecutor(unitManager, callManager, radioSystem, languageProfile, recordsManager);
        }

        private void HandleVoiceCommand(string recognizedText)
        {
            EnsureInitialized();
            if (interpreter == null || executor == null)
            {
                radioSystem?.LogMessage("Dispatch", "Voice command received but parser is not ready.");
                return;
            }

            radioSystem?.BeginTransmission();
            radioSystem?.LogMessage("Dispatch", $"Heard: {recognizedText}");
            var command = interpreter.Parse(recognizedText);
            executor.Execute(command);
            var success = command.Type != CommandType.Unknown;
            EmitFeedback(command, success);
            if (success)
            {
                radioSystem?.PlayUnitResponse();
            }

            radioSystem?.EndTransmission();
            OnCommandHandled?.Invoke(command, success);
        }

        private void EmitFeedback(ParsedCommand command, bool success)
        {
            var feedback = success ? BuildSuccessFeedback(command) : BuildFailureFeedback();
            if (!string.IsNullOrWhiteSpace(feedback))
            {
                OnFeedbackGenerated?.Invoke(feedback);
            }
        }

        private string BuildSuccessFeedback(ParsedCommand command)
        {
            if (feedbackProfile != null)
            {
                return feedbackProfile.BuildSuccessFeedback(command?.RecognizedKeywords);
            }

            var color = ColorUtility.ToHtmlStringRGB(Color.green);
            var keywords = command?.RecognizedKeywords != null && command.RecognizedKeywords.Count > 0
                ? string.Join(", ", command.RecognizedKeywords)
                : "Command acknowledged";
            return $"<color=#{color}>{keywords}</color>";
        }

        private string BuildFailureFeedback()
        {
            if (feedbackProfile != null)
            {
                return feedbackProfile.BuildFailureFeedback();
            }

            var color = ColorUtility.ToHtmlStringRGB(Color.red);
            return $"<color=#{color}>Didn't understand, try: 'Send Unit 21 to call 1023.'</color>";
        }
    }
}
