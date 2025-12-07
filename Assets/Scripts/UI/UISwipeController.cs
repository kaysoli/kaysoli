using System.Collections;
using UnityEngine;

namespace RadioDispatch.UI
{
    /// <summary>
    /// Basic slide-in/slide-out behaviour for the split-screen panel.
    /// </summary>
    public class UISwipeController : MonoBehaviour
    {
        [SerializeField]
        private RectTransform splitPanel;

        [SerializeField]
        private RectTransform hiddenAnchor;

        [SerializeField]
        private RectTransform visibleAnchor;

        [SerializeField]
        private float slideDuration = 0.25f;

        [SerializeField]
        private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Coroutine slideRoutine;
        private bool isVisible;

        /// <summary>
        /// Toggles the panel visibility with a sliding animation.
        /// </summary>
        public void ToggleSplit()
        {
            SetVisible(!isVisible);
        }

        /// <summary>
        /// Explicitly sets whether the split panel is visible.
        /// </summary>
        /// <param name="visible">Whether the panel should be shown.</param>
        public void SetVisible(bool visible)
        {
            if (splitPanel == null || hiddenAnchor == null || visibleAnchor == null)
            {
                Debug.LogWarning("UISwipeController is missing RectTransform references.");
                return;
            }

            if (slideRoutine != null)
            {
                StopCoroutine(slideRoutine);
            }

            slideRoutine = StartCoroutine(SlideTo(visible));
        }

        private IEnumerator SlideTo(bool visible)
        {
            isVisible = visible;
            var start = splitPanel.anchoredPosition;
            var end = visible ? visibleAnchor.anchoredPosition : hiddenAnchor.anchoredPosition;
            var elapsed = 0f;

            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                var t = slideCurve.Evaluate(Mathf.Clamp01(elapsed / slideDuration));
                splitPanel.anchoredPosition = Vector2.Lerp(start, end, t);
                yield return null;
            }

            splitPanel.anchoredPosition = end;
            slideRoutine = null;
        }
    }
}
