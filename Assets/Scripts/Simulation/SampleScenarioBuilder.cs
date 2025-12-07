using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RadioDispatch.Calls;
using RadioDispatch.Core;
using RadioDispatch.Radio;
using RadioDispatch.Records;
using RadioDispatch.Units;

namespace RadioDispatch.Simulation
{
    /// <summary>
    /// Lightweight bootstrap for a designer-friendly sample scene. Drop this on an empty GameObject, assign the
    /// managers/assets you want to test, and it will load rosters/records, wire chatter, and kick off a short
    /// demo sequence (spawn a call, dispatch the first available unit, and play a chatter response) without the
    /// full terminal UI.
    /// </summary>
    [DisallowMultipleComponent]
    public class SampleScenarioBuilder : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField]
        [Tooltip("Optional GameManager used to start shift timers for the sample run.")]
        private GameManager gameManager;

        [SerializeField]
        [Tooltip("Unit manager that will receive roster data and be used for dispatching the sample call.")]
        private UnitManager unitManager;

        [SerializeField]
        [Tooltip("Call manager that will be seeded with templates and used to spawn the sample incident.")]
        private CallManager callManager;

        [SerializeField]
        [Tooltip("Radio system that will play roger beeps and officer voice lines during the sample.")]
        private RadioSystem radioSystem;

        [SerializeField]
        [Tooltip("Chatter manager that will play a sample request/response to prove unit → dispatch → unit flow.")]
        private ChatterManager chatterManager;

        [SerializeField]
        [Tooltip("Records manager used for lookup responses when chatter requests subject/plate data.")]
        private RecordsManager recordsManager;

        [Header("Content Assets")]
        [SerializeField]
        [Tooltip("Roster to load; cloned at runtime so edits do not mutate the asset.")]
        private UnitRoster roster;

        [SerializeField]
        [Tooltip("Call templates that the sample call will be drawn from.")]
        private CallLibrary callLibrary;

        [SerializeField]
        [Tooltip("Chatter lines that will be used for the sample request/response.")]
        private ChatterLibrary chatterLibrary;

        [SerializeField]
        [Tooltip("Optional radio audio profile containing roger beep and shared voice clips.")]
        private RadioAudioProfile audioProfile;

        [SerializeField]
        [Tooltip("Records database asset for subject/vehicle lookups.")]
        private RecordsDatabase recordsDatabase;

        [SerializeField]
        [Tooltip("Additional CSV/TSV sheets to merge into the records database for the sample run.")]
        private List<TextAsset> csvSheets = new();

        [Header("Flow Toggles")]
        [SerializeField]
        [Tooltip("If true, the builder will load content and run the sample sequence on Start.")]
        private bool autoRun = true;

        [SerializeField]
        [Tooltip("Delay before triggering the first chatter line after the sample call is dispatched.")]
        private float chatterDelaySeconds = 3f;

        [SerializeField]
        [Tooltip("If true, a random chatter line will play after the sample call is spawned.")]
        private bool playSampleChatter = true;

        private bool scenarioBuilt;

        private void Start()
        {
            if (autoRun)
            {
                BuildScenario();
                StartCoroutine(RunSampleSequence());
            }
        }

        /// <summary>
        /// Loads rosters, call templates, records, and chatter libraries into their respective managers.
        /// Safe to call multiple times; subsequent calls simply reapply content.
        /// </summary>
        public void BuildScenario()
        {
            scenarioBuilt = true;
            ApplyAudioProfile();
            LoadRecords();
            LoadRoster();
            LoadCallTemplates();
            LoadChatter();

            if (gameManager != null)
            {
                gameManager.BeginShift();
            }
        }

        /// <summary>
        /// Spawns a single sample call from the first template, dispatches the first available unit, and plays an optional
        /// chatter line so designers can hear the end-to-end loop without wiring full UI.
        /// </summary>
        public IEnumerator RunSampleSequence()
        {
            if (!scenarioBuilt)
            {
                BuildScenario();
            }

            // Spawn a sample call if possible.
            CallData call = null;
            if (callManager != null)
            {
                var template = callLibrary != null ? callLibrary.GetTemplates().FirstOrDefault() : null;
                if (template != null)
                {
                    call = callManager.SpawnCallFromTemplate(template);
                }
            }

            // Assign the first available unit to the call to validate radio/log updates.
            if (call != null && unitManager != null)
            {
                var unit = unitManager.GetAvailableUnits().FirstOrDefault();
                if (unit != null)
                {
                    unitManager.AssignUnitToCall(unit, call);
                    radioSystem?.LogMessage("Dispatch", $"{unit.DisplayName ?? unit.Id} responding to {call.Id}");
                }
            }

            if (playSampleChatter && chatterManager != null)
            {
                yield return new WaitForSeconds(chatterDelaySeconds);
                chatterManager.PlayRandomChatter();
            }
        }

        private void ApplyAudioProfile()
        {
            if (audioProfile != null && radioSystem != null)
            {
                radioSystem.SetAudioProfile(audioProfile);
            }
        }

        private void LoadRecords()
        {
            if (recordsManager == null)
            {
                return;
            }

            if (recordsDatabase != null)
            {
                recordsManager.SetDatabase(recordsDatabase);
            }

            if (csvSheets != null && csvSheets.Count > 0)
            {
                recordsManager.SetCsvSheets(csvSheets);
            }

            recordsManager.LoadDatabases();
        }

        private void LoadRoster()
        {
            if (unitManager == null || roster == null)
            {
                return;
            }

            unitManager.SetUnits(roster.GetClonedUnits());
        }

        private void LoadCallTemplates()
        {
            if (callManager == null || callLibrary == null)
            {
                return;
            }

            callManager.SetTemplates(callLibrary.GetTemplates());
        }

        private void LoadChatter()
        {
            if (chatterManager == null)
            {
                return;
            }

            if (chatterLibrary != null)
            {
                chatterManager.SetLibrary(chatterLibrary);
            }

            // Ensure dependencies are wired when used outside prefabs.
            chatterManager.Initialize(unitManager, recordsManager, radioSystem);
        }
    }
}
