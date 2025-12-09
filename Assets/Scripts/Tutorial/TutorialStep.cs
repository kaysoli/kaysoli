using System;
using System.Collections.Generic;
using System.Linq;
using RadioDispatch.Voice;

namespace RadioDispatch.Tutorial
{
    [Serializable]
    public class TutorialStep
    {
        public string Id;
        public string Instruction;
        public List<string> RequiredKeywords = new();
        public bool RequireCodeUsage;
        public bool RequireCallResolution;

        /// <summary>
        /// Determines if this step is satisfied by the supplied command.
        /// </summary>
        public bool IsComplete(ParsedCommand command)
        {
            if (RequireCallResolution)
            {
                return false;
            }

            if (RequireCodeUsage && (command?.RecognizedCodes == null || command.RecognizedCodes.Count == 0))
            {
                return false;
            }

            if (RequiredKeywords == null || RequiredKeywords.Count == 0)
            {
                return true;
            }

            return RequiredKeywords.All(keyword => command?.RawText?.IndexOf(keyword.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
