using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RadioDispatch.Units
{
    /// <summary>
    /// ScriptableObject used to author default rosters for content-heavy phases.
    /// Supports both inline unit definitions and reusable unit definition assets with crew assignments.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitRoster", menuName = "RadioDispatch/Unit Roster")]
    public class UnitRoster : ScriptableObject
    {
        [Tooltip("Inline units authored directly on this asset.")]
        public List<Unit> Units = new();

        [Tooltip("Optional list of reusable unit definition assets.")]
        public List<UnitDefinition> UnitDefinitions = new();

        /// <summary>
        /// Returns deep copies so runtime modifications don't alter the asset.
        /// </summary>
        public List<Unit> GetClonedUnits()
        {
            var cloned = Units?.Select(CloneUnit).ToList() ?? new List<Unit>();

            if (UnitDefinitions != null)
            {
                foreach (var def in UnitDefinitions)
                {
                    if (def != null)
                    {
                        cloned.Add(def.CreateInstance());
                    }
                }
            }

            return cloned;
        }

        private static Unit CloneUnit(Unit source)
        {
            if (source == null)
            {
                return null;
            }

            return new Unit
            {
                Id = source.Id,
                CallSign = source.CallSign,
                DisplayName = source.DisplayName,
                Status = source.Status,
                CurrentZone = source.CurrentZone,
                CurrentCall = null,
                Type = source.Type,
                Department = source.Department,
                AcknowledgementLabel = source.AcknowledgementLabel,
                VoiceProfile = source.VoiceProfile,
                Crew = source.Crew != null ? new List<OfficerProfile>(source.Crew) : new List<OfficerProfile>()
            };
        }
    }
}
