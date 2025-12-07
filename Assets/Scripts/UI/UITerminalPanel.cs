using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RadioDispatch.Calls;

namespace RadioDispatch.UI
{
    /// <summary>
    /// Manages the call list view within the split-screen terminal panel.
    /// </summary>
    public class UITerminalPanel : MonoBehaviour
    {
        [SerializeField]
        private CallManager callManager;

        [SerializeField]
        private Transform callListContainer;

        [SerializeField]
        private GameObject callRowPrefab;

        private readonly List<GameObject> pooledRows = new();
        private System.Action<CallData> refreshHandler;

        private void OnEnable()
        {
            Subscribe();
            RefreshCallList();
        }

        private void OnDisable()
        {
            Unsubscribe();
            ClearRows();
        }

        private void Subscribe()
        {
            if (callManager == null)
            {
                Debug.LogWarning("UITerminalPanel is missing a CallManager reference.");
                return;
            }

            refreshHandler = _ => RefreshCallList();
            CallManager.OnNewCall += refreshHandler;
            CallManager.OnCallResolved += refreshHandler;
            CallManager.OnCallUpdated += refreshHandler;
        }

        private void Unsubscribe()
        {
            if (refreshHandler != null)
            {
                CallManager.OnNewCall -= refreshHandler;
                CallManager.OnCallResolved -= refreshHandler;
                CallManager.OnCallUpdated -= refreshHandler;
            }

            refreshHandler = null;
        }

        /// <summary>
        /// Rebuilds the call list UI from the CallManager's active calls.
        /// </summary>
        public void RefreshCallList()
        {
            if (callManager == null || callListContainer == null || callRowPrefab == null)
            {
                return;
            }

            ClearRows();

            foreach (var call in callManager.GetActiveCalls())
            {
                if (call.IsResolved)
                {
                    continue;
                }

                var row = Instantiate(callRowPrefab, callListContainer);
                pooledRows.Add(row);

                var presenter = row.GetComponent<UICallRow>();
                var textOnly = row.GetComponentInChildren<Text>();
                if (presenter != null)
                {
                    presenter.Bind(call);
                }
                else if (textOnly != null)
                {
                    textOnly.text = UIDisplayFormatter.BuildCallSummary(call);
                }
            }
        }

        private void ClearRows()
        {
            foreach (var row in pooledRows)
            {
                if (row != null)
                {
                    Destroy(row);
                }
            }

            pooledRows.Clear();
        }
    }
}
