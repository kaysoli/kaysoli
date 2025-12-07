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
        /// Merges CSV content into the database. Columns: Id,Name,Type,VehiclePlate,Notes,Metadata
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
                    Type = ParseType(parts.ElementAtOrDefault(2)),
                    VehiclePlate = parts.ElementAtOrDefault(3)?.Trim(),
                    Notes = parts.ElementAtOrDefault(4)?.Trim(),
                    Metadata = parts.ElementAtOrDefault(5)?.Trim(),
                };

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
                VehiclePlate = entry.VehiclePlate,
                Notes = entry.Notes,
                Metadata = entry.Metadata
            };
        }

        private static RecordType ParseType(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return RecordType.Civilian;
            }

            return Enum.TryParse(raw, true, out RecordType parsed) ? parsed : RecordType.Civilian;
        }
    }
}
