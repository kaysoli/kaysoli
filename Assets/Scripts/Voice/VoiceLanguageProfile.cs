using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RadioDispatch.Voice
{
    /// <summary>
    /// ScriptableObject that holds localized keywords for parsing and localized response templates for radio chatter.
    /// This allows designers to swap in language packs without modifying code.
    /// </summary>
    [CreateAssetMenu(fileName = "VoiceLanguageProfile", menuName = "RadioDispatch/Voice Language Profile")]
    public class VoiceLanguageProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string languageName = "English";

        [Header("Intent Keywords")]
        [SerializeField]
        private List<string> assignKeywords = new() { "assign", "send", "respond" };

        [SerializeField]
        private List<string> backupKeywords = new() { "backup", "additional" };

        [SerializeField]
        private List<string> statusKeywords = new() { "status" };

        [SerializeField]
        private List<string> cancelKeywords = new() { "cancel" };

        [SerializeField]
        private List<string> startCalloutKeywords = new() { "start call", "begin call" };

        [SerializeField]
        private List<string> endCalloutKeywords = new() { "end call", "terminate" };

        [SerializeField]
        private List<string> panicKeywords = new() { "panic", "signal 100" };

        [Header("Grammar Mapping")]
        [SerializeField]
        private List<StatusKeyword> statusTokens = new()
        {
            new StatusKeyword("available", Units.UnitStatus.Available),
            new StatusKeyword("en route", Units.UnitStatus.EnRoute),
            new StatusKeyword("on scene", Units.UnitStatus.OnScene),
            new StatusKeyword("busy", Units.UnitStatus.Busy),
            new StatusKeyword("pursuit", Units.UnitStatus.Pursuit),
            new StatusKeyword("transport", Units.UnitStatus.Transporting),
            new StatusKeyword("detained", Units.UnitStatus.InCustody),
            new StatusKeyword("in custody", Units.UnitStatus.InCustody),
            new StatusKeyword("returning", Units.UnitStatus.Returning)
        };

        [SerializeField]
        private List<BackupKeyword> backupTokens = new()
        {
            new BackupKeyword("code 2", BackupRequestType.Code2),
            new BackupKeyword("code 3", BackupRequestType.Code3),
            new BackupKeyword("pursuit", BackupRequestType.Pursuit),
            new BackupKeyword("air", BackupRequestType.AirSupport),
            new BackupKeyword("helicopter", BackupRequestType.AirSupport),
            new BackupKeyword("swat", BackupRequestType.Swat),
            new BackupKeyword("ambulance", BackupRequestType.Ambulance),
            new BackupKeyword("ems", BackupRequestType.Ambulance),
            new BackupKeyword("fire", BackupRequestType.Firetruck)
        };

        [Header("Unit Tokens")]
        [SerializeField]
        private List<string> unitTokens = new() { "unit" };

        [Header("Response Templates")]
        [Tooltip("Template used when acknowledging an assignment. Supports {unit} and {call} tokens.")]
        [SerializeField]
        private string assignmentTemplate = "{unit} responding to call {call}.";

        [Tooltip("Template used when dispatch sends backup.")]
        [SerializeField]
        private string backupTemplate = "Backup dispatched: {unit} to call {call}.";

        [Tooltip("Template for status responses (uses {unit} and {status}).")]
        [SerializeField]
        private string statusTemplate = "{unit} reports {status}.";

        [Tooltip("Template for cancel/resolve replies (uses {call}).")]
        [SerializeField]
        private string cancellationTemplate = "Call {call} canceled.";

        [Header("Officer Replies")]
        [SerializeField]
        private List<string> officerReplies = new() { "Copy, en route", "10-4", "Acknowledged" };

        /// <summary>
        /// Human-readable language name to display in settings or debugging tools.
        /// </summary>
        public string LanguageName => languageName;

        /// <summary>
        /// Returns tokens that should be interpreted as "unit" when extracting numeric identifiers.
        /// </summary>
        public IEnumerable<string> GetUnitTokens()
        {
            return unitTokens ?? new List<string>();
        }

        /// <summary>
        /// Overrides the unit tokens at runtime (useful for tests or dynamic language swaps).
        /// </summary>
        public void SetUnitTokens(IEnumerable<string> tokens)
        {
            unitTokens = tokens == null ? new List<string>() : tokens.Select(t => t.ToLowerInvariant()).ToList();
        }

        /// <summary>
        /// Convenience helper to replace intent keyword lists at runtime.
        /// </summary>
        public void SetIntentKeywords(IEnumerable<string> assign, IEnumerable<string> backup, IEnumerable<string> status, IEnumerable<string> cancel)
        {
            assignKeywords = assign == null ? new List<string>() : assign.Select(k => k.ToLowerInvariant()).ToList();
            backupKeywords = backup == null ? new List<string>() : backup.Select(k => k.ToLowerInvariant()).ToList();
            statusKeywords = status == null ? new List<string>() : status.Select(k => k.ToLowerInvariant()).ToList();
            cancelKeywords = cancel == null ? new List<string>() : cancel.Select(k => k.ToLowerInvariant()).ToList();
        }

        /// <summary>
        /// Replaces status and backup keyword dictionaries to support modded language packs.
        /// </summary>
        public void SetGrammarMappings(IEnumerable<StatusKeyword> statuses, IEnumerable<BackupKeyword> backups)
        {
            statusTokens = statuses == null ? new List<StatusKeyword>() : statuses.ToList();
            backupTokens = backups == null ? new List<BackupKeyword>() : backups.ToList();
        }

        /// <summary>
        /// Checks the provided text for keywords that map to known command intents.
        /// </summary>
        public bool TryMapIntent(string cleanedText, out CommandType commandType)
        {
            commandType = CommandType.Unknown;
            if (string.IsNullOrWhiteSpace(cleanedText))
            {
                return false;
            }

            if (ContainsAny(cleanedText, cancelKeywords))
            {
                commandType = CommandType.CancelCall;
                return true;
            }

            if (ContainsAny(cleanedText, endCalloutKeywords))
            {
                commandType = CommandType.EndCallout;
                return true;
            }

            if (ContainsAny(cleanedText, backupKeywords))
            {
                commandType = CommandType.SendBackup;
                return true;
            }

            if (ContainsAny(cleanedText, startCalloutKeywords))
            {
                commandType = CommandType.CreateCallout;
                return true;
            }

            if (ContainsAny(cleanedText, statusKeywords))
            {
                commandType = CommandType.RequestUnitStatus;
                return true;
            }

            if (ContainsAny(cleanedText, panicKeywords))
            {
                commandType = CommandType.PanicCheck;
                return true;
            }

            if (ContainsAny(cleanedText, assignKeywords))
            {
                commandType = CommandType.AssignUnitsToCall;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds any matched keywords to the recognized list so UI can surface them.
        /// </summary>
        public void ExtractKeywords(string cleanedText, List<string> recognized)
        {
            if (recognized == null || string.IsNullOrWhiteSpace(cleanedText))
            {
                return;
            }

            AddMatches(cleanedText, assignKeywords, recognized);
            AddMatches(cleanedText, backupKeywords, recognized);
            AddMatches(cleanedText, statusKeywords, recognized);
            AddMatches(cleanedText, cancelKeywords, recognized);
            AddMatches(cleanedText, startCalloutKeywords, recognized);
            AddMatches(cleanedText, endCalloutKeywords, recognized);
            AddMatches(cleanedText, panicKeywords, recognized);
        }

        /// <summary>
        /// Builds a localized phrase using known tokens, falling back to the provided default when empty.
        /// </summary>
        public string FormatAssignment(string fallback, string unitLabel, string callLabel)
        {
            return string.IsNullOrWhiteSpace(assignmentTemplate)
                ? fallback
                : assignmentTemplate.Replace("{unit}", unitLabel).Replace("{call}", callLabel);
        }

        public string FormatBackup(string fallback, string unitLabel, string callLabel)
        {
            return string.IsNullOrWhiteSpace(backupTemplate)
                ? fallback
                : backupTemplate.Replace("{unit}", unitLabel).Replace("{call}", callLabel);
        }

        public string FormatStatus(string fallback, string unitLabel, string statusLabel)
        {
            return string.IsNullOrWhiteSpace(statusTemplate)
                ? fallback
                : statusTemplate.Replace("{unit}", unitLabel).Replace("{status}", statusLabel);
        }

        public string FormatCancellation(string fallback, string callLabel)
        {
            return string.IsNullOrWhiteSpace(cancellationTemplate)
                ? fallback
                : cancellationTemplate.Replace("{call}", callLabel);
        }

        /// <summary>
        /// Retrieves a random officer reply in the configured language for immersion.
        /// </summary>
        public string GetOfficerReply()
        {
            if (officerReplies == null || officerReplies.Count == 0)
            {
                return null;
            }

            return officerReplies[Random.Range(0, officerReplies.Count)];
        }

        /// <summary>
        /// Checks for panic words in the recognized speech.
        /// </summary>
        public bool ContainsPanic(string cleanedText)
        {
            return ContainsAny(cleanedText, panicKeywords);
        }

        /// <summary>
        /// Tries to map speech to a known status token for grammar-style "show me ..." commands.
        /// </summary>
        public bool TryMapStatus(string cleanedText, out Units.UnitStatus status, List<string> recognized = null)
        {
            status = Units.UnitStatus.Available;
            if (statusTokens == null)
            {
                return false;
            }

            foreach (var token in statusTokens)
            {
                if (!string.IsNullOrWhiteSpace(token.Keyword) && cleanedText.Contains(token.Keyword.ToLowerInvariant()))
                {
                    recognized?.Add(token.Keyword);
                    status = token.Status;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to map speech to a backup request type such as Code 2/3 or specialty units.
        /// </summary>
        public bool TryMapBackup(string cleanedText, out BackupRequestType backup, List<string> recognized = null)
        {
            backup = BackupRequestType.Standard;
            if (backupTokens == null)
            {
                return false;
            }

            foreach (var token in backupTokens)
            {
                if (!string.IsNullOrWhiteSpace(token.Keyword) && cleanedText.Contains(token.Keyword.ToLowerInvariant()))
                {
                    recognized?.Add(token.Keyword);
                    backup = token.BackupType;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Indicates if a token was a status phrase so the interpreter can switch to UpdateUnitStatus intent.
        /// </summary>
        public bool IsStatusToken(string token)
        {
            return statusTokens != null && statusTokens.Exists(mapping => mapping.Keyword.Equals(token, System.StringComparison.OrdinalIgnoreCase));
        }

        private static void AddMatches(string text, IEnumerable<string> keywords, ICollection<string> recognized)
        {
            if (keywords == null)
            {
                return;
            }

            foreach (var keyword in keywords.Where(k => !string.IsNullOrWhiteSpace(k)))
            {
                if (text.Contains(keyword.ToLowerInvariant()))
                {
                    recognized.Add(keyword);
                }
            }
        }

        private static bool ContainsAny(string text, IEnumerable<string> keywords)
        {
            return keywords != null && keywords.Any(k => !string.IsNullOrWhiteSpace(k) && text.Contains(k.ToLowerInvariant()));
        }
    }

    /// <summary>
    /// Serializable mapping from a keyword to a status so designers can author grammar packs.
    /// </summary>
    [System.Serializable]
    public class StatusKeyword
    {
        public string Keyword;
        public Units.UnitStatus Status;

        public StatusKeyword(string keyword, Units.UnitStatus status)
        {
            Keyword = keyword;
            Status = status;
        }
    }

    /// <summary>
    /// Serializable mapping from keyword to backup type (Code 2/3, pursuit, specialty units).
    /// </summary>
    [System.Serializable]
    public class BackupKeyword
    {
        public string Keyword;
        public BackupRequestType BackupType;

        public BackupKeyword(string keyword, BackupRequestType type)
        {
            Keyword = keyword;
            BackupType = type;
        }
    }
}
