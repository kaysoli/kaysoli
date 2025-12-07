using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RadioDispatch.Calls;
using RadioDispatch.Codes;
using RadioDispatch.Units;

namespace RadioDispatch.Voice
{
    public enum CommandType
    {
        Unknown,
        AssignUnitsToCall,
        SendBackup,
        RequestUnitStatus,
        CancelCall,
        CreateCallout,
        EndCallout,
        UpdateUnitStatus,
        PanicCheck,
        LookupRecord
    }

    public enum BackupRequestType
    {
        Standard,
        Code2,
        Code3,
        Pursuit,
        AirSupport,
        Swat,
        Ambulance,
        Firetruck
    }

    /// <summary>
    /// Result of parsing a raw voice command into something executable.
    /// </summary>
    public class ParsedCommand
    {
        public CommandType Type;
        public List<Unit> TargetUnits = new();
        public CallData TargetCall;
        public CallTemplate TargetTemplate;
        public string RawText;
        public List<string> RecognizedKeywords = new();
        public List<CodePhrase> RecognizedCodes = new();
        public UnitStatus? RequestedStatus;
        public BackupRequestType BackupType = BackupRequestType.Standard;
        public bool PanicCheckRequested;
        public string LookupQuery;
        public bool LookupIsVehicle;
    }

    /// <summary>
    /// Lightweight interpreter that searches for keywords and numeric patterns to infer player intent.
    /// </summary>
    public class CommandInterpreter
    {
        private readonly UnitManager unitManager;
        private readonly CallManager callManager;
        private readonly CodeLibrary codeLibrary;
        private readonly VoiceLanguageProfile languageProfile;

        public CommandInterpreter(UnitManager unitManager, CallManager callManager, CodeLibrary codeLibrary = null, VoiceLanguageProfile languageProfile = null)
        {
            this.unitManager = unitManager;
            this.callManager = callManager;
            this.codeLibrary = codeLibrary;
            this.languageProfile = languageProfile;
        }

        public ParsedCommand Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new ParsedCommand { Type = CommandType.Unknown, RawText = text };
            }

            var cleaned = text.ToLowerInvariant();
            var command = new ParsedCommand { RawText = cleaned };

            languageProfile?.ExtractKeywords(cleaned, command.RecognizedKeywords);
            command.TargetUnits.AddRange(ParseUnits(cleaned, command.RecognizedKeywords));
            command.TargetCall = ParseCall(cleaned, command.RecognizedKeywords);
            command.TargetTemplate = ParseCalloutTemplate(cleaned, command.RecognizedKeywords);
            command.RequestedStatus = ParseStatus(cleaned, command.RecognizedKeywords);
            command.BackupType = ParseBackupType(cleaned, command.RecognizedKeywords);
            command.PanicCheckRequested = languageProfile?.ContainsPanic(cleaned) == true || cleaned.Contains("panic");
            ParseLookup(cleaned, command);
            ParseCodes(cleaned, command);
            command.Type = DetermineIntent(cleaned, command.RecognizedCodes, command.RecognizedKeywords, command);

