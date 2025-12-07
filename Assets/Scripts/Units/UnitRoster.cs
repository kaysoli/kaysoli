using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RadioDispatch.Units
{
    /// <summary>
    /// ScriptableObject used to author default rosters for content-heavy phases.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitRoster", menuName = "RadioDispatch/Unit Roster")]
    public class UnitRoster : ScriptableObject
    {
        public List<Unit> Units = new();

        /// <summary>
        /// Returns deep copies so runtime modifications don't alter the asset.
        /// </summary>
        public List<Unit> GetClonedUnits()
        {
            return Units?.Select(CloneUnit).ToList() ?? new List<Unit>();
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
                AcknowledgementLabel = source.AcknowledgementLabel,
                VoiceProfile = source.VoiceProfile
            };
        }
    }
}
