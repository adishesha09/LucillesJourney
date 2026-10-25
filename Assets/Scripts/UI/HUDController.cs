using System.Collections.Generic;
using TMPro;
using UnityEngine;
using LucillesJourney.Gamification.UI;

namespace LucillesJourney.UI
{
    /// <summary>
    /// Scene-facing handle for the gameplay HUD. The visible HUD is now JourneyHUD (inside the persistent
    /// JourneyUI), which reads the real progress data; this component keeps the original API so existing
    /// scenes and callers keep working, and hides the old text-only panel it used to drive.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text chapterTitleLabel;
        [SerializeField] private TMP_Text objectivesLabel;

        public void Wire(TMP_Text chapterTitle, TMP_Text objectives)
        {
            chapterTitleLabel = chapterTitle;
            objectivesLabel = objectives;
        }

        private void Start()
        {
            if (JourneyUI.Instance != null) gameObject.SetActive(false);
        }

        /// <summary>Asks the HUD to redraw from current progress.</summary>
        public void Refresh() => JourneyUI.Instance?.RefreshHud();

        // The title and objective list are derived from GameManager.CurrentChapterId and JourneyCatalog,
        // so these remain only for API compatibility and drive the legacy labels if JourneyUI is absent.
        public void SetChapterTitle(string title)
        {
            if (chapterTitleLabel != null) chapterTitleLabel.text = title;
        }

        public void SetObjectives(List<(string description, bool complete)> objectives)
        {
            if (objectivesLabel == null) return;
            var builder = new System.Text.StringBuilder();
            foreach (var (description, complete) in objectives)
                builder.Append(complete ? "Done: " : "To do: ").Append(description).Append('\n');
            objectivesLabel.text = builder.ToString().TrimEnd('\n');
        }
    }
}
