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
        /// Merges CSV or TSV content into the database. It accepts either a simple row format (Id,Name,Type,VehiclePlate,Notes,
        /// Metadata,Occupation) or the expanded spreadsheet-style header provided by the user (RECORD_ID ... IS_WITNESS_PROTECT
        /// ION). Columns are matched by header name so order is not important and unknown headers are ignored.
        /// </summary>
        /// <param name="csvAsset">Text asset containing comma, semicolon, or tab separated rows.</param>
        /// <param name="defaultSeparator">Column separator fallback, defaults to comma.</param>
        public void ImportCsv(TextAsset csvAsset, char defaultSeparator = ',')
        {
            if (csvAsset == null || string.IsNullOrWhiteSpace(csvAsset.text))
            {
                Debug.LogWarning("RecordsDatabase.ImportCsv received empty asset.");
                return;
            }

            using var reader = new StringReader(csvAsset.text);
            string line;
            // Skip comment/blank lines until we find a header or data row.
            do
            {
                line = reader.ReadLine();
            }
            while (line != null && (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")));

            if (line == null)
            {
                return;
            }

            // Detect separator (tab-aware) and build header map if a spreadsheet header is present.
            var detectedSeparator = DetectSeparator(line, defaultSeparator);
            var headerParts = line.Split(detectedSeparator);
            var headerMap = BuildHeaderMap(headerParts);
            var hasVerboseHeader = headerMap.Count > 0;

            // If this was a header, advance to the first data line; if not, process the current line as data.
            if (!hasVerboseHeader)
            {
                // Process the line using the simple positional parser.
                ParseSimpleRow(line.Split(detectedSeparator));
            }

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue; // Skip comments and blanks.
                }

                var parts = line.Split(detectedSeparator);
                if (hasVerboseHeader)
                {
                    ParseVerboseRow(parts, headerMap);
                }
                else
                {
                    ParseSimpleRow(parts);
                }
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
                LastName = entry.LastName,
                FirstName = entry.FirstName,
                MiddleInitial = entry.MiddleInitial,
                NameSuffix = entry.NameSuffix,
                Type = entry.Type,
                CustomCategoryLabel = entry.CustomCategoryLabel,
                VehiclePlate = entry.VehiclePlate,
                Occupation = entry.Occupation,
                Employer = entry.Employer,
                RecordStatus = entry.RecordStatus,
                PoliceCautions = entry.PoliceCautions,
                ActiveWarrants = entry.ActiveWarrants,
                BoloStatus = entry.BoloStatus,
                IsArmed = entry.IsArmed,
                HasSecurityClearance = entry.HasSecurityClearance,
                IsWitnessProtection = entry.IsWitnessProtection,
                Aliases = entry.Aliases,
                KnownAssociates = entry.KnownAssociates,
                LocalPoliceContacts = entry.LocalPoliceContacts,
                CriminalHistory = entry.CriminalHistory,
                ProtectiveOrders = entry.ProtectiveOrders,
                Notes = entry.Notes,
                Metadata = entry.Metadata,
                Timestamp = entry.Timestamp,
                LastUpdated = entry.LastUpdated,
                DataVersion = entry.DataVersion,
                Sex = entry.Sex,
                Race = entry.Race,
                EyeColor = entry.EyeColor,
                HairColor = entry.HairColor,
                SkinTone = entry.SkinTone,
                DistinguishingMarks = entry.DistinguishingMarks,
                Height = entry.Height,
                Weight = entry.Weight,
                DateOfBirth = entry.DateOfBirth,
                PlaceOfBirth = entry.PlaceOfBirth,
                DlNumber = entry.DlNumber,
                DlState = entry.DlState,
                DlClass = entry.DlClass,
                DlExpiration = entry.DlExpiration,
                DlStatus = entry.DlStatus,
                StateIdNumber = entry.StateIdNumber,
                StateIdState = entry.StateIdState,
                StateIdExpiration = entry.StateIdExpiration,
                PassportNumber = entry.PassportNumber,
                PassportCountry = entry.PassportCountry,
                PassportExpiration = entry.PassportExpiration,
                Ssn = entry.Ssn,
                MilitaryId = entry.MilitaryId,
                GovernmentEmployeeId = entry.GovernmentEmployeeId,
                Address = entry.Address,
                AddressVerifiedDate = entry.AddressVerifiedDate,
                EducationLevel = entry.EducationLevel,
                FbiNumber = entry.FbiNumber,
                StateIdSid = entry.StateIdSid,
                LocalCid = entry.LocalCid,
                RegisteredVehicles = entry.RegisteredVehicles,
                RegisteredFirearms = entry.RegisteredFirearms,
                RegisteredCustody = entry.RegisteredCustody,
                AdditionalFlags = entry.AdditionalFlags
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

        /// <summary>
        /// Simple positional parser for legacy CSV rows.
        /// </summary>
        private void ParseSimpleRow(IReadOnlyList<string> parts)
        {
            if (parts.Count < 2)
            {
                Debug.LogWarning("RecordsDatabase.ImportCsv skipping malformed row without Id/Name.");
                return;
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

        /// <summary>
        /// Parser for the verbose tabular header the user requested. Header matching is case-insensitive and ignores unknown
        /// columns so the sheet can evolve without breaking imports.
        /// </summary>
        private void ParseVerboseRow(IReadOnlyList<string> parts, Dictionary<string, int> headerMap)
        {
            string Get(string header)
            {
                return headerMap.TryGetValue(header, out var index) && index < parts.Count
                    ? parts[index].Trim()
                    : null;
            }

            var entry = new RecordEntry
            {
                Id = Get("RECORD_ID") ?? Get("ID"),
                Timestamp = Get("TIMESTAMP"),
                DataVersion = Get("DATA_VERSION"),
                RecordStatus = Get("RECORD_STATUS"),
                LastUpdated = Get("LAST_UPDATED"),
                LastName = Get("LAST_NAME"),
                FirstName = Get("FIRST_NAME"),
                MiddleInitial = Get("MIDDLE_INITIAL"),
                NameSuffix = Get("NAME_SUFFIX"),
                Sex = Get("SEX"),
                Race = Get("RACE"),
                Height = Get("HEIGHT"),
                Weight = Get("WEIGHT"),
                DateOfBirth = Get("DATE_OF_BIRTH"),
                PlaceOfBirth = Get("PLACE_OF_BIRTH"),
                EyeColor = Get("EYE_COLOR"),
                HairColor = Get("HAIR_COLOR"),
                SkinTone = Get("SKIN_TONE"),
                DistinguishingMarks = Get("DISTINGUISHING_MARKS"),
                DlNumber = Get("DL_NUMBER"),
                DlState = Get("DL_STATE"),
                DlClass = Get("DL_CLASS"),
                DlExpiration = Get("DL_EXPIRATION"),
                DlStatus = Get("DL_STATUS"),
                StateIdNumber = Get("STATE_ID_NUMBER"),
                StateIdState = Get("STATE_ID_STATE"),
                StateIdExpiration = Get("STATE_ID_EXPIRATION"),
                PassportNumber = Get("PASSPORT_NUMBER"),
                PassportCountry = Get("PASSPORT_COUNTRY"),
                PassportExpiration = Get("PASSPORT_EXPIRATION"),
                Ssn = Get("SSN"),
                MilitaryId = Get("MILITARY_ID"),
                GovernmentEmployeeId = Get("GOVERNMENT_EMP_ID"),
                Address = Get("ADDRESS"),
                AddressVerifiedDate = Get("ADDRESS_VERIFIED_DATE"),
                Occupation = Get("OCCUPATION"),
                Employer = Get("EMPLOYER"),
                EducationLevel = Get("EDUCATION_LEVEL"),
                FbiNumber = Get("FBI_NUMBER"),
                StateIdSid = Get("STATE_ID_SID"),
                LocalCid = Get("LOCAL_CID"),
                Aliases = Get("ALIASES"),
                PoliceCautions = Get("POLICE_CAUTIONS"),
                ActiveWarrants = Get("ACTIVE_WARRANTS"),
                CriminalHistory = Get("CRIMINAL_HISTORY"),
                ProtectiveOrders = Get("PROTECTIVE_ORDERS"),
                RegisteredVehicles = Get("REGISTERED_VEHICLES"),
                RegisteredFirearms = Get("REGISTERED_FIREARMS"),
                KnownAssociates = Get("KNOWN_ASSOCIATES"),
                LocalPoliceContacts = Get("LOCAL_POLICE_CONTACTS"),
                BoloStatus = Get("BOLO_STATUS"),
                IsArmed = Get("IS_ARMED"),
                HasSecurityClearance = Get("HAS_SECURITY_CLEARANCE"),
                IsWitnessProtection = Get("IS_WITNESS_PROTECTION"),
                VehiclePlate = Get("VEHICLE_PLATE") ?? Get("PLATE"),
                Notes = Get("NOTES") ?? Get("CRIMINAL_HISTORY"),
                Metadata = Get("METADATA"),
                AdditionalFlags = Get("FLAGS")
            };

            // Compose a full display name if split fields are present.
            var composedName = ComposeName(entry.FirstName, entry.MiddleInitial, entry.LastName, entry.NameSuffix);
            entry.Name = !string.IsNullOrWhiteSpace(composedName) ? composedName : Get("NAME");

            // Type may come from a dedicated column or from a custom label.
            entry.Type = ParseType(Get("TYPE") ?? Get("RECORD_TYPE") ?? Get("CATEGORY"), out var customCategory);
            entry.CustomCategoryLabel = string.IsNullOrWhiteSpace(customCategory) ? Get("CUSTOM_TYPE") : customCategory;

            AddOrReplace(entry);
        }

        private static string ComposeName(string first, string middle, string last, string suffix)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(first)) parts.Add(first.Trim());
            if (!string.IsNullOrWhiteSpace(middle)) parts.Add(middle.Trim());
            if (!string.IsNullOrWhiteSpace(last)) parts.Add(last.Trim());

            var name = string.Join(" ", parts);
            if (!string.IsNullOrWhiteSpace(suffix))
            {
                name = string.IsNullOrWhiteSpace(name) ? suffix.Trim() : $"{name} {suffix.Trim()}";
            }

            return name;
        }

        private static Dictionary<string, int> BuildHeaderMap(IEnumerable<string> headers)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var index = 0;
            foreach (var header in headers)
            {
                var key = header.Trim();
                if (string.IsNullOrWhiteSpace(key))
                {
                    index++;
                    continue;
                }

                map[key.ToUpperInvariant()] = index;
                index++;
            }

            // Consider it a verbose header if it contains the expected RECORD_ID column.
            if (!map.ContainsKey("RECORD_ID") && !map.ContainsKey("ID"))
            {
                map.Clear();
            }

            return map;
        }

        private static char DetectSeparator(string headerLine, char defaultSeparator)
        {
            if (headerLine.Contains('\t'))
            {
                return '\t';
            }

            if (headerLine.Contains(';'))
            {
                return ';';
            }

            return defaultSeparator;
        }
    }
}
