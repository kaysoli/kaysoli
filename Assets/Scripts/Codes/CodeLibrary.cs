using System.Collections.Generic;
using UnityEngine;

namespace RadioDispatch.Codes
{
    /// <summary>
    /// Aggregates multiple <see cref="CodeSet"/> assets so designers can mix and match
    /// radio code vocabularies without changing gameplay logic.
    /// </summary>
    [CreateAssetMenu(fileName = "CodeLibrary", menuName = "RadioDispatch/Code Library")]
    public class CodeLibrary : ScriptableObject
    {
        [SerializeField]
        private List<CodeSet> codeSets = new();

        /// <summary>
        /// Registers a set at runtime or via inspector. Duplicates are ignored.
        /// </summary>
        public void Register(CodeSet set)
        {
            if (set == null || codeSets.Contains(set))
            {
                return;
            }

            codeSets.Add(set);
        }

        /// <summary>
        /// Replaces all sets with a supplied collection.
        /// </summary>
        public void SetCodeSets(IEnumerable<CodeSet> sets)
        {
            codeSets.Clear();
            if (sets == null)
            {
                return;
            }

            codeSets.AddRange(sets);
        }

        /// <summary>
        /// Attempts to locate a matching code phrase anywhere in the provided text.
        /// Returns true if a match is found along with the matched phrase and the text token used.
        /// </summary>
        public bool TryFindMatch(string text, out CodePhrase phrase, out string matchedToken)
        {
            phrase = null;
            matchedToken = null;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            foreach (var set in codeSets)
            {
                if (set != null && set.TryFindInText(text, out phrase, out matchedToken))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
