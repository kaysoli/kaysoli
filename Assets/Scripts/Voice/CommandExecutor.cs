using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RadioDispatch.Calls;
using RadioDispatch.Radio;
using RadioDispatch.Units;
using RadioDispatch.Records;

namespace RadioDispatch.Voice
{
    // Connections: driven by VoiceCommandController; delegates work to UnitManager/CallManager/RecordsManager
    // and emits radio/audio through RadioSystem so downstream UI/radio listeners stay in sync.
    /// <summary>
    /// Executes parsed commands by delegating to the proper managers and logging radio feedback.
    /// </summary>
    public class CommandExecutor
    {
        private readonly UnitManager unitManager;
        private readonly CallManager callManager;
        private readonly RadioSystem radioSystem;
        private readonly RecordsManager recordsManager;
        private VoiceLanguageProfile languageProfile;

        public CommandExecutor(UnitManager unitManager, CallManager callManager, RadioSystem radioSystem, VoiceLanguageProfile languageProfile = null, RecordsManager recordsManager = null)
        {
            this.unitManager = unitManager;
            this.callManager = callManager;
            this.radioSystem = radioSystem;
            this.languageProfile = languageProfile;
            this.recordsManager = recordsManager;
        }

        /// <summary>
        /// Allows language swapping so radio responses can be localized without recreating the executor.
        /// </summary>
        /// <param name="profile">Language profile to apply.</param>
        public void SetLanguageProfile(VoiceLanguageProfile profile)
        {
            languageProfile = profile;
        }

        public void Execute(ParsedCommand command)
        {
            if (command == null)
            {
                Debug.LogWarning("CommandExecutor received null command.");
                return;
            }

            switch (command.Type)
            {
                case CommandType.AssignUnitsToCall:
                    HandleAssignment(command);
                    break;
                case CommandType.SendBackup:
                    HandleBackup(command);
                    break;
                case CommandType.RequestUnitStatus:
                    HandleStatusRequest(command);
                    break;
                case CommandType.CancelCall:
                    HandleCancellation(command);
                    break;
                case CommandType.CreateCallout:
                    HandleCreateCallout(command);
                    break;
                case CommandType.EndCallout:
                    HandleEndCallout(command);
                    break;
                case CommandType.UpdateUnitStatus:
                    HandleStatusUpdate(command);
                    break;
                case CommandType.PanicCheck:
                    HandlePanicCheck(command);
                    break;
                case CommandType.LookupRecord:
                    HandleLookup(command);
                    break;
                case CommandType.QueryAvailableUnits:
                    HandleAvailability();
                    break;
                default:
                    radioSystem.LogMessage("Dispatch", $"Unrecognized command: {command.RawText}");
                    break;
            }
        }

        private void HandleAssignment(ParsedCommand command)
        {
            if (command.TargetCall == null)
            {
                radioSystem.LogMessage("Dispatch", "No call reference detected for assignment.");
                return;
            }

            if (command.TargetUnits.Count == 0)
            {
                radioSystem.LogMessage("Dispatch", "No unit specified. Assigning nearest available unit.");
                var nearest = unitManager.GetAvailableUnits().FirstOrDefault();
                if (nearest == null)
                {
                    radioSystem.LogMessage("Dispatch", "No available units to assign.");
                    return;
                }

                unitManager.AssignUnitToCall(nearest, command.TargetCall);
                radioSystem.LogMessage("Dispatch", FormatAssignment(nearest, command.TargetCall));
                LogOfficerReply(nearest, VoiceResponseType.Assignment, "Copy, en route.");
                return;
            }

            foreach (var unit in command.TargetUnits)
            {
                unitManager.AssignUnitToCall(unit, command.TargetCall);
                radioSystem.LogMessage("Dispatch", FormatAssignment(unit, command.TargetCall));
                LogOfficerReply(unit, VoiceResponseType.Assignment, "Acknowledged.");
            }
        }

        private void HandleBackup(ParsedCommand command)
        {
            var call = command.TargetCall
                       ?? command.TargetUnits.FirstOrDefault(u => u.CurrentCall != null)?.CurrentCall
                       ?? unitManager.GetAllUnits().FirstOrDefault(u => u.CurrentCall != null)?.CurrentCall;
            if (call == null)
            {
                radioSystem.LogMessage("Dispatch", "Backup requested but no call reference detected.");
                return;
            }

            var backupUnit = FindBackupUnit(command.BackupType);
            if (backupUnit == null)
            {
                radioSystem.LogMessage("Dispatch", "No suitable units available for backup.");
                return;
            }

            unitManager.AssignUnitToCall(backupUnit, call);
            radioSystem.LogMessage("Dispatch", FormatBackup(backupUnit, call, command.BackupType));
            LogOfficerReply(backupUnit, VoiceResponseType.Backup, "Backup en route.");
        }

