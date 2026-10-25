using TMPro;
using UnityEngine;
using LucillesJourney.Interaction;

namespace LucillesJourney.UI
{
    /// <summary>
    /// Shows "[E] Water Garden" when an interactable is in range and hides it otherwise.
    /// The key hint is text (not colour-only) so it is accessible.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private InteractionDetector detector;
        [SerializeField] private TMP_Text promptLabel;
        [SerializeField] private string keyHint = "E";

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            Hide();

            if (detector == null) Debug.LogError("InteractionPromptUI: detector is not assigned.", this);
            if (promptLabel == null) Debug.LogError("InteractionPromptUI: promptLabel is not assigned.", this);
        }

        /// <summary>Used by the editor scene builder to assign references created at build time.</summary>
        public void Wire(InteractionDetector playerDetector, TMP_Text label, string key = "E")
        {
            if (detector != null) detector.FocusChanged -= HandleFocusChanged;
            detector = playerDetector;
            promptLabel = label;
            keyHint = key;
            if (detector != null) detector.FocusChanged += HandleFocusChanged;
        }

        private void OnEnable()
        {
            if (detector != null) detector.FocusChanged += HandleFocusChanged;
        }

        private void OnDisable()
        {
            if (detector != null) detector.FocusChanged -= HandleFocusChanged;
        }

        private void HandleFocusChanged(Interactable target)
        {
            if (target == null || promptLabel == null)
            {
                Hide();
                return;
            }
            promptLabel.text = $"[{keyHint}]  {target.PromptText}";
            canvasGroup.alpha = 1f;
        }

        private void Hide() => canvasGroup.alpha = 0f;
    }
}