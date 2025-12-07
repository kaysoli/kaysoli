using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RadioDispatch.Records
{
    /// <summary>
    /// ScriptableObject that stores subject/vehicle records and can optionally import simple CSV sheets for rapid authoring.
    /// </summary>
    [CreateAssetMenu(fileName = "RecordsDatabase", menuName = "RadioDispatch/Records Database")]
    public class RecordsDatabase : ScriptableObject
    {
        [Tooltip("Initial records authored directly in the asset.")]
        public List<RecordEntry> Records = new();

        /// <summary>
        /// Merges CSV content into the database. Columns: Id,Name,Type,VehiclePlate,Notes,Metadata,Occupation (optional)
        /// </summary>
        /// <param name="csvAsset">Text asset containing comma or semicolon separated rows.</param>
        /// <param name="separator">Column separator, defaults to comma.</param>
        public void ImportCsv(TextAsset csvAsset, char separator = ',')
        {
            if (csvAsset == null || string.IsNullOrWhiteSpace(csvAsset.text))
            {
                Debug.LogWarning("RecordsDatabase.ImportCsv received empty asset.");
                return;
            }

            using var reader = new StringReader(csvAsset.text);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue; // Skip comments and blanks.
                }

                var parts = line.Split(separator);
                if (parts.Length < 2)
                {
                    Debug.LogWarning($"RecordsDatabase.ImportCsv skipping malformed row: {line}");
                    continue;
                }

                var entry = new RecordEntry
                {
                    Id = parts[0].Trim(),
                    Name = parts[1].Trim(),
                };

                entry.Type = ParseType(parts.ElementAtOrDefault(2), out var customCategory);
                entry.CustomCategoryLabel = customCategory;
                entry.VehiclePlate = parts.ElementAtOrDefault(3)?.Trim();
                entry.Notes = parts.ElementAtOrDefault(4)?.Trim();
                entry.Metadata = parts.ElementAtOrDefault(5)?.Trim();
                entry.Occupation = parts.ElementAtOrDefault(6)?.Trim();

                AddOrReplace(entry);
            }
        }

        /// <summary>
        /// Adds or updates a record in-place by matching ID (case-insensitive).
        /// </summary>
        public void AddOrReplace(RecordEntry entry)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.Id))
            {
                Debug.LogWarning("RecordsDatabase.AddOrReplace ignored null or missing ID.");
                return;
            }

            var existing = Records.Find(r => r.Id.Equals(entry.Id, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                Records.Remove(existing);
            }

            Records.Add(entry);
        }

        /// <summary>
        /// Returns a shallow copy of all records to prevent accidental asset mutation at runtime.
        /// </summary>
        public List<RecordEntry> CloneAll()
        {
            return Records.Select(Clone).ToList();
        }

        private static RecordEntry Clone(RecordEntry entry)
        {
            return new RecordEntry
            {
                Id = entry.Id,
                Name = entry.Name,
                Type = entry.Type,
                CustomCategoryLabel = entry.CustomCategoryLabel,
                VehiclePlate = entry.VehiclePlate,
                Occupation = entry.Occupation,
                Notes = entry.Notes,
                Metadata = entry.Metadata
            };
        }

        private static RecordType ParseType(string raw, out string customCategory)
        {
            customCategory = null;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return RecordType.Civilian;
            }

            var normalized = raw.Trim().ToLowerInvariant();
            return normalized switch
            {
                "civilian worker" or "worker" => RecordType.CivilianWorker,
                "presidential" or "presidential staff" or "white house" => RecordType.PresidentialStaff,
                "minister" or "cabinet" => RecordType.Minister,
                "army" or "military" => RecordType.Military,
                "police officer" or "patrol" => RecordType.PoliceOfficer,
                "police" or "officer" or "leo" or "law enforcement" => RecordType.Officer,
                "fbi" or "federal agent" or "agent" => RecordType.FbiAgent,
                "secret service" or "usss" => RecordType.SecretService,
                "undercover" or "uc" => RecordType.Undercover,
                "prisoner" or "inmate" => RecordType.Prisoner,
                "vehicle" or "car" or "plate" => RecordType.Vehicle,
                _ => TryParseEnumOrCustom(raw, out customCategory)
            };
        }

        private static RecordType TryParseEnumOrCustom(string raw, out string customCategory)
        {
            customCategory = null;
            if (Enum.TryParse(raw, true, out RecordType parsed))
            {
                return parsed;
            }

            // Preserve the label for UI/radio while defaulting to civilian-type filtering.
            customCategory = raw?.Trim();
            return RecordType.Civilian;
        }
    }
}
