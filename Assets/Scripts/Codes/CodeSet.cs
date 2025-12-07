using System.Collections.Generic;
using UnityEngine;

namespace RadioDispatch.Codes
{
    [CreateAssetMenu(fileName = "CodeSet", menuName = "RadioDispatch/Code Set")]
    public class CodeSet : ScriptableObject
    {
        public string Name;
        public List<CodePhrase> Phrases = new();

        /// <summary>
        /// Attempts to match a natural language phrase to a known code meaning.
        /// </summary>
        public CodePhrase FindMatch(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            return TryFindInText(input, out var phrase, out _) ? phrase : null;
        }

        /// <summary>
        /// Attempts to find a code or variant inside a larger phrase. Returns both the code
        /// and the matched token if successful.
        /// </summary>
        public bool TryFindInText(string input, out CodePhrase phrase, out string matchedToken)
        {
            phrase = null;
            matchedToken = null;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            input = input.ToLowerInvariant();
            foreach (var candidate in Phrases)
            {
                if (candidate == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(candidate.Code) && input.Contains(candidate.Code.ToLowerInvariant()))
                {
                    phrase = candidate;
                    matchedToken = candidate.Code;
                    return true;
                }

                foreach (var variant in candidate.Variants)
                {
                    if (string.IsNullOrWhiteSpace(variant))
                    {
                        continue;
                    }

                    if (input.Contains(variant.ToLowerInvariant()))
                    {
                        phrase = candidate;
                        matchedToken = variant;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
