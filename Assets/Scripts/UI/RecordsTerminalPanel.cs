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
            var result = recordsManager != null ? recordsManager.Lookup(query) : RecordLookupResult.MissingQuery();
            var display = result.BuildResponse();

            if (outputText != null)
            {
                outputText.text = display;
            }

            radioSystem?.LogMessage("Dispatch", display);
        }
    }
}
