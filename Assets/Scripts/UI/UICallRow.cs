using UnityEngine;
using UnityEngine.UI;
using RadioDispatch.Calls;

namespace RadioDispatch.UI
{
    /// <summary>
    /// Simple presenter for a call row prefab. Supports binding data to text labels.
    /// </summary>
    public class UICallRow : MonoBehaviour
    {
        [SerializeField]
        private Text idLabel;

        [SerializeField]
        private Text titleLabel;

        [SerializeField]
        private Text priorityLabel;

        [SerializeField]
        private Text locationLabel;

        [SerializeField]
        private Text timerLabel;

        [SerializeField]
        private Text assignedUnitsLabel;

        /// <summary>
        /// Binds UI fields to a call instance.
        /// </summary>
        /// <param name="call">Call data to display.</param>
        public void Bind(CallData call)
        {
            if (call == null)
            {
                Debug.LogWarning("UICallRow received null call data.");
                return;
            }

            if (idLabel != null)
            {
                idLabel.text = string.IsNullOrWhiteSpace(call.Id) ? "--" : call.Id;
            }

            if (titleLabel != null)
            {
                titleLabel.text = string.IsNullOrWhiteSpace(call.Title) ? "Pending" : call.Title;
            }

            if (priorityLabel != null)
            {
                priorityLabel.text = call.Priority.ToString();
            }

            if (locationLabel != null)
            {
                locationLabel.text = string.IsNullOrWhiteSpace(call.Location) ? "Unknown" : call.Location;
            }

            if (timerLabel != null)
            {
                timerLabel.text = System.TimeSpan.FromSeconds(Mathf.Max(call.TimeSinceCreated, 0f)).ToString("mm\\:ss");
            }

            if (assignedUnitsLabel != null)
            {
                assignedUnitsLabel.text = call.AssignedUnits?.Count.ToString() ?? "0";
            }
        }
    }
}
