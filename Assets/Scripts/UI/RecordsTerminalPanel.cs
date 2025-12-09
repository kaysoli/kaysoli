using System;
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

        [Tooltip("Optional dropdown to restrict searches to a record type (Any + each enum value in RecordType order). Leave empty for no filter.")]
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

            if (typeDropdown.value <= 0)
            {
                return null;
            }

            var enumValues = (RecordType[])Enum.GetValues(typeof(RecordType));
            var enumIndex = typeDropdown.value - 1;
            if (enumIndex < 0 || enumIndex >= enumValues.Length)
            {
                Debug.LogWarning($"RecordsTerminalPanel dropdown index {typeDropdown.value} is out of range for RecordType options.");
                return null;
            }

            return enumValues[enumIndex];
        }
    }
}