        private void HandleStatusRequest(ParsedCommand command)
        {
            if (command.TargetUnits.Count == 0)
            {
                radioSystem.LogMessage("Dispatch", "No unit specified for status check.");
                return;
            }

            foreach (var unit in command.TargetUnits)
            {
                radioSystem.LogMessage(unit.DisplayName, FormatStatus(unit));
                radioSystem.PlayUnitVoice(unit.VoiceProfile, VoiceResponseType.Status);
            }
        }

        private void HandleStatusUpdate(ParsedCommand command)
        {
            if (command.TargetUnits.Count == 0 || command.RequestedStatus == null)
            {
                radioSystem.LogMessage("Dispatch", "Need a unit and status to update.");
                return;
            }

            foreach (var unit in command.TargetUnits)
            {
                unitManager.ChangeUnitStatus(unit, command.RequestedStatus.Value);
                radioSystem.LogMessage("Dispatch", FormatStatus(unit));
                var responseType = command.RequestedStatus == UnitStatus.InCustody || command.RequestedStatus == UnitStatus.Detaining
                    ? VoiceResponseType.Custody
                    : VoiceResponseType.Status;
                var fallback = command.RequestedStatus == UnitStatus.InCustody || command.RequestedStatus == UnitStatus.Detaining
                    ? "Suspect detained."
                    : "Status received.";
                LogOfficerReply(unit, responseType, fallback);
            }
        }

        private void HandleCancellation(ParsedCommand command)
        {
            if (command.TargetCall == null)
            {
                radioSystem.LogMessage("Dispatch", "No call specified to cancel.");
                return;
            }

            callManager.ResolveCall(command.TargetCall);
            radioSystem.LogMessage("Dispatch", FormatCancellation(command.TargetCall));
        }

        private void HandleCreateCallout(ParsedCommand command)
        {
            if (command.TargetTemplate == null)
            {
                radioSystem.LogMessage("Dispatch", "No matching callout template heard.");
                return;
            }

            var call = callManager.SpawnCallFromTemplate(command.TargetTemplate);
            radioSystem.LogMessage("Dispatch", $"Callout created: {call.Title} at {call.Location}.");

            if (command.TargetUnits.Count > 0)
            {
                foreach (var unit in command.TargetUnits)
                {
                    unitManager.AssignUnitToCall(unit, call);
                    radioSystem.LogMessage("Dispatch", FormatAssignment(unit, call));
                }
            }
        }

        private void HandleEndCallout(ParsedCommand command)
        {
            var call = command.TargetCall ?? callManager.GetActiveCalls().LastOrDefault();
            if (call == null)
            {
                radioSystem.LogMessage("Dispatch", "No active call found to end.");
                return;
            }

            callManager.ResolveCall(call);
            radioSystem.LogMessage("Dispatch", FormatCancellation(call));
            foreach (var unit in call.AssignedUnits)
            {
                LogOfficerReply(unit, VoiceResponseType.Termination, "Clearing callout.");
            }
        }

        private void HandlePanicCheck(ParsedCommand command)
        {
            var units = command.TargetUnits.Count > 0 ? command.TargetUnits : unitManager.GetUnitsByStatus(UnitStatus.Panic);
            if (units.Count == 0)
            {
                radioSystem.LogMessage("Dispatch", "No panic signal linked to a unit.");
                return;
            }

            foreach (var unit in units)
            {
                unitManager.FlagPanic(unit);
                radioSystem.LogMessage("Dispatch", $"{unit.DisplayName}, confirm panic activation. Do you need backup or is this a false alarm?");
                radioSystem.PlayUnitVoice(unit.VoiceProfile, VoiceResponseType.Panic);
            }
        }

        private void HandleLookup(ParsedCommand command)
        {
            if (recordsManager == null)
            {
                radioSystem.LogMessage("Dispatch", "No records system configured.");
                return;
            }

            if (string.IsNullOrWhiteSpace(command.LookupQuery))
            {
                radioSystem.LogMessage("Dispatch", "No ID, plate, or subject provided for lookup.");
                return;
            }

            RecordType? typeFilter = null;
            if (command.LookupIsVehicle)
            {
                typeFilter = Records.RecordType.Vehicle;
            }

            var result = recordsManager.Lookup(command.LookupQuery, typeFilter);
            var fallback = result.BuildResponse();
            var formatted = languageProfile?.FormatLookup(fallback, command.LookupQuery, result.Entry?.BuildSummary() ?? fallback) ?? fallback;
            radioSystem.LogMessage("Dispatch", formatted);

            foreach (var unit in command.TargetUnits)
            {
                LogOfficerReply(unit, VoiceResponseType.Status, "Copy, data received.");
            }
        }

