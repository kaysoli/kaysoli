using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RadioDispatch.Units;

namespace RadioDispatch.Simulation
{
    /// <summary>
    /// Generates large rosters of units from rosters, CSV sheets, or procedural rules so designers can simulate dense cities.
    /// </summary>
    [DisallowMultipleComponent]
    public class UnitGenesisPool : MonoBehaviour
    {
        [Header("Sources")]
        [Tooltip("If assigned, units from this roster are cloned into the simulation before procedural generation kicks in.")]
        [SerializeField]
        private UnitRoster roster;

        [Tooltip("Optional CSV sheets (Id,Name,Type,Status,Zone,CallSign,VoiceProfile) that get merged on load. Lines starting with # are ignored.")]
        [SerializeField]
        private List<TextAsset> csvSheets = new();

        [Header("Generation")]
        [Tooltip("Unit manager that will receive the generated pool. If null, generation still returns the list for manual injection.")]
        [SerializeField]
        private UnitManager unitManager;

        [Tooltip("Minimum number of units to keep in the pool after applying rosters/CSVs.")]
        [SerializeField]
        private int minimumUnitCount = 100;

        [Tooltip("Automatically generate and push a roster to the UnitManager during Awake.")]
        [SerializeField]
        private bool generateOnAwake = true;

        private static readonly string[] PatrolPhonetics = { "Adam", "Boy", "Charles", "David", "Edward", "Frank" };

        private void Awake()
        {
            if (generateOnAwake)
            {
                GenerateAndApply();
            }
        }

        /// <summary>
        /// Builds a composite roster from the configured sources and pushes it to the UnitManager if present.
        /// </summary>
        /// <returns>List of generated units so callers can inspect or override.</returns>
        public List<Unit> GenerateAndApply()
        {
            var generated = new List<Unit>();
            generated.AddRange(LoadFromRoster());
            generated.AddRange(LoadFromCsv());

            if (generated.Count < minimumUnitCount)
            {
                generated.AddRange(GenerateProcedural(minimumUnitCount - generated.Count));
            }

            if (unitManager != null)
            {
                unitManager.SetUnits(generated);
            }

            return generated;
        }

        /// <summary>
        /// Generates a batch of random units inspired by modern call sign formats (e.g., 3A23, Air-5).
        /// </summary>
        /// <param name="count">Number of units to create.</param>
        public List<Unit> GenerateProcedural(int count)
        {
            var list = new List<Unit>();
            var random = new System.Random();

            for (int i = 0; i < count; i++)
            {
                var district = random.Next(1, 9); // 1-8 divisions
                var shift = random.Next(1, 4); // 1-3 shifts
                var beat = PatrolPhonetics[random.Next(PatrolPhonetics.Length)];
                var car = random.Next(10, 99);
                var id = $"{district}{beat[0]}{car}";

                list.Add(new Unit
                {
                    Id = id,
                    CallSign = $"{district}{beat}{car}",
                    DisplayName = $"{district}{beat}{car}",
                    Status = UnitStatus.Available,
                    CurrentZone = $"Zone {district}-{shift}",
                    Type = UnitType.Patrol
                });
            }

            return list;
        }

        /// <summary>
        /// Clones units from the configured roster so the generator is non-destructive.
        /// </summary>
        private IEnumerable<Unit> LoadFromRoster()
        {
            if (roster == null || roster.Units == null)
            {
                return Enumerable.Empty<Unit>();
            }

            return roster.GetClonedUnits();
        }

        /// <summary>
        /// Parses CSV files so designers can bulk-inject units without recompiling.
        /// </summary>
        private IEnumerable<Unit> LoadFromCsv()
        {
            var list = new List<Unit>();
            if (csvSheets == null)
            {
                return list;
            }

            foreach (var sheet in csvSheets)
            {
                if (sheet == null || string.IsNullOrWhiteSpace(sheet.text))
                {
                    continue;
                }

                var lines = sheet.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var columns = line.Split(',');
                    if (columns.Length < 3)
                    {
                        Debug.LogWarning($"Unit CSV row skipped because it has fewer than 3 columns: {line}");
                        continue;
                    }

                    var unit = new Unit
                    {
                        Id = columns[0].Trim(),
                        DisplayName = columns[1].Trim(),
                        CallSign = columns.Length > 5 ? columns[5].Trim() : columns[0].Trim(),
                        Type = ParseUnitType(columns.Length > 2 ? columns[2].Trim() : string.Empty),
                        Status = ParseUnitStatus(columns.Length > 3 ? columns[3].Trim() : string.Empty),
                        CurrentZone = columns.Length > 4 ? columns[4].Trim() : string.Empty
                    };

                    list.Add(unit);
                }
            }

            return list;
        }

        private static UnitType ParseUnitType(string value)
        {
            if (Enum.TryParse<UnitType>(value, true, out var parsed))
            {
                return parsed;
            }

            return UnitType.Unknown;
        }

        private static UnitStatus ParseUnitStatus(string value)
        {
            if (Enum.TryParse<UnitStatus>(value, true, out var parsed))
            {
                return parsed;
            }

            return UnitStatus.Available;
        }
    }
}