            return command;
        }

        private CommandType DetermineIntent(string cleaned, List<CodePhrase> codes, List<string> recognized, ParsedCommand command)
        {
            if (languageProfile != null && languageProfile.TryMapIntent(cleaned, out var localizedIntent))
            {
                return localizedIntent;
            }

            if (!string.IsNullOrWhiteSpace(command.LookupQuery))
            {
                return CommandType.LookupRecord;
            }

            if (ContainsMeaning(codes, "backup") || cleaned.Contains("backup") || cleaned.Contains("additional"))
            {
                return CommandType.SendBackup;
            }

            if (ContainsMeaning(codes, "end") || cleaned.Contains("end call") || cleaned.Contains("terminate"))
            {
                return CommandType.EndCallout;
            }

            if (ContainsMeaning(codes, "callout") || cleaned.Contains("start call") || cleaned.Contains("callout"))
            {
                return CommandType.CreateCallout;
            }

            if (ContainsMeaning(codes, "panic") || cleaned.Contains("panic"))
            {
                return CommandType.PanicCheck;
            }

            if (ContainsMeaning(codes, "cancel") || cleaned.Contains("cancel"))
            {
                return CommandType.CancelCall;
            }

            if (recognized != null && recognized.Any(keyword => languageProfile != null && languageProfile.IsStatusToken(keyword)))
            {
                return CommandType.UpdateUnitStatus;
            }

            if (ContainsMeaning(codes, "status") || cleaned.Contains("status") || (CommandHasStatus(recognized)))
            {
                return CommandType.RequestUnitStatus;
            }

            if (ContainsMeaning(codes, "respond") || cleaned.Contains("assign") || cleaned.Contains("send") || cleaned.Contains("respond"))
            {
                return CommandType.AssignUnitsToCall;
            }

            return CommandType.Unknown;
        }

        private static bool CommandHasStatus(List<string> recognized)
        {
            return recognized != null && recognized.Any(token => token.ToLowerInvariant().Contains("status"));
        }

        private static bool ContainsMeaning(IEnumerable<CodePhrase> codes, string keyword)
        {
            return codes != null && codes.Any(code => code.Meaning != null && code.Meaning.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private IEnumerable<Unit> ParseUnits(string cleaned, List<string> recognized)
        {
            var units = new List<Unit>();
            var tokens = languageProfile?.GetUnitTokens() ?? new List<string> { "unit" };
            foreach (var token in tokens)
            {
                var pattern = $"{Regex.Escape(token.ToLowerInvariant())}\\s*(\\d+)";
                var matches = Regex.Matches(cleaned, pattern);
                foreach (Match match in matches)
                {
                    var id = match.Groups[1].Value;
                    var unit = unitManager.GetUnitById(id);
                    if (unit != null)
                    {
                        units.Add(unit);
                        recognized?.Add(unit.DisplayName ?? $"Unit {unit.Id}");
                    }
                }
            }

            if (cleaned.Contains("nearest") || cleaned.Contains("available"))
            {
                var available = unitManager.GetAvailableUnits();
                if (available.Count > 0)
                {
                    units.Add(available[0]);
                    recognized?.Add(available[0].DisplayName ?? available[0].Id);
                }
            }

            return units;
        }

        private CallData ParseCall(string cleaned, List<string> recognized)
        {
            var match = Regex.Match(cleaned, "call\\s*(\\d+)");
            if (match.Success)
            {
                var id = match.Groups[1].Value;
                var call = callManager.GetCallById(id);
                if (call != null)
                {
                    recognized?.Add($"Call {call.Id}");
                }

                return call;
            }

            return null;
        }

        private void ParseCodes(string cleaned, ParsedCommand command)
        {
            if (codeLibrary == null)
            {
                return;
            }

            if (codeLibrary.TryFindMatch(cleaned, out var phrase, out var token))
            {
                command.RecognizedCodes.Add(phrase);
                if (!string.IsNullOrWhiteSpace(token))
                {
                    command.RecognizedKeywords.Add(token);
                }
            }
        }

        private CallTemplate ParseCalloutTemplate(string cleaned, List<string> recognized)
        {
            var template = callManager.GetTemplateForSpeech(cleaned);
            if (template != null)
            {
                recognized?.Add(template.title);
            }

            return template;
        }

        private void ParseLookup(string cleaned, ParsedCommand command)
        {
            if (command == null)
            {
                return;
            }

            if (languageProfile != null && languageProfile.TryExtractLookup(cleaned, out var localizedQuery, out var isVehicle))
            {
                command.LookupQuery = localizedQuery;
                command.LookupIsVehicle = isVehicle;
                if (!string.IsNullOrWhiteSpace(localizedQuery))
                {
                    command.RecognizedKeywords.Add(localizedQuery);
                }

                return;
            }

            // Fallback pattern matching for IDs and plates.
            var idMatch = Regex.Match(cleaned, "(?:id|plate|subject)\\s*([a-z0-9]+)");
            if (idMatch.Success)
            {
                command.LookupQuery = idMatch.Groups[1].Value;
                command.LookupIsVehicle = cleaned.Contains("plate");
                command.RecognizedKeywords.Add(command.LookupQuery);
                return;
            }

            var nameMatch = Regex.Match(cleaned, "(?:name|person)\\s*([a-z\\s]+)");
            if (nameMatch.Success)
            {
                command.LookupQuery = nameMatch.Groups[1].Value.Trim();
                command.LookupIsVehicle = false;
                command.RecognizedKeywords.Add(command.LookupQuery);
            }
        }

        private UnitStatus? ParseStatus(string cleaned, List<string> recognized)
        {
            if (languageProfile != null && languageProfile.TryMapStatus(cleaned, out var status, recognized))
            {
                return status;
            }

            if (cleaned.Contains("in custody") || cleaned.Contains("detained"))
            {
                recognized?.Add("InCustody");
                return UnitStatus.InCustody;
            }

            if (cleaned.Contains("pursuit"))
            {
                recognized?.Add("Pursuit");
                return UnitStatus.Pursuit;
            }

            if (cleaned.Contains("transport"))
            {
                recognized?.Add("Transporting");
                return UnitStatus.Transporting;
            }

            return null;
        }

        private BackupRequestType ParseBackupType(string cleaned, List<string> recognized)
        {
            if (languageProfile != null && languageProfile.TryMapBackup(cleaned, out var backup, recognized))
            {
                return backup;
            }

            if (cleaned.Contains("code 2"))
            {
                recognized?.Add("Code 2");
                return BackupRequestType.Code2;
            }

            if (cleaned.Contains("code 3"))
            {
                recognized?.Add("Code 3");
                return BackupRequestType.Code3;
            }

            if (cleaned.Contains("pursuit"))
            {
                recognized?.Add("Pursuit");
                return BackupRequestType.Pursuit;
            }

            if (cleaned.Contains("air"))
            {
                recognized?.Add("AirSupport");
                return BackupRequestType.AirSupport;
            }

            if (cleaned.Contains("swat"))
            {
                recognized?.Add("SWAT");
                return BackupRequestType.Swat;
            }

            if (cleaned.Contains("ambulance") || cleaned.Contains("ems"))
            {
                recognized?.Add("Ambulance");
                return BackupRequestType.Ambulance;
            }

            if (cleaned.Contains("fire"))
            {
                recognized?.Add("Firetruck");
                return BackupRequestType.Firetruck;
            }

            return BackupRequestType.Standard;
        }
    }
}
