using UnityEngine;
using UnityEngine.UI;
using RadioDispatch.Radio;
using RadioDispatch.Records;

namespace RadioDispatch.UI
{
    /// <summary>
    /// Simple terminal-style panel that accepts a search query and prints lookup results.
    /// </summary>
    public class RecordsTerminalPanel : MonoBehaviour
    {
        [Header("UI Widgets")]
        [SerializeField]
        private InputField queryField;

        [SerializeField]
        private Text outputText;

        [Tooltip("Optional dropdown to restrict searches to a record type (Any/Civilian/Officer/Prisoner/Vehicle). Leave empty for no filter.")]
        [SerializeField]
        private Dropdown typeDropdown;

        [Header("Dependencies")]
        [SerializeField]
        private RecordsManager recordsManager;

        [SerializeField]
        private RadioSystem radioSystem;

        /// <summary>
        /// Convenience initializer for wiring dependencies from UIManager or tests.
        /// </summary>
        public void Initialize(RecordsManager manager, RadioSystem radio)
        {
            recordsManager = manager;
            radioSystem = radio;
        }

        /// <summary>
        /// Button callback to search for the provided query and display the result.
        /// </summary>
        public void SubmitQuery()
        {
            var query = queryField != null ? queryField.text : string.Empty;
            var typeFilter = ParseSelectedType();
            var result = recordsManager != null ? recordsManager.Lookup(query, typeFilter) : RecordLookupResult.MissingQuery();
            var display = result.BuildResponse();

            if (outputText != null)
            {
                outputText.text = display;
            }

            radioSystem?.LogMessage("Dispatch", display);
        }

        private RecordType? ParseSelectedType()
        {
            if (typeDropdown == null)
            {
                return null;
            }

            // Expect dropdown options to align with RecordType ordering plus an "Any" entry at index 0.
            return typeDropdown.value switch
            {
                1 => RecordType.Civilian,
                2 => RecordType.Officer,
                3 => RecordType.Prisoner,
                4 => RecordType.Vehicle,
                _ => null
            };
        }
    }
}
