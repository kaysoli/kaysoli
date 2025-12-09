using System.Collections.Generic;
using UnityEngine;

namespace RadioDispatch.Calls
{
    /// <summary>
    /// Aggregates call templates so Phase 5 can load larger incident sets easily.
    /// </summary>
    [CreateAssetMenu(fileName = "CallLibrary", menuName = "RadioDispatch/Call Library")]
    public class CallLibrary : ScriptableObject
    {
        public List<CallTemplate> Templates = new();

        public List<CallTemplate> GetTemplates()
        {
            return Templates ?? new List<CallTemplate>();
        }
    }
}