        /// <summary>
        /// Provides a quick roster roll-up for phrases like "any available units?".
        /// </summary>
        private void HandleAvailability()
        {
            var available = unitManager != null ? unitManager.GetUnitsByStatus(UnitStatus.Available) : new List<Unit>();
            var response = languageProfile?.FormatAvailability(available) ?? FormatAvailabilityFallback(available);
            radioSystem.LogMessage("Dispatch", response);

            if (available.Count > 0)
            {
                LogOfficerReply(available[0], VoiceResponseType.Status, "Standing by.");
            }
        }

        private string FormatAssignment(Unit unit, CallData call)
        {
            var unitLabel = GetUnitLabel(unit);
            var fallback = $"{unitLabel} responding to call {call.Id}.";
            return languageProfile?.FormatAssignment(fallback, unitLabel, call.Id) ?? fallback;
        }

        private string FormatBackup(Unit unit, CallData call, BackupRequestType backupType)
        {
            var unitLabel = GetUnitLabel(unit);
            var descriptor = backupType == BackupRequestType.Standard ? string.Empty : $" ({backupType})";
            var fallback = $"Backup{descriptor} dispatched: {unitLabel} to call {call.Id}.";
            return languageProfile?.FormatBackup(fallback, unitLabel, call.Id) ?? fallback;
        }

        private string FormatStatus(Unit unit)
        {
            var unitLabel = GetUnitLabel(unit);
            var fallback = $"Status is {unit.Status}.";
            return languageProfile?.FormatStatus(fallback, unitLabel, unit.Status.ToString()) ?? fallback;
        }

        private static string FormatAvailabilityFallback(IEnumerable<Unit> units)
        {
            var labels = units.Select(GetUnitLabel).ToList();
            return labels.Count == 0 ? "No units available." : $"Available units: {string.Join(", ", labels)}.";
        }

        private string FormatCancellation(CallData call)
        {
            var fallback = $"Call {call.Id} canceled.";
            return languageProfile?.FormatCancellation(fallback, call.Id) ?? fallback;
        }

        private void LogOfficerReply(Unit unit, VoiceResponseType responseType, string fallback = null)
        {
            var reply = languageProfile?.GetOfficerReply();
            var speaker = GetUnitLabel(unit);
            var text = string.IsNullOrWhiteSpace(reply) ? fallback : reply;

            if (!string.IsNullOrWhiteSpace(text))
            {
                radioSystem.LogMessage(speaker, text);
            }

            radioSystem.PlayUnitVoice(unit?.VoiceProfile, responseType);
        }

        private static string GetUnitLabel(Unit unit)
        {
            if (unit == null)
            {
                return "Unit";
            }

            if (!string.IsNullOrWhiteSpace(unit.CallSign))
            {
                return string.IsNullOrWhiteSpace(unit.DisplayName)
                    ? unit.CallSign
                    : $"{unit.CallSign} ({unit.DisplayName})";
            }

            return string.IsNullOrWhiteSpace(unit.DisplayName) ? unit.Id : unit.DisplayName;
        }

        /// <summary>
        /// Picks an appropriate backup unit based on the requested type, falling back to any available unit.
        /// </summary>
        private Unit FindBackupUnit(BackupRequestType type)
        {
            return type switch
            {
                BackupRequestType.AirSupport => unitManager.GetUnitsByType(UnitType.AirSupport).FirstOrDefault(u => u.Status == UnitStatus.Available) ?? unitManager.GetAvailableUnits().FirstOrDefault(),
                BackupRequestType.Swat => unitManager.GetUnitsByType(UnitType.SWAT).FirstOrDefault(u => u.Status == UnitStatus.Available) ?? unitManager.GetAvailableUnits().FirstOrDefault(),
                BackupRequestType.Ambulance => unitManager.GetUnitsByType(UnitType.EMS).FirstOrDefault(u => u.Status == UnitStatus.Available) ?? unitManager.GetAvailableUnits().FirstOrDefault(),
                BackupRequestType.Firetruck => unitManager.GetUnitsByType(UnitType.Traffic).FirstOrDefault(u => u.Status == UnitStatus.Available) ?? unitManager.GetAvailableUnits().FirstOrDefault(),
                _ => unitManager.GetAvailableUnits().FirstOrDefault()
            };
        }
    }
}
