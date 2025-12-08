using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RadioDispatch.Radio;
using RadioDispatch.Units;

namespace RadioDispatch.Records
{
    /// <summary>
    /// Handles record lookups (subjects, vehicles, officers) and surfaces results to the radio and UI terminals.
    /// </summary>
    public class RecordsManager : MonoBehaviour
    {
        [Header("Data Sources")]
        [SerializeField]
        private RecordsDatabase defaultDatabase;

        [Tooltip("Optional CSV/TSV sheets to load at startup. Supports simple Id,Name rows or the verbose RECORD_ID...IS_WITNESS_PROTECTION header.")]
        [SerializeField]
        private List<TextAsset> csvSheets = new();

        /// <summary>
        /// Allows bootstrap scripts (e.g., simulation sandboxes) to inject CSV/TSV sheets at runtime before <see cref="LoadDatabases"/> is called.
        /// Useful when you want to keep thousands of civilian/officer entries in external spreadsheets without modifying the default asset.
        /// </summary>
        public void SetCsvSheets(IEnumerable<TextAsset> sheets)
        {
            csvSheets = sheets == null ? new List<TextAsset>() : sheets.ToList();
        }

        [Header("Outputs")]
        [SerializeField]
        private RadioSystem radioSystem;

        private readonly List<RecordEntry> runtimeRecords = new();

        /// <summary>
        /// Event fired whenever a lookup completes so UI can display results.
        /// </summary>
        public event Action<RecordLookupResult> OnLookupCompleted;

        private void Awake()
        {
            LoadDatabases();
        }

        /// <summary>
        /// Allows callers to swap the default database at runtime (useful for tests or fallback seeding).
        /// </summary>
        public void SetDatabase(RecordsDatabase database)
        {
            defaultDatabase = database;
        }

        /// <summary>
        /// Clears and reloads runtime records from the default database and any CSV sheets.
        /// </summary>
        public void LoadDatabases()
        {
            runtimeRecords.Clear();

            if (defaultDatabase != null)
            {
                // Ensure any attached CSV/TSV files populate the asset before cloning rows.
                defaultDatabase.RebuildFromAttachedCsvs();
                foreach (var entry in defaultDatabase.CloneAll())
                {
                    AppendRecord(entry);
                }
            }

            foreach (var sheet in csvSheets)
            {
                // To avoid mutating the ScriptableObject, create a temporary database for parsing.
                var tempDb = ScriptableObject.CreateInstance<RecordsDatabase>();
                tempDb.ImportCsv(sheet);
                foreach (var entry in tempDb.CloneAll())
                {
                    AppendRecord(entry);
                }
            }
        }

        /// <summary>
        /// Searches by ID, name, or vehicle plate (case-insensitive). Optionally restricts results to a specific record type
        /// (civilian, officer, prisoner, vehicle) so dispatchers can target the correct database tab. Returns the most specific
        /// match or a not-found notice.
        /// </summary>
        public RecordLookupResult Lookup(string query, RecordType? typeFilter = null)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RecordLookupResult.MissingQuery();
            }

            var trimmed = query.Trim();
            // First pass: enforce type filter if provided so officers get the right subject/vehicle/officer hit.
            var filtered = typeFilter.HasValue
                ? runtimeRecords.Where(r => r.Type == typeFilter.Value).ToList()
                : runtimeRecords;

            var match = filtered.FirstOrDefault(r => r.Id.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
                        ?? filtered.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.VehiclePlate) && r.VehiclePlate.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
                        ?? filtered.FirstOrDefault(r => r.Name != null && r.Name.IndexOf(trimmed, StringComparison.OrdinalIgnoreCase) >= 0);

            // Second pass: if a filter was set but nothing matched, expand to any record so the dispatcher still hears a reply.
            if (match == null && typeFilter.HasValue)
            {
                match = runtimeRecords.FirstOrDefault(r => r.Id.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
                        ?? runtimeRecords.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.VehiclePlate) && r.VehiclePlate.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
                        ?? runtimeRecords.FirstOrDefault(r => r.Name != null && r.Name.IndexOf(trimmed, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (match == null)
            {
                var notFound = RecordLookupResult.NotFound(trimmed);
                OnLookupCompleted?.Invoke(notFound);
                return notFound;
            }

            var result = RecordLookupResult.Success(trimmed, match);
            OnLookupCompleted?.Invoke(result);
            return result;
        }

        /// <summary>
        /// Allows units to request information (plate checks, subject lookups) over radio, emitting responses and voice lines.
        /// </summary>
        public void HandleUnitLookupRequest(Unit requestingUnit, string query, RecordType? typeFilter = null)
        {
            var result = Lookup(query, typeFilter);
            var speaker = requestingUnit != null ? requestingUnit.DisplayName ?? requestingUnit.Id : "Unit";

            radioSystem?.BeginTransmission();
            radioSystem?.LogMessage(speaker, $"Requesting data on {result.Query}");
            radioSystem?.LogMessage("Dispatch", result.BuildResponse());
            if (requestingUnit != null)
            {
                radioSystem?.PlayUnitVoice(requestingUnit.VoiceProfile, VoiceResponseType.Status);
            }

            radioSystem?.EndTransmission();
        }

        /// <summary>
        /// Adds a record to the runtime list, replacing any existing entry with the same ID.
        /// </summary>
        private void AppendRecord(RecordEntry entry)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.Id))
            {
                return;
            }

            var existing = runtimeRecords.Find(r => r.Id.Equals(entry.Id, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                runtimeRecords.Remove(existing);
            }

            runtimeRecords.Add(entry);
        }
    }
}
